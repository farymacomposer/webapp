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
        /// Дата выпуска трека
        /// </summary>
        public required DateTime? ReleaseDate { get; init; }

        /// <summary>
        /// Ссылка на обложку
        /// </summary>
        public required string? CoverUrl { get; init; }

        /// <summary>
        /// Расширенные жанры
        /// </summary>
        public required IEnumerable<string> ExtendedGenres { get; init; }

        /// <summary>
        /// Тэги
        /// </summary>
        public required IEnumerable<TrackTag> Tags { get; init; }

        /// <summary>
        /// Страна производства
        /// </summary>
        public required TrackCountryQueryModel? Country { get; init; }

        /// <summary>
        /// Связь с исполнителями
        /// </summary>
        public required IEnumerable<TrackArtistQueryModel> Artists { get; init; }

        /// <summary>
        /// Связь с жанрами
        /// </summary>
        public required IEnumerable<TrackGenreQueryModel> Genres { get; init; }

        /// <summary>
        /// Результаты разборов трека композитором
        /// </summary>
        public required IEnumerable<ReviewQueryModel> Reviews { get; init; }

        /// <summary>
        /// Оценки пользователей
        /// </summary>
        public required IEnumerable<UserTrackRatingQueryModel> UserRatings { get; init; }
    }
}