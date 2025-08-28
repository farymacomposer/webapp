using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Shared.Dto;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    public sealed partial class ReviewOrderVM : ObservableObject
    {
        public ReviewOrderDto Dto { get; private set; }

        public ReviewOrderVM(ReviewOrderDto dto)
        {
            Dto = dto;
        }

        public void Update(ReviewOrderDto dto) => Dto = dto;
    }
}