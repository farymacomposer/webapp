using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Services;
using Faryma.Composer.Desktop.ViewModels;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ReviewOrderPageVM : ObservableObject
    {
        private readonly DialogService _dialogService;

        public ObservableCollection<ReviewOrderGroup> ActivityGroups { get; } = [];

        [ObservableProperty]
        public partial ReviewOrderVM? SelectedReviewOrder { get; set; }

        public ReviewOrderPageVM(OrderQueueService orderQueueService, DialogService dialogService)
        {
            _dialogService = dialogService;

            ActivityGroups.Add(new ReviewOrderGroup
            {
                Title = "Активные",
                Orders = orderQueueService.ActiveOrders,
            });

            ActivityGroups.Add(new ReviewOrderGroup
            {
                Title = "Выполненные",
                Orders = orderQueueService.CompletedOrders,
            });

            ActivityGroups.Add(new ReviewOrderGroup
            {
                Title = "Замороженные",
                Orders = orderQueueService.FrozenOrders,
            });

            ActivityGroups.Add(new ReviewOrderGroup
            {
                Title = "Запланированные",
                Orders = orderQueueService.ScheduledOrders,
            });
        }

        [RelayCommand]
        private Task OpenCreateReviewOrder() => _dialogService.ShowDialog<CreateReviewOrderDialog, CreateReviewOrderDialogVM>();

        [RelayCommand]
        private void SelectReviewOrder(ReviewOrderVM reviewOrder)
        {
            SelectedReviewOrder?.IsSelected = false;
            SelectedReviewOrder = reviewOrder;
            SelectedReviewOrder.IsSelected = true;
        }
    }
}
