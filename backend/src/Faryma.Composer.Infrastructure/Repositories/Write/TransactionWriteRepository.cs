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

            if (topUpProvider == AccountTopUpProvider.Unspecified)
            {
                throw new InvalidOperationException($"Недопустимый провайдер пополнения счета '{topUpProvider}'");
            }

            AccountTopUpEntity source = new()
            {
                CreatedAt = createdAt,
                Provider = topUpProvider,
                Account = account,
            };

            context.Add(source);

            return context.Add(new TransactionEntity
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.AccountTopUp,
                Account = account,
                Credit = amount,
                Debit = 0,
                Source = source,
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
                throw new InvalidOperationException($"Недопустимый источник платежа '{source.GetType().Name}'");
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

            TransactionReversalEntity source = new()
            {
                CreatedAt = createdAt,
                ReversedByUser = reversedByUser,
                ReversedTransaction = reversedTransaction,
            };

            context.Add(source);

            TransactionEntity result = new()
            {
                CreatedAt = createdAt,
                Kind = TransactionKind.Reversal,
                Credit = reversedTransaction.Debit,
                Debit = reversedTransaction.Credit,
                Account = reversedTransaction.Account,
                Source = source
            };

            context.Add(result);
            source.ReversalTransaction = result;

            return result;
        }
    }
}