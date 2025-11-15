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

        public ObservableCollection<ControlInfoDataGroup> Groups { get; } = new()
        {
            new ControlInfoDataGroup
            {
                Title = "Title1",
            },
            new ControlInfoDataGroup
            {
                Title = "Title2",
            }
        };

        public ReviewOrderPageVM(OrderQueueService orderQueueService, DialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        private Task OpenCreateReviewOrder() => _dialogService.ShowDialog<CreateReviewOrderDialog, CreateReviewOrderDialogVM>();
    }

    public sealed partial class ControlInfoDataGroup : ObservableObject
    {
        public required string Title { get; set; }
        public ObservableCollection<ReviewOrderVM> Items { get; } = [];
    }
}