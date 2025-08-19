using HotChocolate.Data.Sorting;

namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Параметры сортировки для музыкального трека
    /// </summary>
    public sealed class TrackQueryModelSortInput : SortInputType<TrackQueryModel>
    {
        protected override void Configure(ISortInputTypeDescriptor<TrackQueryModel> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.LastReviewRating);
            descriptor.Field(x => x.UserRating);
        }
    }
}