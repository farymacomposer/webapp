using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.CommonDto
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
        /// Дата и время взятия заказа в работу
        /// </summary>
        public required DateTime? InProgressAt { get; init; }

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public required DateTime? CompletedAt { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; init; }

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public required OrderCategoryType CategoryType { get; init; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public required ReviewOrderStatus Status { get; init; }

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public required bool IsFrozen { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [Url]
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public required string? UserComment { get; init; }

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        [Required]
        public required string MainNickname { get; init; }

        /// <summary>
        /// Общая стоимость заказа (номинал + платежи)
        /// </summary>
        public required decimal TotalAmount { get; init; }

        public static ReviewOrderDto Map(ReviewOrder item)
        {
            return new()
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                InProgressAt = item.InProgressAt,
                CompletedAt = item.CompletedAt,
                Type = item.Type,
                CategoryType = item.CategoryType,
                Status = item.Status,
                IsFrozen = item.IsFrozen,
                TrackUrl = item.TrackUrl,
                UserComment = item.UserComment,
                MainNickname = item.MainNickname,
                TotalAmount = item.GetTotalAmount(),
            };
        }
    }
}