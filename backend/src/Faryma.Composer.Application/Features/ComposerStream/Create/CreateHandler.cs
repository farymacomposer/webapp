using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Faryma.Composer.Infrastructure.Features.User;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStream.Create
{
    public sealed class CreateHandler(
        AppDbContext context,
        UserStore userStore,
        ComposerStreamStore composerStreamStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CreateCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CreateCommand command, CancellationToken ct)
        {
            try
            {
                UserEntity createdByUser = await userStore.GetUser(command.CreatedByUserId, ct);

                ComposerStreamEntity stream = composerStreamStore.CreateStream(command.EventDate, command.Type, createdByUser);
                await context.SaveChangesAsync(ct);

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
