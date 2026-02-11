using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Faryma.Composer.Infrastructure.Persistence.EntityConfigurations
{
    internal sealed class UserTrackRatingEntityConfiguration : IEntityTypeConfiguration<UserTrackRatingEntity>
    {
        public void Configure(EntityTypeBuilder<UserTrackRatingEntity> builder)
        {
            builder.ToTable("user_track_ratings");

            builder.Property(x => x.Comment)
                .HasMaxLength(200);

            builder.HasOne(x => x.Track)
                .WithMany(x => x.UserRatings)
                .HasForeignKey(x => x.TrackId);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.TrackRatings)
                .HasForeignKey(x => x.CreatedByUserId);
        }
    }
}