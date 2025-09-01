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
        public long Id { get; } = dto.Id;

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public DateTime CreatedAt { get; } = dto.CreatedAt;

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public DateTime? InProgressAt { get; } = dto.InProgressAt;

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public DateTime? CompletedAt { get; } = dto.CompletedAt;

        /// <summary>
        /// Тип заказа
        /// </summary>
        public ReviewOrderType Type { get; } = dto.Type;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CategoryType { get; } = dto.CategoryType;

        /// <summary>
        /// Статус заказа
        /// </summary>
        public ReviewOrderStatus Status { get; } = dto.Status;

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public bool IsFrozen { get; } = dto.IsFrozen;

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; } = dto.TrackUrl;

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; } = dto.UserComment;

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public string MainNickname { get; } = dto.MainNickname;

        /// <summary>
        /// Общая стоимость заказа (номинал + платежи)
        /// </summary>
        public decimal TotalAmount { get; } = dto.TotalAmount;

        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public int QueueIndex { get; } = currentPosition.QueueIndex;

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public OrderActivityStatus ActivityStatus { get; } = currentPosition.ActivityStatus;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CurrentCategoryType { get; } = currentPosition.CategoryType;

        /// <summary>
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public int CategoryDebtNumber { get; } = currentPosition.CategoryDebtNumber;

        /// <summary>
        /// Id стрима
        /// </summary>
        public long StreamId { get; } = dto.CreationStream.Id;

        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public DateOnly StreamEventDate { get; } = dto.CreationStream.EventDate;

        /// <summary>
        /// Статус стрима
        /// </summary>
        public ComposerStreamStatus StreamStatus { get; } = dto.CreationStream.Status;

        /// <summary>
        /// Тип стрима
        /// </summary>
        public ComposerStreamType StreamType { get; } = dto.CreationStream.Type;

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StreamStartedAt { get; } = dto.CreationStream.StartedAt;

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? StreamCompletedAt { get; } = dto.CreationStream.CompletedAt;

        [RelayCommand]
        private void Select() => App.GetService<OrderQueuePageVM>().SelectedOrder = this;
    }
}