using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Результат выполнения идемпотентного API-запроса
    /// </summary>
    public sealed class IdempotencyRecordEntity : BaseEntity
    {
        /// <summary>
        /// Шаблон маршрута или путь защищенного endpoint
        /// </summary>
        public required string EndpointKey { get; set; }

        /// <summary>
        /// Пользователь, в рамках которого уникален ключ
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Ключ идемпотентности из запроса
        /// </summary>
        public required Guid IdempotencyKey { get; set; }

        /// <summary>
        /// Хеш полезной нагрузки запроса
        /// </summary>
        public required string RequestHash { get; set; }

        /// <summary>
        /// HTTP-статус сохраненного ответа
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// JSON сохраненного ответа
        /// </summary>
        public string? ResponseJson { get; set; }

        /// <summary>
        /// Дата и время создания записи
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время истечения записи
        /// </summary>
        public required DateTime ExpiresAt { get; set; }
    }
}
