using System.Collections.ObjectModel;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature;
using Faryma.Composer.Desktop.Services.OrderQueueFeature;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.AddTrackUrl;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Cancel;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Complete;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Dto;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Freeze;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.TakeInProgress;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Unfreeze;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Up;
using Faryma.Composer.Desktop.Shared.Dto;
using Faryma.Composer.Desktop.Shared.ViewModels;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.UI.OrderQueueFeature
{
    public sealed partial class OrderQueuePageVM(
        OrderQueueService orderQueueService,
        ComposerStreamService composerStreamService,
        ReviewOrderService reviewOrderService) : ObservableObject
    {
        public OrderQueueService OrderQueueService { get; } = orderQueueService;
        public ReviewOrderType[] OrderTypes { get; } = Enum.GetValues<ReviewOrderType>();

        [ObservableProperty]
        public partial ReviewOrderVM? SelectedOrder { get; set; }

        [ObservableProperty]
        public partial Guid IdempotencyKey { get; set; }

        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? Nickname { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        [ObservableProperty]
        public partial ReviewOrderType OrderType { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [ObservableProperty]
        public partial string? TrackUrl { get; set; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        [ObservableProperty]
        public partial string? PaymentAmount { get; set; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? UserComment { get; set; }

        public ObservableCollection<ComposerStreamVM> Streams { get; } = [];

        [ObservableProperty]
        public partial ComposerStreamVM? SelectedStream { get; set; }

        [ObservableProperty]
        public partial DateOnly DateFrom { get; set; }

        [ObservableProperty]
        public partial DateOnly DateTo { get; set; }

        public Task Initialize() => CurrentWeek();

        private static DateOnly StartOfWeek(DateOnly date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

            return date.AddDays(-1 * diff);
        }

        [RelayCommand]
        private void GenerateOrder()
        {
            Faker faker = new();

            IdempotencyKey = faker.Random.Guid();
            Nickname = faker.Internet.UserName();
            TrackUrl = faker.Internet.Url().OrNull(faker);
            PaymentAmount = faker.Finance.Amount(750, 5000, 0).ToString();
            UserComment = faker.Lorem.Sentence(5, 15).OrNull(faker);
        }

        [RelayCommand]
        private void GenerateIdempotencyKey() => IdempotencyKey = Guid.NewGuid();

        [RelayCommand]
        private void ClearOrder()
        {
            Nickname = null;
            OrderType = ReviewOrderType.Unspecified;
            TrackUrl = null;
            PaymentAmount = null;
            UserComment = null;
        }

        [RelayCommand]
        private async Task CreateReviewOrder()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            await reviewOrderService.Post(IdempotencyKey, new CreateReviewOrderRequest
            {
                Nickname = Nickname,
                OrderType = OrderType,
                TrackUrl = TrackUrl,
                PaymentAmount = paymentAmount,
                UserComment = UserComment,
            });
        }

        [RelayCommand]
        private async Task UpReviewOrder()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            await reviewOrderService.Post(IdempotencyKey, new UpReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
                Nickname = Nickname,
                PaymentAmount = paymentAmount,
            });
        }

        [RelayCommand]
        private async Task AddTrackUrl()
        {
            await reviewOrderService.Post(new AddTrackUrlRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
                TrackUrl = TrackUrl,
            });
        }

        [RelayCommand]
        private async Task TakeOrderInProgress()
        {
            await reviewOrderService.Post(new TakeOrderInProgressRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
            });
        }

        [RelayCommand]
        private async Task CompleteReviewOrder()
        {
            await reviewOrderService.Post(new CompleteReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
                Rating = 20,
            });
        }

        [RelayCommand]
        private async Task FreezeReviewOrder()
        {
            await reviewOrderService.Post(new FreezeReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
            });
        }

        [RelayCommand]
        private async Task UnfreezeReviewOrder()
        {
            await reviewOrderService.Post(new UnfreezeReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
            });
        }

        [RelayCommand]
        private async Task CancelReviewOrder()
        {
            await reviewOrderService.Post(new CancelReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
            });
        }

        [RelayCommand]
        private Task UpdateOrderQueue() => OrderQueueService.UpdateOrderQueue();

        [RelayCommand]
        private async Task CurrentWeek()
        {
            DateFrom = StartOfWeek(DateOnly.FromDateTime(DateTime.Now));
            DateTo = DateFrom.AddDays(6);
            await UpdateStreams();
        }

        [RelayCommand]
        private async Task PreviousWeek()
        {
            DateFrom = DateFrom.AddDays(-6);
            DateTo = DateTo.AddDays(-6);
            await UpdateStreams();
        }

        [RelayCommand]
        private async Task NextWeek()
        {
            DateFrom = DateFrom.AddDays(6);
            DateTo = DateTo.AddDays(6);
            await UpdateStreams();
        }

        private async Task UpdateStreams()
        {
            IEnumerable<ComposerStreamDto> streams = await composerStreamService.Find(DateFrom, DateTo);

            Streams.Clear();

            foreach (ComposerStreamDto dto in streams.OrderBy(x => x.EventDate))
            {
                Streams.Add(new ComposerStreamVM(dto));
            }
        }
    }
}