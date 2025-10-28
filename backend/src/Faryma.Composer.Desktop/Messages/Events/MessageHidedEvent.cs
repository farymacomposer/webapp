namespace Faryma.Composer.Desktop.Messages.Events
{
    /// <summary>
    /// Сообщение скрыто
    /// </summary>
    public sealed class MessageHidedEvent(string frameName)
    {
        /// <summary>
        /// Имя фрейма, в котором отображается сообщение
        /// </summary>
        public string FrameName { get; } = frameName;
    }
}