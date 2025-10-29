namespace Faryma.Composer.Desktop.Navigation
{
    /// <summary>
    /// Ответ MessageDialog, при его закрытии
    /// </summary>
    public enum MessageDialogResponse
    {
        /// <summary>
        /// MessageDialog был закрыт программным способом
        /// </summary>
        None = 0,

        /// <summary>
        /// Была нажата первая кнопка
        /// </summary>
        FirstButton = 1,

        /// <summary>
        /// Была нажата вторая кнопка
        /// </summary>
        SecondButton = 2,
    }
}