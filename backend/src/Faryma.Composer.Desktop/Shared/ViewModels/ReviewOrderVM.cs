using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Desktop.UI.OrderQueueFeature;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    public sealed partial class ReviewOrderVM(ReviewOrderDto dto, OrderQueuePositionDto currentPosition) : ObservableObject
    {
        /// <summary>
        /// Id заказа
        /// </summary>
        public long Id => dto.Id;

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public DateTime CreatedAt => dto.CreatedAt;

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public DateTime? InProgressAt => dto.InProgressAt;

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public DateTime? CompletedAt => dto.CompletedAt;

        /// <summary>
        /// Тип заказа
        /// </summary>
        public ReviewOrderType Type => dto.Type;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CategoryType => dto.CategoryType;

        /// <summary>
        /// Статус заказа
        /// </summary>
        public ReviewOrderStatus Status => dto.Status;

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public bool IsFrozen => dto.IsFrozen;

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl => dto.TrackUrl;

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment => dto.UserComment;

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public string MainNickname => dto.MainNickname;

        /// <summary>
        /// Общая стоимость заказа (номинал + платежи)
        /// </summary>
        public decimal TotalAmount => dto.TotalAmount;

        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public int QueueIndex => currentPosition.QueueIndex;

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public OrderActivityStatus ActivityStatus => currentPosition.ActivityStatus;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CurrentCategoryType => currentPosition.CategoryType;

        /// <summary>
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public int CategoryDebtNumber => currentPosition.CategoryDebtNumber;

        /// <summary>
        /// Id стрима
        /// </summary>
        public long StreamId => dto.CreationStream.Id;

        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public DateOnly StreamEventDate => dto.CreationStream.EventDate;

        /// <summary>
        /// Статус стрима
        /// </summary>
        public ComposerStreamStatus StreamStatus => dto.CreationStream.Status;

        /// <summary>
        /// Тип стрима
        /// </summary>
        public ComposerStreamType StreamType => dto.CreationStream.Type;

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StreamStartedAt => dto.CreationStream.StartedAt;

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? StreamCompletedAt => dto.CreationStream.CompletedAt;

        [RelayCommand]
        private void Select() => App.GetService<OrderQueuePageVM>().SelectedOrder = this;
    }
}