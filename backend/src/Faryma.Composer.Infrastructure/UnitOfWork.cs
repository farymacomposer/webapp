using Faryma.Composer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,
        ArtistRepository artistRepository,
        ComposerStreamRepository composerStreamRepository,
        GenreRepository genreRepository,
        ReviewOrderRepository reviewOrderRepository,
        ReviewRepository reviewRepository,
        TrackRepository trackRepository,
        TransactionRepository transactionRepository,
        UserAccountRepository userAccountRepository,
        UserNicknameRepository userNicknameRepository,
        UserRepository userRepository,
        UserTrackRatingRepository userTrackRatingRepository)
    {
        public ArtistRepository ArtistRepository { get; } = artistRepository;
        public ComposerStreamRepository ComposerStreamRepository { get; } = composerStreamRepository;
        public GenreRepository GenreRepository { get; } = genreRepository;
        public ReviewOrderRepository ReviewOrderRepository { get; } = reviewOrderRepository;
        public ReviewRepository ReviewRepository { get; } = reviewRepository;
        public TrackRepository TrackRepository { get; } = trackRepository;
        public TransactionRepository TransactionRepository { get; } = transactionRepository;
        public UserAccountRepository UserAccountRepository { get; } = userAccountRepository;
        public UserNicknameRepository UserNicknameRepository { get; } = userNicknameRepository;
        public UserRepository UserRepository { get; } = userRepository;
        public UserTrackRatingRepository UserTrackRatingRepository { get; } = userTrackRatingRepository;

        public Task<IDbContextTransaction> BeginTransaction() => context.Database.BeginTransactionAsync();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    }
}