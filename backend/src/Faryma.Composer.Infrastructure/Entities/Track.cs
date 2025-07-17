using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Музыкальный трек
    /// </summary>
    public sealed class Track : BaseEntity
    {
        /// <summary>
        /// Название трека
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Год выпуска трека
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Дата и время загрузки трека
        /// </summary>
        public required DateTime UploadedAt { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string Url { get; set; }

        /// <summary>
        /// Происхождение трека
        /// </summary>
        public TrackOrigin? Origin { get; set; }

        public Guid UploadedByUserNicknameId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Пользователь, загрузивший трек
        /// </summary>
        [ForeignKey(nameof(UploadedByUserNicknameId))]
        public required UserNickname UploadedBy { get; set; }

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public ICollection<Artist> Artists { get; set; } = [];

        /// <summary>
        /// Связь с жанрами
        /// </summary>
        public ICollection<Genre> Genres { get; set; } = [];

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