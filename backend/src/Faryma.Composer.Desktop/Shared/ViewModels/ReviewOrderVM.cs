using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    public sealed partial class ReviewOrderVM : ObservableObject
    {
        public string Title => $"{Id} {CreatedAt:s} {MainNickname} {TotalAmount}";

        /// <summary>
        /// Id заказа
        /// </summary>
        public long Id => Dto.Id;

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public DateTime CreatedAt => Dto.CreatedAt;

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public DateTime? InProgressAt => Dto.InProgressAt;

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public DateTime? CompletedAt => Dto.CompletedAt;

        /// <summary>
        /// Тип заказа
        /// </summary>
        public ReviewOrderType Type => Dto.Type;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CategoryType => Dto.CategoryType;

        /// <summary>
        /// Статус заказа
        /// </summary>
        public ReviewOrderStatus Status => Dto.Status;

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public bool IsFrozen => Dto.IsFrozen;

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl => Dto.TrackUrl;

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment => Dto.UserComment;

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public string MainNickname => Dto.MainNickname;

        /// <summary>
        /// Общая стоимость заказа (номинал + платежи)
        /// </summary>
        public decimal TotalAmount => Dto.TotalAmount;

        public ReviewOrderDto Dto { get; private set; }

        public ReviewOrderVM(ReviewOrderDto dto)
        {
            Dto = dto;
        }

        public void UpdateTrackUrl(ReviewOrderDto dto)
        {
            Dto = dto;
            OnPropertyChanged(nameof(TrackUrl));
        }
    }
}