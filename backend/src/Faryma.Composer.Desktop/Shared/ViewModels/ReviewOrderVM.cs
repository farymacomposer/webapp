using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Desktop.Services.OrderQueueFeature.Dto;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    public sealed partial class ReviewOrderVM : ObservableObject
    {
        public string Title => $"{MainNickname} {TotalAmount} {CreatedAt:T}";

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

        /// <summary>
        /// Позиция заказа в очереди
        /// </summary>
        public int QueueIndex => CurrentPosition.QueueIndex;

        /// <summary>
        /// Статус активности заказа
        /// </summary>
        public OrderActivityStatus ActivityStatus => CurrentPosition.ActivityStatus;

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public OrderCategoryType CurrentCategoryType => CurrentPosition.CategoryType;

        /// <summary>
        /// Номер категории, если заказ относится к долговой категории
        /// </summary>
        public int CategoryDebtNumber => CurrentPosition.CategoryDebtNumber;

        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public DateOnly StreamEventDate => Dto.CreationStream.EventDate;

        /// <summary>
        /// Статус стрима
        /// </summary>
        public ComposerStreamStatus StreamStatus => Dto.CreationStream.Status;

        /// <summary>
        /// Тип стрима
        /// </summary>
        public ComposerStreamType StreamType => Dto.CreationStream.Type;

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StreamStartedAt => Dto.CreationStream.StartedAt;

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? StreamCompletedAt => Dto.CreationStream.CompletedAt;

        public ReviewOrderDto Dto { get; private set; }
        public OrderQueuePositionDto CurrentPosition { get; private set; }

        public ReviewOrderVM(ReviewOrderDto dto, OrderQueuePositionDto currentPosition)
        {
            Dto = dto;
            CurrentPosition = currentPosition;
        }

        public void Update(ReviewOrderDto dto, OrderQueuePositionDto currentPosition)
        {
            Dto = dto;
            CurrentPosition = currentPosition;
            OnPropertyChanged();
        }
    }
}