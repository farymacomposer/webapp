using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateReviewOrderDialogVM(
        DialogService dialogService,
        ReviewOrderHttpClient reviewOrderClient
        ) : DialogVM(dialogService)
    {
        public ReviewOrderType[] OrderTypes { get; } =
        [
            ReviewOrderType.Donation,
            ReviewOrderType.OutOfQueue,
            ReviewOrderType.Free,
            ReviewOrderType.Charity,
            ReviewOrderType.Custom,
        ];

        [ObservableProperty]
        public partial Guid IdempotencyKey { get; set; }

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

        [RelayCommand]
        private async Task Create()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            await reviewOrderClient.Create(IdempotencyKey, new CreateReviewOrderRequest
            {
                Nickname = Nickname,
                OrderType = OrderType,
                TrackUrl = TrackUrl,
                PaymentAmount = paymentAmount,
                UserComment = UserComment,
            });
        }

        private void Refresh()
        {
            IdempotencyKey = Guid.NewGuid();
            OrderType = ReviewOrderType.Donation;
            Nickname = null;
            PaymentAmount = null;
            TrackUrl = null;
            UserComment = null;
        }
    }
}