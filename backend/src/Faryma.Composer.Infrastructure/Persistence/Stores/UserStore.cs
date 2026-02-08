using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class UserStore(AppDbContext context)
    {
        public UserEntity Get(Guid id) => context.Users.Find(id)
            ?? throw new InvalidOperationException($"Пользователь с ID {id} не найден");
    }
}