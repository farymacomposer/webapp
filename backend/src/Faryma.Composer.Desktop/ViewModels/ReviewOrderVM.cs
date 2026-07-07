using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Contracts.Api.Features.OrderQueue.Dto;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Desktop.UI;

namespace Faryma.Composer.Desktop.ViewModels
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
        /// Есть ссылка на трек
        /// </summary>
        public bool HasTrackUrl => TrackUrl is not null;

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; } = dto.UserComment;

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public string MainNickname { get; } = dto.MainNickname;

        /// <summary>
        /// Денежная сумма для обратной совместимости
        /// </summary>
        public long TotalAmount { get; } = dto.TotalAmount;

        /// <summary>
        /// Обязательная стоимость заказа
        /// </summary>
        public long RequiredAmount { get; } = dto.RequiredAmount;

        /// <summary>
        /// Сумма покрытия обязательной стоимости
        /// </summary>
        public long CoveredAmount { get; } = dto.CoveredAmount;

        /// <summary>
        /// Сумма денежных платежей по заказу
        /// </summary>
        public long PaidAmount { get; } = dto.PaidAmount;

        /// <summary>
        /// Денежная сумма, которая влияет на донатный приоритет
        /// </summary>
        public long PaidPriorityAmount { get; } = dto.PaidPriorityAmount;

        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public int QueueIndex { get; } = currentPosition.QueueIndex;

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public OrderActivityStatus ActivityStatus { get; } = currentPosition.ActivityStatus;

        /// <summary>
        /// Категория заказа в очереди
        /// </summary>
        public QueueCategory CurrentQueueCategory { get; } = currentPosition.QueueCategory;

        /// <summary>
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public int CategoryDebtIndex { get; } = currentPosition.CategoryDebtIndex;

        /// <summary>
        /// Id стрима где создан заказ
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
        /// Выбран
        /// </summary>
        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [RelayCommand]
        private void Select() => App.GetService<OrderQueuePageVM>().SelectedOrder = this;
    }
}
