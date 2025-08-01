namespace Faryma.Composer.Core.Features.ReviewFeature.Commands
{
    public sealed record CompleteReviewCommand
    {
        public long ReviewOrderId { get; init; }

        public int Rating { get; init; }

        public required string Comment { get; init; }
    }
}