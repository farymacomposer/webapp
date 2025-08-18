using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        /// <summary>
        /// Настройки приложения
        /// </summary>
        public DbSet<AppSettingsEntity> AppSettings { get; set; }

        /// <summary>
        /// Стримы композитора
        /// </summary>
        public DbSet<ComposerStream> ComposerStreams { get; set; }

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
        /// Исполнители музыкальных треков
        /// </summary>
        public DbSet<TrackArtist> TrackArtists { get; set; }

        /// <summary>
        /// Страны производства треков
        /// </summary>
        public DbSet<TrackCountry> TrackCountries { get; set; }

        /// <summary>
        /// Музыкальные жанры
        /// </summary>
        public DbSet<TrackGenre> TrackGenres { get; set; }

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("app");
            base.OnModelCreating(builder);

            builder.Entity<ComposerStream>()
                .HasMany(cs => cs.CreatedReviewOrders)
                .WithOne(ro => ro.CreationStream)
                .HasForeignKey(ro => ro.CreationStreamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ComposerStream>()
                .HasMany(cs => cs.ProcessedReviewOrders)
                .WithOne(ro => ro.ProcessingStream)
                .HasForeignKey(ro => ro.ProcessingStreamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Track>()
                .OwnsMany(x => x.Tags, x => x.ToJson());

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

            builder.Entity<TrackGenre>().HasData(
                new TrackGenre { Id = 1, Name = "электронное" },
                new TrackGenre { Id = 2, Name = "фолк" },
                new TrackGenre { Id = 3, Name = "рок" },
                new TrackGenre { Id = 4, Name = "разное" },
                new TrackGenre { Id = 5, Name = "джаз" },
                new TrackGenre { Id = 6, Name = "метал" },
                new TrackGenre { Id = 7, Name = "рэп" },
                new TrackGenre { Id = 8, Name = "поп" },
                new TrackGenre { Id = 9, Name = "оркестровый" },
                new TrackGenre { Id = 10, Name = "фанк" },
                new TrackGenre { Id = 11, Name = "мюзикл/опера" },
                new TrackGenre { Id = 12, Name = "инди" },
                new TrackGenre { Id = 13, Name = "поп-рок" },
                new TrackGenre { Id = 14, Name = "шансон" },
                new TrackGenre { Id = 15, Name = "специфическое" },
                new TrackGenre { Id = 16, Name = "баллада" },
                new TrackGenre { Id = 17, Name = "фортепиано" },
                new TrackGenre { Id = 18, Name = "инструментал" }
            );
        }
    }
}