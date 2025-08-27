using Faryma.Composer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,
        TrackArtistRepository artistRepository,
        ComposerStreamRepository composerStreamRepository,
        ReviewOrderRepository reviewOrderRepository,
        ReviewRepository reviewRepository,
        TrackRepository trackRepository,
        TransactionRepository transactionRepository,
        UserAccountRepository userAccountRepository,
        UserNicknameRepository userNicknameRepository,
        UserRepository userRepository,
        UserTrackRatingRepository userTrackRatingRepository)
    {
        public ComposerStreamRepository ComposerStreamRepository { get; } = composerStreamRepository;
        public ReviewRepository ReviewRepository { get; } = reviewRepository;
        public ReviewOrderRepository ReviewOrderRepository { get; } = reviewOrderRepository;
        public TrackRepository TrackRepository { get; } = trackRepository;
        public TrackArtistRepository TrackArtistRepository { get; } = artistRepository;
        public TransactionRepository TransactionRepository { get; } = transactionRepository;
        public UserRepository UserRepository { get; } = userRepository;
        public UserAccountRepository UserAccountRepository { get; } = userAccountRepository;
        public UserNicknameRepository UserNicknameRepository { get; } = userNicknameRepository;
        public UserTrackRatingRepository UserTrackRatingRepository { get; } = userTrackRatingRepository;

        public Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default) => context.Database.BeginTransactionAsync(cancellationToken);
        public Task<int> SaveChanges(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => context.Remove(entity);
    }
}