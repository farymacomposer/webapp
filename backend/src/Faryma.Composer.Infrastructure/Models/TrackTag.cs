namespace Faryma.Composer.Infrastructure.Models
{
    /// <summary>
    /// Тег музыкального трека
    /// </summary>
    public sealed class TrackTag
    {
        /// <summary>
        /// Имя тега
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public string? Meta { get; set; }
    }
}