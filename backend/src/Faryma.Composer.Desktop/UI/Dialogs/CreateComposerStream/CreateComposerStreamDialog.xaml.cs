using Faryma.Composer.Desktop.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateComposerStreamDialog : UserControl, IDialogControl<CreateComposerStreamDialogVM>
    {
        public CreateComposerStreamDialogVM ViewModel { get; } = App.GetService<CreateComposerStreamDialogVM>();

        public CreateComposerStreamDialog()
        {
            InitializeComponent();
        }
    }
}