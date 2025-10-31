using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Validation;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateReviewOrderDialogVM(
        ReviewOrderHttpClient reviewOrderClient,
        MessageService messageService,
        ValidationService validationService,
        DialogService dialogService) : DialogVM(dialogService)
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
            OnPropertyChanged(nameof(IsPaymentAmountEnabled));
        }

        [RelayCommand]
        private async Task Create()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            CreateReviewOrderRequest request = new()
            {
                Nickname = Nickname,
                OrderType = OrderType,
                TrackUrl = TrackUrl,
                PaymentAmount = paymentAmount,
                UserComment = UserComment,
            };

            if (await validationService.Check(request))
            {
                await messageService.HandleException(async () =>
                {
                    await reviewOrderClient.Create(_idempotencyKey, request);

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
            UserComment = null;
        }
    }
}