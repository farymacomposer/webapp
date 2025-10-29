using Faryma.Composer.Desktop.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI.Dialogs
{
    public sealed partial class TestDialog : UserControl, IDialogControl<TestDialogVM>
    {
        public TestDialogVM ViewModel { get; } = App.GetService<TestDialogVM>();

        public TestDialog()
        {
            InitializeComponent();
        }
    }
}