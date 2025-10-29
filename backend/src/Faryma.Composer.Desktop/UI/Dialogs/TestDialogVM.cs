using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Navigation;

namespace Faryma.Composer.Desktop.UI.Dialogs
{
    public sealed partial class TestDialogVM(DialogService dialogService) : DialogVM(dialogService)
    {
        [ObservableProperty]
        public partial string? MyProperty { get; set; }

        public override Task OnNavigatedTo(object? parameter)
        {
            MyProperty = parameter?.ToString();

            return Task.CompletedTask;
        }
    }
}