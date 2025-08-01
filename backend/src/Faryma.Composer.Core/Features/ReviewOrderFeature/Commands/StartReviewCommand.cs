namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    public sealed record StartReviewCommand
    {
        public long ReviewOrderId { get; init; }
    }
}