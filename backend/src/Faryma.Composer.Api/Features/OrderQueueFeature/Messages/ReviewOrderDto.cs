using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.OrderQueueFeature.Dto
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    public sealed record ReviewOrderDto
    {
        /// <summary>
        /// Id заказа
        /// </summary>
        public required long Id { get; init; }

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public required string MainNickname { get; init; }

        /// <summary>
        /// Общая стоимость заказа (номинал + платежи)
        /// </summary>
        public required decimal TotalAmount { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; init; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public required ReviewOrderStatus Status { get; init; }

        public static ReviewOrderDto Map(ReviewOrder item)
        {
            return new()
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                TrackUrl = item.TrackUrl,
                MainNickname = item.MainNickname,
                TotalAmount = item.GetTotalAmount(),
                Type = item.Type,
                Status = item.Status,
            };
        }
    }
}