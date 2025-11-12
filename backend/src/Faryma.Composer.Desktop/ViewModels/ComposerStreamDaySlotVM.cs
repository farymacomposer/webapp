using CommunityToolkit.Mvvm.ComponentModel;

namespace Faryma.Composer.Desktop.ViewModels
{
    /// <summary>
    /// Слот расписания стрима
    /// </summary>
    public sealed partial class ComposerStreamDaySlotVM : ObservableObject
    {
        /// <summary>
        /// Дата
        /// </summary>
        public DateOnly Date { get; init; }

        /// <summary>
        /// Сегодня
        /// </summary>
        public bool IsToday => Date == DateOnly.FromDateTime(DateTime.Today);

        /// <summary>
        /// Текущий месяц
        /// </summary>
        public bool IsCurrentMonth { get; init; }

        /// <summary>
        /// Стрим
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStream))]
        public partial ComposerStreamVM? Stream { get; set; }

        /// <summary>
        /// Есть стрим
        /// </summary>
        public bool HasStream => Stream is not null;

        /// <summary>
        /// Выбран
        /// </summary>
        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }
}