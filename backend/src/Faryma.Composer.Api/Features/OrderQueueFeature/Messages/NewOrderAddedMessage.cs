namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class NewOrderAddedMessage
    {
        public ReviewOrderDto Order { get; set; } = null!;
        public OrderQueuePositionDto CurrentPosition { get; set; } = null!;
    }
}