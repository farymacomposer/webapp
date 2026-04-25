namespace Faryma.Composer.Desktop.Navigation
{
    /// <summary>
    /// Ответ MessageDialog, при его закрытии
    /// </summary>
    public enum MessageDialogResponse
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

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
