using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Dto
{
    public sealed class TrackDto
    {
        public required string Url { get; init; }
        public string? Title { get; init; }
        public int? Year { get; init; }
        public TrackOrigin? Origin { get; init; }
        public string? ArtistName { get; init; }
    }
}