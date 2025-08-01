namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    public sealed record FreezeCommand
    {
        public long ReviewOrderId { get; init; }
    }
}