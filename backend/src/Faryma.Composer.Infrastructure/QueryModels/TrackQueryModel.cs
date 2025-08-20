using Faryma.Composer.Infrastructure.Models;

namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Музыкальный трек
    /// </summary>
    public sealed record TrackQueryModel
    {
        /// <summary>
        /// Название трека
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Год выпуска трека
        /// </summary>
        public required int? ReleaseYear { get; init; }

        /// <summary>
        /// Id страны производства
        /// </summary>
        public required long? CountryId { get; init; }

        /// <summary>
        /// Ссылка на обложку
        /// </summary>
        public required string? CoverUrl { get; init; }

        /// <summary>
        /// Есть разнос
        /// </summary>
        public required bool HasReview { get; init; }

        /// <summary>
        /// Оценка последнего разноса
        /// </summary>
        public required int? LastReviewRating { get; init; }

        /// <summary>
        /// Оценка пользователей
        /// </summary>
        public required int? UserRating { get; init; }

        /// <summary>
        /// Расширенные жанры
        /// </summary>
        public required IEnumerable<string> ExtendedGenres { get; init; }

        /// <summary>
        /// Тэги
        /// </summary>
        public required IEnumerable<TrackTag> Tags { get; init; }

        /// <summary>
        /// Исполнители
        /// </summary>
        public required IEnumerable<string> Artists { get; init; }

        /// <summary>
        /// Id жанров
        /// </summary>
        public required IEnumerable<long> GenreIds { get; init; }
    }
}