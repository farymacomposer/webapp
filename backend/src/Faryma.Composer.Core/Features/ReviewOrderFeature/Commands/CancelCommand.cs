namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    public sealed record CancelCommand
    {
        public long ReviewOrderId { get; init; }
    }
}