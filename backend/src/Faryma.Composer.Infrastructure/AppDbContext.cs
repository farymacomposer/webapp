using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        /// <summary>
        /// Исполнители музыкальных треков
        /// </summary>
        public DbSet<Artist> Artists { get; set; }

        /// <summary>
        /// Стримы композитора
        /// </summary>
        public DbSet<ComposerStream> ComposerStreams { get; set; }

        /// <summary>
        /// Музыкальные жанры
        /// </summary>
        public DbSet<Genre> Genres { get; set; }

        /// <summary>
        /// Результаты разборов треков
        /// </summary>
        public DbSet<Review> Reviews { get; set; }

        /// <summary>
        /// Заказы разборов треков
        /// </summary>
        public DbSet<ReviewOrder> ReviewOrders { get; set; }

        /// <summary>
        /// Музыкальные треки
        /// </summary>
        public DbSet<Track> Tracks { get; set; }

        /// <summary>
        /// Операции по счетам
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// Пользователи
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// Счета пользователей
        /// </summary>
        public DbSet<UserAccount> UserAccounts { get; set; }

        /// <summary>
        /// Псевдонимы пользователей
        /// </summary>
        public DbSet<UserNickname> UserNicknames { get; set; }

        /// <summary>
        /// Оценки пользователей
        /// </summary>
        public DbSet<UserTrackRating> UserTrackRatings { get; set; }

        /// <summary>
        /// Настройки приложения
        /// </summary>
        public DbSet<AppSettingsEntity> AppSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("app");
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("9C3DDCDE-24E7-458C-8D9C-1E5F424D3DDD"),
                    Name = "Composer",
                    NormalizedName = "COMPOSER",
                    ConcurrencyStamp = "9C3DDCDE-24E7-458C-8D9C-1E5F424D3DDD"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("AC0B9E85-A06F-4655-822B-9C125D8D7BB4"),
                    Name = "Moderator",
                    NormalizedName = "MODERATOR",
                    ConcurrencyStamp = "AC0B9E85-A06F-4655-822B-9C125D8D7BB4"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("910C6755-4833-4C62-8DF7-4241A159A8D2"),
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "910C6755-4833-4C62-8DF7-4241A159A8D2"
                }
            );

            builder.Entity<AppSettingsEntity>().HasData(
                new AppSettingsEntity
                {
                    Id = 1,
                    ReviewOrderNominalAmount = 750,
                }
            );
        }
    }
}