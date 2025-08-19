using HotChocolate.Data.Filters;
using HotChocolate.Types;

namespace Faryma.Composer.Infrastructure.QueryModels
{
    /// <summary>
    /// Параметры фильтрации для музыкального трека
    /// </summary>
    public sealed class TrackQueryModelFilterInput : FilterInputType<TrackQueryModel>
    {
        protected override void Configure(IFilterInputTypeDescriptor<TrackQueryModel> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Title);
            descriptor.Field(x => x.ReleaseYear);
            descriptor.Field(x => x.CountryId);
            descriptor.Field(x => x.HasReview);
            descriptor.Field(x => x.ExtendedGenres);
            descriptor.Field(x => x.Tags);
            descriptor.Field(x => x.Artists);
            descriptor.Field(x => x.GenreIds);
        }
    }

    public sealed class Contains : StringOperationFilterInputType
    {
        protected override void Configure(IFilterInputTypeDescriptor descriptor) =>
            descriptor.Operation(DefaultFilterOperations.Contains).Type<StringType>();
    }

    public sealed class Int : IntOperationFilterInputType
    {
        protected override void Configure(IFilterInputTypeDescriptor descriptor)
        {
            descriptor.Operation(DefaultFilterOperations.GreaterThanOrEquals).Type<IntType>();
            descriptor.Operation(DefaultFilterOperations.LowerThanOrEquals).Type<IntType>();
        }
    }
}