using CommunityToolkit.Mvvm.ComponentModel;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Shared.ViewModels
{
    /// <summary>
    /// Стрим композитора
    /// </summary>
    public sealed partial class ComposerStreamVM(ComposerStreamDto dto) : ObservableObject
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public long Id => dto.Id;

        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public DateOnly EventDate => dto.EventDate;

        /// <summary>
        /// Статус стрима
        /// </summary>
        public ComposerStreamStatus Status => dto.Status;

        /// <summary>
        /// Тип стрима
        /// </summary>
        public ComposerStreamType Type => dto.Type;

        /// <summary>
        /// Дата и время начала стрима
        /// </summary>
        public DateTime? StartedAt => dto.StartedAt;

        /// <summary>
        /// Дата и время завершения стрима
        /// </summary>
        public DateTime? CompletedAt => dto.CompletedAt;
    }
}