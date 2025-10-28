namespace Faryma.Composer.Desktop.Navigation.Message
{
    /// <summary>
    /// Настройки окна сообщения
    /// </summary>
    public sealed class MessageOptions
    {
        /// <summary>
        /// Заголовок
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Текст первой кнопки
        /// </summary>
        public string? FirstButtonText { get; init; }

        /// <summary>
        /// Текст второй кнопки
        /// </summary>
        public string SecondButtonText { get; init; } = "OK";

        /// <summary>
        /// Дополнительное сообщение
        /// </summary>
        internal string? SubMessage { get; set; }
    }
}