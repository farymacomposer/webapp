using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Validation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateReviewOrderDialogVM(
        ReviewOrderHttpClient reviewOrderHttpClient,
        MessageService messageService,
        ValidationService validationService,
        DialogService dialogService
        ) : DialogVM(dialogService)
    {
        private Guid _idempotencyKey;

        public ReviewOrderType[] OrderTypes { get; } =
        [
            ReviewOrderType.Donation,
            ReviewOrderType.OutOfQueue,
            ReviewOrderType.Free,
            ReviewOrderType.Charity,
            ReviewOrderType.Custom,
        ];

        /// <summary>
        /// Тип заказа
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPaymentAmountEnabled))]
        public partial ReviewOrderType OrderType { get; set; }

        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? Nickname { get; set; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        [ObservableProperty]
        public partial string? PaymentAmount { get; set; }

        public bool IsPaymentAmountEnabled => OrderType is ReviewOrderType.Donation;

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [ObservableProperty]
        public partial string? TrackUrl { get; set; }

        /// <summary>
        /// Длительность трека
        /// </summary>
        public string? TrackDuration
        {
            get => field;
            set
            {
                field = (TimeSpan.TryParseExact(value, "mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan trackDuration) && trackDuration > TimeSpan.Zero)
                    ? trackDuration.ToString("mm\\:ss")
                    : null;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? UserComment { get; set; }

        public override Task OnNavigatedTo(object? parameter)
        {
            Refresh();

            return Task.CompletedTask;
        }

        partial void OnOrderTypeChanged(ReviewOrderType value)
        {
            PaymentAmount = null;
        }

        [RelayCommand]
        private async Task Create()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            int? trackDurationSeconds = null;
            if (TrackDuration is not null)
            {
                TimeSpan.TryParseExact(TrackDuration, "mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan trackDuration);
                trackDurationSeconds = (int)trackDuration.TotalSeconds;
            }

            CreateReviewOrderRequest request = new()
            {
                Nickname = Nickname?.Trim() ?? string.Empty,
                OrderType = OrderType,
                TrackUrl = string.IsNullOrWhiteSpace(TrackUrl) ? null : TrackUrl,
                TrackDurationSeconds = trackDurationSeconds,
                PaymentAmount = paymentAmount,
                TopUpProvider = AccountTopUpProvider.Manual,
                UserComment = string.IsNullOrWhiteSpace(UserComment) ? null : UserComment,
            };

            if (await validationService.Check(request))
            {
                await messageService.HandleException(async () =>
                {
                    await reviewOrderHttpClient.Create(_idempotencyKey, request);

                    HideDialog();
                });
            }
        }

        private void Refresh()
        {
            _idempotencyKey = Guid.NewGuid();
            OrderType = ReviewOrderType.Donation;
            Nickname = null;
            PaymentAmount = null;
            TrackUrl = null;
            TrackDuration = null;
            UserComment = null;
        }
    }
}
