using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed class DialogService(IServiceProvider provider)
    {
        private ContentControl _frame = null!;

        public void HideDialog() => _frame.Content = null;
        public void SetFrame(ContentControl frame) => _frame = frame;

        public async Task ShowDialog<TDialog, TViewModel>(object? parameter = null)
            where TDialog : UserControl, IDialogControl<TViewModel>
            where TViewModel : DialogVM
        {
            TDialog dialog = provider.GetRequiredService<TDialog>();

            await dialog.ViewModel.OnNavigatedTo(parameter);

            _frame.Content = dialog;
        }
    }
}
