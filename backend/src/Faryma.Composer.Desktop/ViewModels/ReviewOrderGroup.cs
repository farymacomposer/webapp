using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.ViewModels
{
    public sealed partial class ReviewOrderGroup : ObservableObject
    {
        public required string Title { get; init; }
        public required ObservableCollection<ReviewOrderVM> Orders { get; init; }
    }
}
