namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class OrderRemovedMessage
    {
        public ReviewOrderDto Order { get; set; } = null!;
        public OrderQueuePositionDto PreviousPosition { get; set; } = null!;
    }
}