using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Navigation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ReviewOrderPageVM(DialogService dialogService) : ObservableObject
    {
        public ObservableCollection<ControlInfoDataGroup> Groups { get; } = new()
        {
            new ControlInfoDataGroup
            {
                Title = "Title1",
                Items =
                {
                    new()
                    {
                        Title = "Title1",
                        Subtitle = "Subtitle1"
                    },
                    new()
                    {
                        Title = "Title1",
                        Subtitle = "Subtitle2"
                    }
                },
            },
            new ControlInfoDataGroup
            {
                Title = "Title2",
                Items =
                {
                    new()
                    {
                        Title = "Title2",
                        Subtitle = "Subtitle3"
                    },
                    new()
                    {
                        Title = "Title2",
                        Subtitle = "Subtitle4"
                    }
                },
            }
        };

        [RelayCommand]
        private Task OpenCreateReviewOrder() => dialogService.ShowDialog<CreateReviewOrderDialog, CreateReviewOrderDialogVM>();
    }

    public sealed partial class ControlInfoDataGroup : ObservableObject
    {
        public required string Title { get; set; }
        public ObservableCollection<ControlInfoDataItem> Items { get; } = [];
    }

    public sealed partial class ControlInfoDataItem : ObservableObject
    {
        public required string Title { get; set; }
        public required string Subtitle { get; set; }
    }
}