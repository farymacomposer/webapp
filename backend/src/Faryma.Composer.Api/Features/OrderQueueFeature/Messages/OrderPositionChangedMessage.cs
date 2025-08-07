namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class OrderPositionChangedMessage
    {
        public required ReviewOrderDto Order { get; set; }
        public required OrderQueuePositionDto CurrentPosition { get; set; }
        public required OrderQueuePositionDto PreviousPosition { get; set; }
    }
}