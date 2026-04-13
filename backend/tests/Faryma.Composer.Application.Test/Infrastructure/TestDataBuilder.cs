using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Application.Test.Infrastructure
{
    public sealed class TestDataBuilder(ApplicationTestHost app)
    {
        private const string _defaultTrackUrl = "https://example.com/track";
        private int _streamSequence;

        public Task<UserEntity> CreateUserAsync(string? userName = null) => app.RunScopeAsync(async services =>
        {
            UserManager<UserEntity> userManager = services.GetRequiredService<UserManager<UserEntity>>();
            string actualUserName = userName ?? $"user-{Guid.NewGuid():N}";

            UserEntity user = new()
            {
                Id = Guid.NewGuid(),
                UserName = actualUserName,
                Email = $"{actualUserName}@example.com",
                CreatedAt = app.FixedNow,
            };

            IdentityResult result = await userManager.CreateAsync(user);
            Assert.True(
                result.Succeeded,
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(x => x.Description))}");

            return user;
        });

        public Task<ComposerStreamEntity> CreateStreamAsync(
            Guid? createdByUserId = null,
            DateOnly? eventDate = null,
            ComposerStreamType type = ComposerStreamType.Donation,
            ComposerStreamStatus status = ComposerStreamStatus.Planned,
            DateTime? startedAt = null,
            DateTime? completedAt = null) =>
            app.RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                UserManager<UserEntity> userManager = services.GetRequiredService<UserManager<UserEntity>>();

                UserEntity createdByUser = await GetOrCreateUserAsync(userManager, createdByUserId, "stream");
                ComposerStreamEntity stream = uow.ComposerStreamStore.Create(
                    eventDate ?? GetNextStreamDate(),
                    type,
                    createdByUser);

                stream.Status = status;
                stream.StartedAt = startedAt;
                stream.CompletedAt = completedAt;

                await uow.SaveChanges(CancellationToken.None);

                return stream;
            });

        public Task<ReviewOrderEntity> CreateReviewOrderAsync(
            Guid? createdByUserId = null,
            long? creationStreamId = null,
            long? processingStreamId = null,
            string nickname = "nickname",
            ReviewOrderType type = ReviewOrderType.Donation,
            ReviewOrderStatus status = ReviewOrderStatus.Pending,
            bool isFrozen = false,
            string? trackUrl = null,
            int nominalAmount = 750,
            int payableAmount = 750,
            long totalPaymentAmount = 0,
            QueueCategory queueCategory = QueueCategory.Unspecified,
            DateTime? inProgressAt = null,
            DateTime? completedAt = null,
            DateTime? canceledAt = null,
            string? cancelReason = null,
            int? reviewRating = null) =>
            app.RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                UserManager<UserEntity> userManager = services.GetRequiredService<UserManager<UserEntity>>();

                UserEntity createdByUser = await GetOrCreateUserAsync(userManager, createdByUserId, "order");
                ComposerStreamEntity creationStream = await GetOrCreateCreationStreamAsync(uow, createdByUser, creationStreamId);
                ComposerStreamEntity? processingStream = await GetOrCreateProcessingStreamAsync(uow, createdByUser, processingStreamId, status);
                UserNicknameEntity userNickname = await GetOrCreateNicknameAsync(uow, nickname);

                string? initialTrackUrl = trackUrl ?? (status == ReviewOrderStatus.Preorder ? null : _defaultTrackUrl);
                int initialPayableAmount = (type is ReviewOrderType.Donation or ReviewOrderType.Free)
                    ? payableAmount
                    : 0;

                ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                    nominalAmount,
                    initialPayableAmount,
                    initialTrackUrl,
                    userComment: "test-comment",
                    type,
                    creationStream,
                    userNickname,
                    createdByUser);

                order.Status = status;
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
                    uow.TransactionStore.CreateAccountTopUp(
                        AccountTopUpProvider.Manual,
                        totalPaymentAmount,
                        userNickname.Account,
                        createdByUser);

                    uow.TransactionStore.CreatePayment(
                        totalPaymentAmount,
                        userNickname.Account,
                        order);
                }

                if (reviewRating is not null)
                {
                    order.Review = uow.ReviewStore.Create(order, reviewRating.Value, createdByUser);
                }

                await uow.SaveChanges(CancellationToken.None);

                return order;
            });

        private static async Task<UserNicknameEntity> GetOrCreateNicknameAsync(UnitOfWork uow, string nickname)
        {
            UserNicknameEntity? existing = await uow.UserNicknameStore.FindByNickname(nickname, CancellationToken.None);
            if (existing is not null)
            {
                return existing;
            }

            UserNicknameEntity created = uow.UserNicknameStore.Create(nickname);
            await uow.SaveChanges(CancellationToken.None);

            return created;
        }

        private async Task<UserEntity> GetOrCreateUserAsync(
            UserManager<UserEntity> userManager,
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
                CreatedAt = app.FixedNow,
            };

            IdentityResult result = await userManager.CreateAsync(user);
            Assert.True(
                result.Succeeded,
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(x => x.Description))}");

            return user;
        }

        private async Task<ComposerStreamEntity> GetOrCreateCreationStreamAsync(
            UnitOfWork uow,
            UserEntity createdByUser,
            long? creationStreamId)
        {
            if (creationStreamId is long existingStreamId)
            {
                return await uow.ComposerStreamStore.FindById(existingStreamId, CancellationToken.None)
                    ?? throw new InvalidOperationException($"Creation stream {existingStreamId} not found.");
            }

            ComposerStreamEntity stream = uow.ComposerStreamStore.Create(GetNextStreamDate(), ComposerStreamType.Donation, createdByUser);
            await uow.SaveChanges(CancellationToken.None);

            return stream;
        }

        private async Task<ComposerStreamEntity?> GetOrCreateProcessingStreamAsync(
            UnitOfWork uow,
            UserEntity createdByUser,
            long? processingStreamId,
            ReviewOrderStatus status)
        {
            if (processingStreamId is long existingStreamId)
            {
                return await uow.ComposerStreamStore.FindById(existingStreamId, CancellationToken.None)
                    ?? throw new InvalidOperationException($"Processing stream {existingStreamId} not found.");
            }

            if (status is not (ReviewOrderStatus.InProgress or ReviewOrderStatus.Completed))
            {
                return null;
            }

            ComposerStreamEntity stream = uow.ComposerStreamStore.Create(GetNextStreamDate(), ComposerStreamType.Donation, createdByUser);
            stream.Status = ComposerStreamStatus.Live;
            stream.StartedAt = app.FixedNow;

            await uow.SaveChanges(CancellationToken.None);

            return stream;
        }

        private DateOnly GetNextStreamDate() => app.Today.AddDays(Interlocked.Increment(ref _streamSequence) - 1);
    }
}