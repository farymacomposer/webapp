using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.TrackFeature
{
    public record TrackDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public int? Year { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Url { get; set; } = default!;
        public TrackOrigin? Origin { get; set; }
        public Guid UploadedByUserNicknameId { get; set; }
        public string UploadedByNickname { get; set; } = default!;

        public List<string> ArtistNames { get; set; } = [];
        public List<string> GenreNames { get; set; } = [];

        public static TrackDto Map(Track track)
        {
            return new()
            {
                Id = track.Id,
                Title = track.Title,
                Year = track.Year,
                UploadedAt = track.UploadedAt,
                Url = track.Url,
                Origin = track.Origin,
                UploadedByUserNicknameId = track.UploadedByUserNicknameId,
                UploadedByNickname = track.UploadedBy.Nickname,
                ArtistNames = track.Artists.Select(a => a.Name).ToList(),
                GenreNames = track.Genres.Select(g => g.Name).ToList()
            };
        }
    }
}