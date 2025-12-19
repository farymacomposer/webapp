using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;
using Faryma.Composer.Contracts.Infrastructure.Models;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Музыкальный трек
    /// </summary>
    public sealed class TrackEntity : BaseEntity
    {
        /// <summary>
        /// Дата и время добавления трека
        /// </summary>
        public required DateTime AddedAt { get; set; }

        /// <summary>
        /// Название трека
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string Url { get; set; }

        /// <summary>
        /// Дата выпуска трека
        /// </summary>
        public DateOnly? ReleaseDate { get; set; }

        /// <summary>
        /// Ссылка на обложку
        /// </summary>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Расширенные жанры
        /// </summary>
        public List<string> ExtendedGenres { get; set; } = [];

        /// <summary>
        /// Тэги
        /// </summary>
        public List<TrackTag> Tags { get; set; } = [];

        public Guid AddedByUserNicknameId { get; set; }
        public long? CountryId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, загрузивший трек
        /// </summary>
        [ForeignKey(nameof(AddedByUserNicknameId))]
        public required UserNicknameEntity AddedBy { get; set; }

        /// <summary>
        /// Страна производства
        /// </summary>
        [ForeignKey(nameof(CountryId))]
        public TrackCountryEntity? Country { get; set; }

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public ICollection<TrackArtistEntity> Artists { get; set; } = [];

        /// <summary>
        /// Связь с жанрами
        /// </summary>
        public ICollection<TrackGenreEntity> Genres { get; set; } = [];

        /// <summary>
        /// Заказы разборов трека
        /// </summary>
        public ICollection<ReviewOrderEntity> ReviewOrders { get; set; } = [];

        /// <summary>
        /// Результаты разборов трека композитором
        /// </summary>
        public ICollection<ReviewEntity> Reviews { get; set; } = [];

        /// <summary>
        /// Оценки пользователей
        /// </summary>
        public ICollection<UserTrackRatingEntity> UserRatings { get; set; } = [];
    }
}