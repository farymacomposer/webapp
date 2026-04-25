using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Faryma.Composer.Desktop.Navigation
{
    public abstract partial class DialogVM(DialogService dialogService) : ObservableObject
    {
        public virtual Task OnNavigatedTo(object? parameter) => Task.CompletedTask;

        [RelayCommand]
        protected void HideDialog() => dialogService.HideDialog();
    }
}
