using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.ViewModels;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class StreamDaySlotVM : ObservableObject
    {
        public DateOnly Date { get; init; }
        public bool IsCurrentMonth { get; init; }
        public ComposerStreamVM? Stream { get; init; }
    }
}