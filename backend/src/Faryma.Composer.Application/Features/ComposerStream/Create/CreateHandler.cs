using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStream.Create
{
    public sealed class CreateHandler(
        UnitOfWork uow,
        UserManager<UserEntity> userManager,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CreateCommand command, CancellationToken ct = default)
        {
            try
            {
                UserEntity createdByUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == command.CreatedByUserId, ct)
                    ?? throw new ComposerStreamException($"Пользователь с id: {command.CreatedByUserId} не найден");

                ComposerStreamEntity stream = uow.ComposerStreamStore.Create(command.EventDate, command.Type, createdByUser);
                await uow.SaveChanges(ct);

                orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCreated);

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }
    }
}
