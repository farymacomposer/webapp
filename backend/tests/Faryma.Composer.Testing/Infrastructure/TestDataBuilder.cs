using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Infrastructure.Features.UserNickname;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Testing.Infrastructure
{
    /// <summary>
    /// Готовит тестовые данные для сценариев application- и API-слоя.
    /// </summary>
    public sealed class TestDataBuilder(IServiceProvider services)
    {
        private const string _defaultTrackUrl = "https://example.com/track";
        private int _streamSequence;

        /// <summary>
        /// Создает пользователя для проверки сценариев, завязанных на автора действий.
        /// </summary>
        public Task<UserEntity> CreateUserAsync(string? userName = null) => services.RunInScopeAsync(async scoped =>
        {
            UserManager<UserEntity> userManager = scoped.GetRequiredService<UserManager<UserEntity>>();
            DateTimeContext clock = scoped.GetRequiredService<DateTimeContext>();
            string actualUserName = userName ?? $"user-{Guid.NewGuid():N}";

            UserEntity user = new()
            {
                Id = Guid.NewGuid(),
                UserName = actualUserName,
                Email = $"{actualUserName}@example.com",
                CreatedAt = clock.Now,
            };

            IdentityResult result = await userManager.CreateAsync(user);
            Assert.True(
                result.Succeeded,
                $"Не удалось создать тестового пользователя: {string.Join(", ", result.Errors.Select(x => x.Description))}");

            return user;
        });

        /// <summary>
        /// Создает стрим с нужным состоянием для подготовки проверки.
        /// </summary>
        public Task<ComposerStreamEntity> CreateStreamAsync(
            Guid? createdByUserId = null,
            DateOnly? eventDate = null,
            ComposerStreamType type = ComposerStreamType.Donation,
            ComposerStreamStatus status = ComposerStreamStatus.Planned,
            DateTime? startedAt = null,
            DateTime? completedAt = null) =>
            services.RunInScopeAsync(async scoped =>
            {
                ComposerStreamStore composerStreamStore = scoped.GetRequiredService<ComposerStreamStore>();
                UserManager<UserEntity> userManager = scoped.GetRequiredService<UserManager<UserEntity>>();
                DateTimeContext clock = scoped.GetRequiredService<DateTimeContext>();
                AppDbContext db = scoped.GetRequiredService<AppDbContext>();

                UserEntity createdByUser = await GetOrCreateUserAsync(userManager, clock.Now, createdByUserId, "stream");
                ComposerStreamEntity stream = composerStreamStore.CreateStream(
                    eventDate ?? GetNextStreamDate(clock.Today),
                    type,
                    createdByUser);

                stream.Status = status;
                stream.StartedAt = startedAt;
                stream.CompletedAt = completedAt;

                await db.SaveChangesAsync();

                return stream;
            });

        /// <summary>
        /// Создает заказ с заданными атрибутами, чтобы проверить нужный бизнес-сценарий.
        /// </summary>
        public Task<ReviewOrderEntity> CreateReviewOrderAsync(
            Guid? createdByUserId = null,
            long? creationStreamId = null,
            long? processingStreamId = null,
            string nickname = "nickname",
            ReviewOrderType type = ReviewOrderType.Donation,
            ReviewOrderStatus status = ReviewOrderStatus.Pending,
            bool isFrozen = false,
            string? trackUrl = null,
            int? trackDurationSeconds = null,
            int nominalPrice = 1_000,
            int payableAmount = 1_000,
            long totalPaymentAmount = 0,
            QueueCategory queueCategory = QueueCategory.Unspecified,
            DateTime? inProgressAt = null,
            DateTime? completedAt = null,
            DateTime? canceledAt = null,
            string? cancelReason = null,
            int? reviewRating = null) =>
            services.RunInScopeAsync(async scoped =>
            {
                ComposerStreamStore composerStreamStore = scoped.GetRequiredService<ComposerStreamStore>();
                ReviewOrderStore reviewOrderStore = scoped.GetRequiredService<ReviewOrderStore>();
                ReviewStore reviewStore = scoped.GetRequiredService<ReviewStore>();
                TransactionStore transactionStore = scoped.GetRequiredService<TransactionStore>();
                UserNicknameStore userNicknameStore = scoped.GetRequiredService<UserNicknameStore>();
                UserManager<UserEntity> userManager = scoped.GetRequiredService<UserManager<UserEntity>>();
                DateTimeContext clock = scoped.GetRequiredService<DateTimeContext>();
                AppDbContext db = scoped.GetRequiredService<AppDbContext>();

                UserEntity createdByUser = await GetOrCreateUserAsync(userManager, clock.Now, createdByUserId, "order");
                ComposerStreamEntity creationStream = await GetOrCreateCreationStreamAsync(
                    composerStreamStore,
                    db,
                    createdByUser,
                    clock.Today,
                    creationStreamId);
                ComposerStreamEntity? processingStream = await GetOrCreateProcessingStreamAsync(
                    composerStreamStore,
                    db,
                    createdByUser,
                    clock,
                    processingStreamId,
                    status);
                UserNicknameEntity userNickname = await GetOrCreateNicknameAsync(userNicknameStore, db, nickname);

                string? initialTrackUrl = trackUrl ?? (status == ReviewOrderStatus.Preorder ? null : _defaultTrackUrl);
                int? initialTrackDurationSeconds = initialTrackUrl is null ? null : trackDurationSeconds ?? 60;
                int initialPayableAmount = (type is ReviewOrderType.Donation or ReviewOrderType.Free)
                    ? payableAmount
                    : 0;

                ReviewOrderEntity order = reviewOrderStore.CreateOrder(
                    type,
                    status,
                    initialTrackUrl,
                    initialTrackDurationSeconds,
                    nominalPrice,
                    initialPayableAmount,
                    userComment: "тестовый комментарий",
                    creationStream,
                    userNickname,
                    createdByUser);

                order.IsFrozen = isFrozen;
                order.QueueCategory = queueCategory;
                order.ProcessingStream = processingStream;
                order.InProgressAt = inProgressAt;
                order.CompletedAt = completedAt;
                order.CanceledAt = canceledAt;
                order.CancelReason = cancelReason;

                if (status == ReviewOrderStatus.Canceled)
                {
                    order.InProgressAt = null;
                    order.ProcessingStream = null;
                    order.QueueCategory = QueueCategory.Unspecified;
                }

                if (totalPaymentAmount > 0)
                {
                    transactionStore.CreateAccountTopUp(
                        AccountTopUpProvider.Manual,
                        totalPaymentAmount,
                        userNickname.Account,
                        createdByUser);

                    transactionStore.CreatePayment(
                        totalPaymentAmount,
                        userNickname.Account,
                        order);
                }

                if (reviewRating is not null)
                {
                    order.Review = reviewStore.CreateReview(order, reviewRating.Value, createdByUser);
                }

                await db.SaveChangesAsync();

                return order;
            });

        private static async Task<UserNicknameEntity> GetOrCreateNicknameAsync(
            UserNicknameStore userNicknameStore,
            AppDbContext db,
            string nickname)
        {
            UserNicknameEntity? existing = await userNicknameStore.FindByNickname(nickname, CancellationToken.None);
            if (existing is not null)
            {
                return existing;
            }

            UserNicknameEntity created = userNicknameStore.Create(nickname);
            await db.SaveChangesAsync();

            return created;
        }

        private static async Task<UserEntity> GetOrCreateUserAsync(
            UserManager<UserEntity> userManager,
            DateTime createdAt,
            Guid? userId,
            string prefix)
        {
            if (userId is Guid existingUserId)
            {
                return await userManager.Users.FirstAsync(x => x.Id == existingUserId);
            }

            string actualUserName = $"{prefix}-{Guid.NewGuid():N}";
            UserEntity user = new()
            {
                Id = Guid.NewGuid(),
                UserName = actualUserName,
                Email = $"{actualUserName}@example.com",
                CreatedAt = createdAt,
            };

            IdentityResult result = await userManager.CreateAsync(user);
            Assert.True(
                result.Succeeded,
                $"Не удалось создать тестового пользователя: {string.Join(", ", result.Errors.Select(x => x.Description))}");

            return user;
        }

        private async Task<ComposerStreamEntity> GetOrCreateCreationStreamAsync(
            ComposerStreamStore composerStreamStore,
            AppDbContext db,
            UserEntity createdByUser,
            DateOnly today,
            long? creationStreamId)
        {
            if (creationStreamId is long existingStreamId)
            {
                return await composerStreamStore.GetStream(existingStreamId, CancellationToken.None);
            }

            ComposerStreamEntity stream = composerStreamStore.CreateStream(
                GetNextStreamDate(today),
                ComposerStreamType.Donation,
                createdByUser);
            await db.SaveChangesAsync();

            return stream;
        }

        private async Task<ComposerStreamEntity?> GetOrCreateProcessingStreamAsync(
            ComposerStreamStore composerStreamStore,
            AppDbContext db,
            UserEntity createdByUser,
            DateTimeContext clock,
            long? processingStreamId,
            ReviewOrderStatus status)
        {
            if (processingStreamId is long existingStreamId)
            {
                return await composerStreamStore.GetStream(existingStreamId, CancellationToken.None);
            }

            if (status is not (ReviewOrderStatus.InProgress or ReviewOrderStatus.Completed))
            {
                return null;
            }

            ComposerStreamEntity stream = composerStreamStore.CreateStream(
                GetNextStreamDate(clock.Today),
                ComposerStreamType.Donation,
                createdByUser);
            stream.Status = ComposerStreamStatus.Live;
            stream.StartedAt = clock.Now;

            await db.SaveChangesAsync();

            return stream;
        }

        private DateOnly GetNextStreamDate(DateOnly today) => today.AddDays(Interlocked.Increment(ref _streamSequence) - 1);
    }
}
