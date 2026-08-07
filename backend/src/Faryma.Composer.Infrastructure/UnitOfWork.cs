using Faryma.Composer.Infrastructure.Persistence.Stores;
using Microsoft.EntityFrameworkCore.Storage;

namespace Faryma.Composer.Infrastructure
{
    public sealed class UnitOfWork(
        AppDbContext context,
        DateTimeService dateTimeService,

        ComposerStreamStore composerStreamStore,
        RefreshTokenStore refreshTokenStore,
        ReviewStore reviewStore,
        ReviewOrderStore reviewOrderStore,
        TransactionStore transactionStore,
        UserEntitlementStore userEntitlementStore,
        UserNicknameStore userNicknameStore
        )
    {
        public AppDbContext Context { get; } = context;
        public DateTimeService DateTimeService { get; } = dateTimeService;

        public ComposerStreamStore ComposerStreamStore { get; } = composerStreamStore;
        public RefreshTokenStore RefreshTokenStore { get; } = refreshTokenStore;
        public ReviewStore ReviewStore { get; } = reviewStore;
        public ReviewOrderStore ReviewOrderStore { get; } = reviewOrderStore;
        public TransactionStore TransactionStore { get; } = transactionStore;
        public UserEntitlementStore UserEntitlementStore { get; } = userEntitlementStore;
        public UserNicknameStore UserNicknameStore { get; } = userNicknameStore;

        public Task<IDbContextTransaction> BeginTransaction(CancellationToken ct) => Context.Database.BeginTransactionAsync(ct);
        public Task<int> SaveChanges(CancellationToken ct = default) => Context.SaveChangesAsync(ct);
        public void Remove<TEntity>(TEntity entity) where TEntity : class => Context.Remove(entity);
    }
}
