using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.Navigation
{
    public abstract class DialogVM(DialogService dialogService) : ObservableObject
    {
        public virtual Task OnNavigatedTo(object? parameter) => Task.CompletedTask;
        protected void HideDialog() => dialogService.HideDialog();
    }
}