namespace Faryma.Composer.Desktop.Messages.Events
{
    /// <summary>
    /// Сообщение отображается
    /// </summary>
    public sealed class MessageDisplayedEvent(string frameName)
    {
        /// <summary>
        /// Имя фрейма, в котором отображается сообщение
        /// </summary>
        public string FrameName { get; } = frameName;
    }
}