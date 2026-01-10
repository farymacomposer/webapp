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
            long amount,
            UserAccountEntity account)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            AccountTopUpEntity topUp = new()
            {
                CreatedAt = createdAt,
                Provider = topUpProvider,
                Account = account,
            };

            context.Add(topUp);

            return context.Add(new TransactionEntity
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.AccountTopUp,
                Account = account,
                Credit = amount,
                Debit = 0,
                Source = topUp,
            }).Entity;
        }

        public TransactionEntity CreatePayment(
            DateTime createdAt,
            long amount,
            UserAccountEntity account,
            TransactionSourceEntity source)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            if (source is not ReviewOrderEntity)
            {
                throw new InvalidOperationException("Недопустимый источник платежа");
            }

            return context.Add(new TransactionEntity
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.Payment,
                Account = account,
                Credit = 0,
                Debit = amount,
                Source = source,
            }).Entity;
        }

        public TransactionEntity CreateReversal(DateTime createdAt, UserEntity reversedByUser, TransactionEntity reversedTransaction)
        {
            if (reversedTransaction.Kind == TransactionKind.Reversal)
            {
                throw new InvalidOperationException("Невозможно отменить транзакцию отмены");
            }

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
                Credit = reversedTransaction.Debit,
                Debit = reversedTransaction.Credit,
                Account = reversedTransaction.Account,
                Source = reversal
            };

            context.Add(reversalTransaction);
            reversal.ReversalTransaction = reversalTransaction;

            return reversalTransaction;
        }
    }
}