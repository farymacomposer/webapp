using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed class DialogService(IServiceProvider provider)
    {
        private ContentControl _frame = null!;

        public void HideDialog() => _frame.Content = null;
        public void SetFrame(ContentControl frame) => _frame = frame;

        public async Task ShowDialog<TDialog>(object? parameter = null) where TDialog : UserControl
        {
            TDialog dialog = provider.GetRequiredService<TDialog>();
            if (dialog.DataContext is DialogVM vm)
            {
                await vm.OnNavigatedTo(parameter);
            }

            _frame.Content = dialog;
        }
    }
}