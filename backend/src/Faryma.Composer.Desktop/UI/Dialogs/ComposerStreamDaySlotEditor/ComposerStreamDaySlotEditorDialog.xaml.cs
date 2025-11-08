using Faryma.Composer.Desktop.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamDaySlotEditorDialog : UserControl, IDialogControl<ComposerStreamDaySlotEditorDialogVM>
    {
        public ComposerStreamDaySlotEditorDialogVM ViewModel { get; } = App.GetService<ComposerStreamDaySlotEditorDialogVM>();

        public ComposerStreamDaySlotEditorDialog()
        {
            InitializeComponent();
        }
    }
}