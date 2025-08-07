namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class OrderRemovedMessage
    {
        public required ReviewOrderDto Order { get; set; }
        public required OrderQueuePositionDto PreviousPosition { get; set; }
    }
}