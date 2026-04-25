using System.Diagnostics;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class TransactionStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public TransactionEntity CreateAccountTopUp(
            AccountTopUpProvider topUpProvider,
            long amount,
            UserNicknameAccountEntity account,
            UserEntity createdByUser)
        {
            AccountTopUpEntity source = new()
            {
                CreatedAt = dateTimeService.Now,
                Provider = topUpProvider,
                UserNicknameAccount = account,
                CreatedByUser = createdByUser,
            };

            context.Add(source);

            return context.Add(new TransactionEntity
            {
                CreatedAt = dateTimeService.Now,
                Kind = TransactionKind.AccountTopUp,
                UserNicknameAccount = account,
                Credit = amount,
                Debit = 0,
                TransactionSource = source,
            }).Entity;
        }

        public TransactionEntity CreatePayment(
            long amount,
            UserNicknameAccountEntity account,
            TransactionSourceEntity source)
        {
            if (source is not ReviewOrderEntity)
            {
                throw new UnreachableException($"Недопустимый источник платежа '{source.GetType().Name}'");
            }

            return context.Add(new TransactionEntity
            {
                CreatedAt = dateTimeService.Now,
                Kind = TransactionKind.Payment,
                UserNicknameAccount = account,
                Credit = 0,
                Debit = amount,
                TransactionSource = source,
            }).Entity;
        }

        public TransactionEntity CreateReversal(UserEntity createdByUser, TransactionEntity reversedTransaction)
        {
            if (reversedTransaction.Kind == TransactionKind.Reversal)
            {
                throw new UnreachableException("Невозможно отменить транзакцию отмены");
            }

            TransactionReversalEntity source = new()
            {
                CreatedAt = dateTimeService.Now,
                CreatedByUser = createdByUser,
                ReversedTransaction = reversedTransaction,
            };

            context.Add(source);

            TransactionEntity result = new()
            {
                CreatedAt = dateTimeService.Now,
                Kind = TransactionKind.Reversal,
                Credit = reversedTransaction.Debit,
                Debit = reversedTransaction.Credit,
                UserNicknameAccount = reversedTransaction.UserNicknameAccount,
                TransactionSource = source
            };

            context.Add(result);
            source.ReversalTransaction = result;

            return result;
        }
    }
}