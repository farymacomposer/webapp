using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Repositories.Write
{
    public sealed class TransactionWriteRepository(AppDbContext context)
    {
        public TransactionEntity CreateAccountTopUp(
            DateTime createdAt,
            AccountTopUpProvider topUpProvider,
            decimal amount,
            UserAccountEntity account)
        {
            AccountTopUpEntity topUp = new()
            {
                CreatedAt = createdAt,
                Provider = topUpProvider,
            };

            context.Add(topUp);

            return context.Add(new TransactionEntity
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.AccountTopUp,
                Direction = TransactionDirection.Credit,
                Account = account,
                Amount = amount,
                Source = topUp,
            }).Entity;
        }

        public TransactionEntity CreatePayment(
            DateTime createdAt,
            decimal amount,
            UserAccountEntity account,
            TransactionSourceEntity source)
        {
            if (source is not ReviewOrderEntity)
            {
                throw new InvalidOperationException("Недопустимый источник платежа");
            }

            return context.Add(new TransactionEntity
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.Payment,
                Direction = TransactionDirection.Debit,
                Account = account,
                Amount = amount,
                Source = source,
            }).Entity;
        }

        public TransactionEntity CreateReversal(DateTime createdAt, UserEntity reversedByUser, TransactionEntity reversedTransaction)
        {
            TransactionReversalEntity reversal = new()
            {
                CreatedAt = createdAt,
                ReversedByUser = reversedByUser,
                ReversedTransaction = reversedTransaction,
            };

            context.Add(reversal);

            TransactionEntity reversalTransaction = new()
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.Reversal,
                Direction = reversedTransaction.Direction == TransactionDirection.Debit
                    ? TransactionDirection.Credit
                    : TransactionDirection.Debit,
                Amount = reversedTransaction.Amount,
                Account = reversedTransaction.Account,
                Source = reversal
            };

            context.Add(reversalTransaction);
            reversal.ReversalTransaction = reversalTransaction;

            return reversalTransaction;
        }
    }
}