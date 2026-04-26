using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.UI;

namespace Faryma.Composer.Desktop.ViewModels
{
    public sealed partial class StreamContainerVM : ObservableObject
    {
        [ObservableProperty]
        public partial DateOnly Date { get; set; }

        [ObservableProperty]
        public partial ComposerStreamVM? Stream { get; set; }

        [RelayCommand]
        private void Select()
        {
            OrderQueuePageVM page = App.GetService<OrderQueuePageVM>();
            page.SelectedEventDate = Date;
            page.SelectedStream = Stream;
        }
    }
}
