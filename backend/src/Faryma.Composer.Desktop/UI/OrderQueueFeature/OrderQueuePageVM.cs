using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Services.OrderQueueFeature;

namespace Faryma.Composer.Desktop.UI.OrderQueueFeature
{
    public sealed partial class OrderQueuePageVM(OrderQueueService orderQueueService) : ObservableObject
    {
        public OrderQueueService OrderQueueService { get; } = orderQueueService;
    }
}