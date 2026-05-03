using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Features.ReviewOrder.Pricing;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.AsyncContracts;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Application.Features.AppSettings;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Models;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class ReviewOrderPricingTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что стоимость дополнительной длительности считается по текущей серверной настройке.
        /// </summary>
        [Fact]
        public async Task CalculateExtraTimePaymentPricing_UsesConfiguredAmountPerSecond()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            ReviewOrderExtraTimePaymentPricing pricing = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 500);

                return services.GetRequiredService<ReviewOrderService>()
                    .CalculateExtraTimePaymentPricing(trackDurationSeconds: 420);
            });

            Assert.Equal(420, pricing.TrackDurationSeconds);
            Assert.Equal(300, pricing.IncludedDurationSeconds);
            Assert.Equal(120, pricing.ExtraDurationSeconds);
            Assert.Equal(3, pricing.AmountPerSecond);
            Assert.Equal(360, pricing.Amount);
        }

        /// <summary>
        /// Проверяет, что длительность до пяти минут не создает доплату.
        /// </summary>
        [Theory]
        [InlineData(60)]
        [InlineData(300)]
        public async Task CalculateExtraTimePaymentPricing_ReturnsZeroAmount_WhenTrackFitsIncludedDuration(int trackDurationSeconds)
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            ReviewOrderExtraTimePaymentPricing pricing = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 500);

                return services.GetRequiredService<ReviewOrderService>()
                    .CalculateExtraTimePaymentPricing(trackDurationSeconds);
            });

            Assert.Equal(0, pricing.ExtraDurationSeconds);
            Assert.Equal(0, pricing.Amount);
        }

        /// <summary>
        /// Проверяет, что стоимость подробного разбора берется из текущей серверной настройки.
        /// </summary>
        [Fact]
        public async Task CalculateDetailedReviewPaymentAmount_UsesConfiguredAmount()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            long amount = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);

                return services.GetRequiredService<ReviewOrderService>()
                    .CalculateDetailedReviewPaymentAmount();
            });

            Assert.Equal(650, amount);
        }

        /// <summary>
        /// Проверяет, что обязательная стоимость существующего заказа берется из сохраненного snapshot.
        /// </summary>
        [Fact]
        public async Task CalculatePricing_UsesPayableAmountSnapshotAndCoverage()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                trackDurationSeconds: 420,
                nominalAmount: 1000,
                payableAmount: 1000,
                totalPaymentAmount: 900);

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 500);
                ReviewOrderEntity actualOrder = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderStore
                    .FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);
            });

            Assert.Equal(1000, pricing.RequiredAmount);
            Assert.Equal(900, pricing.CoveredAmount);
            Assert.Equal(900, pricing.PaidAmount);
            Assert.Equal(900, pricing.PaidPriorityAmount);
            Assert.False(pricing.IsRequiredCovered);
            Assert.Equal(
                [ReviewOrderPriceComponentKind.Nominal, ReviewOrderPriceComponentKind.ExtraTrackDuration],
                pricing.PriceComponents.Select(x => x.Kind).ToArray());
        }

        /// <summary>
        /// Проверяет, что созданный заказ сохраняет обязательную стоимость и не переоценивается после смены настроек.
        /// </summary>
        [Fact]
        public async Task CreateDonation_SnapshotsRequiredAmount_WhenSettingsChangeAfterCreation()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 5, detailedReviewAmount: 1_000);

                return await services.GetRequiredService<ReviewOrderService>()
                    .CreateDonation(new CreateDonationOrderCommand
                    {
                        Nickname = "Nick-CreateSnapshot",
                        TrackUrl = "https://example.com/create-snapshot",
                        TrackDurationSeconds = 420,
                        UserComment = null,
                        PaymentAmount = 1_600,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    });
            });

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 1, detailedReviewAmount: 1_000);
                ReviewOrderEntity actualOrder = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderStore
                    .FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);
            });

            Assert.Equal(ReviewOrderStatus.Pending, order.Status);
            Assert.Equal(1_600, order.PayableAmount);
            Assert.Equal(1_600, pricing.RequiredAmount);
            Assert.True(pricing.IsRequiredCovered);
        }

        /// <summary>
        /// Проверяет, что платежи за отдельные услуги не увеличивают сумму денежного приоритета заказа.
        /// </summary>
        [Fact]
        public async Task CalculatePricing_ExcludesOptionalServicePaymentsFromPaidPriorityAmount()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                trackDurationSeconds: 420,
                totalPaymentAmount: 750);

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);
                ReviewOrderService service = services.GetRequiredService<ReviewOrderService>();

                await service.PayDetailedReview(new PayDetailedReviewCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Detailed",
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                });

                ReviewOrderEntity actualOrder = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderStore
                    .FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);
            });

            Assert.Equal(750, pricing.CoveredAmount);
            Assert.Equal(750, pricing.PaidAmount);
            Assert.Equal(750, pricing.PaidPriorityAmount);
        }

        /// <summary>
        /// Проверяет, что жетон увеличивает покрытие, но не денежный приоритет.
        /// </summary>
        [Fact]
        public async Task CalculatePricing_IncludesTokenCoverageWithoutIncreasingPaidPriority()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                ReviewOrderEntity order = await services.GetRequiredService<ReviewOrderService>()
                    .CreateFree(new CreateFreeOrderCommand
                    {
                        Nickname = "Nick-TokenPricing",
                        TrackUrl = "https://example.com/token-pricing",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    });

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(order);
            });

            Assert.Equal(1_000, pricing.RequiredAmount);
            Assert.Equal(1_000, pricing.CoveredAmount);
            Assert.Equal(0, pricing.PaidAmount);
            Assert.Equal(0, pricing.PaidPriorityAmount);
            Assert.True(pricing.IsRequiredCovered);
        }

        /// <summary>
        /// Проверяет, что queue-запрос загружает погашения покрытий для расчета pricing.
        /// </summary>
        [Fact]
        public async Task GetOrdersInQueue_LoadsCoverageRedemptionsForPricing()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateFree(new CreateFreeOrderCommand
                {
                    Nickname = "Nick-QueueCoverage",
                    TrackUrl = "https://example.com/queue-coverage",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                ReviewOrderEntity loadedOrder = (await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderQueries
                    .GetOrdersInQueue())
                    .Single(x => x.Id == order.Id);

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(loadedOrder);
            });

            Assert.Equal(1_000, pricing.RequiredAmount);
            Assert.Equal(1_000, pricing.CoveredAmount);
            Assert.Equal(0, pricing.PaidAmount);
            Assert.Equal(0, pricing.PaidPriorityAmount);
            Assert.True(pricing.IsRequiredCovered);
        }

        /// <summary>
        /// Проверяет, что погашение жетона увеличивает покрытие, но не денежный приоритет и не суммируется с legacy-полем.
        /// </summary>
        [Fact]
        public async Task CalculatePricing_IncludesEntitlementRedemptionCoverageWithoutIncreasingPaidPriority()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                nickname: "Nick-EntitlementCoverage",
                nominalAmount: 1000,
                payableAmount: 1000,
                totalPaymentAmount: 400);

            ReviewOrderPricing pricing = await app.RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                UserEntity actualUser = await services.GetRequiredService<UserManager<UserEntity>>()
                    .FindByIdAsync(user.Id.ToString())
                    ?? throw new InvalidOperationException("Пользователь не найден");
                ReviewOrderEntity actualOrder = await uow.ReviewOrderStore.FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");
                UserNicknameEntity userNickname = await uow.UserNicknameStore.FindByNickname("Nick-EntitlementCoverage")
                    ?? throw new InvalidOperationException("Псевдоним не найден");

                actualOrder.NonPaymentCoverageAmount = 600;
                UserEntitlementEntity token = uow.UserEntitlementStore.CreateServiceToken(
                    userNickname,
                    UserEntitlementTarget.ReviewOrder,
                    actualUser);

                uow.UserEntitlementStore.Redeem(
                    token,
                    UserEntitlementTarget.ReviewOrder,
                    coveredAmount: 600,
                    actualUser,
                    reviewOrder: actualOrder);

                await uow.SaveChanges();

                actualOrder = await uow.ReviewOrderStore.FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");

                return services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);
            });

            Assert.Equal(1000, pricing.RequiredAmount);
            Assert.Equal(1000, pricing.CoveredAmount);
            Assert.Equal(400, pricing.PaidAmount);
            Assert.Equal(400, pricing.PaidPriorityAmount);
            Assert.True(pricing.IsRequiredCovered);
        }

        /// <summary>
        /// Проверяет, что DTO заказа явно получает checkout-суммы из рассчитанной модели pricing.
        /// </summary>
        [Fact]
        public async Task ReviewOrderDtoMap_IncludesCheckoutAmounts()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                trackDurationSeconds: 420,
                nominalAmount: 1000,
                payableAmount: 1000,
                totalPaymentAmount: 900);

            ReviewOrderDto dto = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 500);
                ReviewOrderEntity actualOrder = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderStore
                    .FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");
                ReviewOrderPricing pricing = services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);

                return ReviewOrderDto.Map(
                    actualOrder,
                    pricing.RequiredAmount,
                    pricing.CoveredAmount,
                    pricing.PaidAmount,
                    pricing.PaidPriorityAmount);
            });

            Assert.Equal(1000, dto.RequiredAmount);
            Assert.Equal(900, dto.CoveredAmount);
            Assert.Equal(900, dto.PaidAmount);
            Assert.Equal(900, dto.PaidPriorityAmount);
            Assert.Equal(dto.PaidPriorityAmount, dto.TotalAmount);
        }

        /// <summary>
        /// Проверяет, что REST-style DTO и queue DTO отдают одинаковые checkout-суммы.
        /// </summary>
        [Fact]
        public async Task OrderQueueSnapshotMessageMap_MatchesExplicitCheckoutAmounts()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                trackDurationSeconds: 420,
                nominalAmount: 1_000,
                payableAmount: 1_000,
                totalPaymentAmount: 900);

            (ReviewOrderDto restDto, ReviewOrderDto queueDto) = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 10, detailedReviewAmount: 1_000);
                ReviewOrderEntity actualOrder = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderStore
                    .FindById(order.Id)
                    ?? throw new InvalidOperationException("Заказ не найден");
                ReviewOrderPricing pricing = services.GetRequiredService<ReviewOrderPricingService>().Calculate(actualOrder);
                ReviewOrderDto explicitDto = ReviewOrderDto.Map(
                    actualOrder,
                    pricing.RequiredAmount,
                    pricing.CoveredAmount,
                    pricing.PaidAmount,
                    pricing.PaidPriorityAmount);
                OrderPosition position = OrderPosition.Create(actualOrder);
                position.UpdateCurrentPosition(0, OrderActivityStatus.Active);
                OrderQueueSnapshotMessage message = OrderQueueSnapshotMessage.Map(new OrderQueueSnapshot
                {
                    SyncVersion = 1,
                    OrderQueueUpdateType = OrderQueueUpdateType.Unspecified,
                    Positions = [position],
                });

                return (explicitDto, message.ActiveOrders.Single().Order);
            });

            Assert.Equal(restDto.RequiredAmount, queueDto.RequiredAmount);
            Assert.Equal(restDto.CoveredAmount, queueDto.CoveredAmount);
            Assert.Equal(restDto.PaidAmount, queueDto.PaidAmount);
            Assert.Equal(restDto.PaidPriorityAmount, queueDto.PaidPriorityAmount);
            Assert.Equal(restDto.TotalAmount, queueDto.TotalAmount);
        }

        private static async Task ConfigurePricing(
            IServiceProvider services,
            long extraTimeAmountPerSecond,
            long detailedReviewAmount)
        {
            AppSettingsService appSettingsService = services.GetRequiredService<AppSettingsService>();
            await appSettingsService.Update(new AppSettingsModel
            {
                ReviewOrderNominalAmount = appSettingsService.Settings.ReviewOrderNominalAmount,
                ReviewOrderExtraTimeAmountPerSecond = extraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = detailedReviewAmount,
            }, CancellationToken.None);
        }
    }
}
