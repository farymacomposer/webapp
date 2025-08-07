namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class NewOrderAddedMessage
    {
        public required ReviewOrderDto Order { get; set; }
        public required OrderQueuePositionDto CurrentPosition { get; set; }
    }
}