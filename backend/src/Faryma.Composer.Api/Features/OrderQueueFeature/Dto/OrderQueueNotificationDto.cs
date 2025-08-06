namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    public class NewOrderAddedMessage
    {
        public ReviewOrderDto Order { get; set; } = null!;
        public OrderQueuePositionDto CurrentPosition { get; set; } = null!;
    }

    public class OrderRemovedMessage
    {
        public ReviewOrderDto Order { get; set; } = null!;
        public OrderQueuePositionDto PreviousPosition { get; set; } = null!;
    }

    public class OrderPositionChangedMessage
    {
        public ReviewOrderDto Order { get; set; } = null!;
        public OrderQueuePositionDto CurrentPosition { get; set; } = null!;
        public OrderQueuePositionDto PreviousPosition { get; set; } = null!;
    }
}