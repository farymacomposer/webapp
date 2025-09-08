using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Repositories.ReadWrite
{
    public sealed class Transaction_RW_Repository(AppDbContext context)
    {
        public Transaction CreateDeposit(UserAccount account, decimal amount)
        {
            return context.Add(new Transaction
            {
                CreatedAt = DateTime.UtcNow,
                Type = TransactionType.Deposit,
                Account = account,
                Amount = amount,
            }).Entity;
        }

        public Transaction CreatePayment(UserAccount account, decimal amount)
        {
            return context.Add(new Transaction
            {
                CreatedAt = DateTime.UtcNow,
                Type = TransactionType.Payment,
                Account = account,
                Amount = amount,
            }).Entity;
        }
    }
}