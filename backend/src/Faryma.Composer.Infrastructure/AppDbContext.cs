using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options), IDataProtectionKeyContext
    {
        /// <summary>
        /// Настройки приложения
        /// </summary>
        public DbSet<AppSettingsEntity> AppSettings { get; set; }

        /// <summary>
        /// Стримы композитора
        /// </summary>
        public DbSet<ComposerStreamEntity> ComposerStreams { get; set; }

        /// <summary>
        /// Результаты разборов треков
        /// </summary>
        public DbSet<ReviewEntity> Reviews { get; set; }

        /// <summary>
        /// Заказы разборов треков
        /// </summary>
        public DbSet<ReviewOrderEntity> ReviewOrders { get; set; }

        /// <summary>
        /// Музыкальные треки
        /// </summary>
        public DbSet<TrackEntity> Tracks { get; set; }

        /// <summary>
        /// Исполнители музыкальных треков
        /// </summary>
        public DbSet<TrackArtistEntity> TrackArtists { get; set; }

        /// <summary>
        /// Страны производства треков
        /// </summary>
        public DbSet<TrackCountryEntity> TrackCountries { get; set; }

        /// <summary>
        /// Музыкальные жанры
        /// </summary>
        public DbSet<TrackGenreEntity> TrackGenres { get; set; }

        /// <summary>
        /// Операции по счетам
        /// </summary>
        public DbSet<TransactionEntity> Transactions { get; set; }

        /// <summary>
        /// Пользователи
        /// </summary>
        public DbSet<UserEntity> User { get; set; }

        /// <summary>
        /// Счета пользователей
        /// </summary>
        public DbSet<UserAccountEntity> UserAccounts { get; set; }

        /// <summary>
        /// Псевдонимы пользователей
        /// </summary>
        public DbSet<UserNicknameEntity> UserNicknames { get; set; }

        /// <summary>
        /// Оценки пользователей
        /// </summary>
        public DbSet<UserTrackRatingEntity> UserTrackRatings { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema(DbContextHelper.SchemaName);
            base.OnModelCreating(builder);

            builder.HasPostgresEnum();

            builder.Entity<ComposerStreamEntity>()
                .HasMany(cs => cs.CreatedReviewOrders)
                .WithOne(ro => ro.CreationStream)
                .HasForeignKey(ro => ro.CreationStreamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ComposerStreamEntity>()
                .HasMany(cs => cs.ProcessedReviewOrders)
                .WithOne(ro => ro.ProcessingStream)
                .HasForeignKey(ro => ro.ProcessingStreamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrackEntity>()
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

            builder.Entity<AppSettingsEntity>().HasData(new AppSettingsEntity
            {
                Id = 1,
                ReviewOrderNominalAmount = 750,
            });

            builder.Entity<TrackGenreEntity>().HasData(
                new TrackGenreEntity { Id = 1, Name = "электронное" },
                new TrackGenreEntity { Id = 2, Name = "фолк" },
                new TrackGenreEntity { Id = 3, Name = "рок" },
                new TrackGenreEntity { Id = 4, Name = "разное" },
                new TrackGenreEntity { Id = 5, Name = "джаз" },
                new TrackGenreEntity { Id = 6, Name = "метал" },
                new TrackGenreEntity { Id = 7, Name = "рэп" },
                new TrackGenreEntity { Id = 8, Name = "поп" },
                new TrackGenreEntity { Id = 9, Name = "оркестровый" },
                new TrackGenreEntity { Id = 10, Name = "фанк" },
                new TrackGenreEntity { Id = 11, Name = "мюзикл/опера" },
                new TrackGenreEntity { Id = 12, Name = "инди" },
                new TrackGenreEntity { Id = 13, Name = "поп-рок" },
                new TrackGenreEntity { Id = 14, Name = "шансон" },
                new TrackGenreEntity { Id = 15, Name = "специфическое" },
                new TrackGenreEntity { Id = 16, Name = "баллада" },
                new TrackGenreEntity { Id = 17, Name = "фортепиано" },
                new TrackGenreEntity { Id = 18, Name = "инструментал" }
            );
        }
    }
}