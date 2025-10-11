using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    public sealed partial class PageViewModel : ObservableObject
    {
        public string Icon { get; init; } = null!;
        public Type PageType { get; init; } = null!;
        public string Title { get; init; } = null!;
    }
}