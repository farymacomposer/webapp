using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class TransactionWriteRepository(AppDbContext context)
    {
        public TransactionEntity CreateDeposit(UserAccountEntity account, decimal amount)
        {
            return context.Add(new TransactionEntity
            {
                CreatedAt = DateTime.UtcNow,
                Type = TransactionType.Deposit,
                Account = account,
                Amount = amount,
            }).Entity;
        }

        public TransactionEntity CreatePayment(UserAccountEntity account, decimal amount)
        {
            return context.Add(new TransactionEntity
            {
                CreatedAt = DateTime.UtcNow,
                Type = TransactionType.Payment,
                Account = account,
                Amount = amount,
            }).Entity;
        }
    }
}