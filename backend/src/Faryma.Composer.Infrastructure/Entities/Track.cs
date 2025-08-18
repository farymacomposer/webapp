using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Models;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Музыкальный трек
    /// </summary>
    public sealed class Track : BaseEntity
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
        public DateTime? ReleaseDate { get; set; }

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
        public required UserNickname AddedBy { get; set; }

        /// <summary>
        /// Страна производства
        /// </summary>
        [ForeignKey(nameof(CountryId))]
        public TrackCountry? Country { get; set; }

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public ICollection<TrackArtist> Artists { get; set; } = [];

        /// <summary>
        /// Связь с жанрами
        /// </summary>
        public ICollection<TrackGenre> Genres { get; set; } = [];

        /// <summary>
        /// Заказы разборов трека
        /// </summary>
        public ICollection<ReviewOrder> ReviewOrders { get; set; } = [];

        /// <summary>
        /// Результаты разборов трека композитором
        /// </summary>
        public ICollection<Review> Reviews { get; set; } = [];

        /// <summary>
        /// Оценки пользователей
        /// </summary>
        public ICollection<UserTrackRating> UserRatings { get; set; } = [];
    }
}