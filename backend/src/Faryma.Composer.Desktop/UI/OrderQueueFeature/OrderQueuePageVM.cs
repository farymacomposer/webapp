using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Requests;
using Faryma.Composer.Desktop.Services.OrderQueueFeature;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.AddTrackUrl;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Cancel;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Complete;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Dto;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Freeze;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.MoveUp;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.TakeInProgress;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Unfreeze;
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
        public OrderQueuePage Page { get; set; } = null!;
        public OrderQueueService OrderQueueService { get; } = orderQueueService;
        public ReviewOrderType[] OrderTypes { get; } = Enum.GetValues<ReviewOrderType>();
        public ComposerStreamType[] StreamTypes { get; } = Enum.GetValues<ComposerStreamType>();

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

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        [ObservableProperty]
        public partial string? Rating { get; set; }

        public StreamContainerVM[] StreamSchedule { get; } =
        [
            new StreamContainerVM(),
            new StreamContainerVM(),
            new StreamContainerVM(),
            new StreamContainerVM(),
            new StreamContainerVM(),
            new StreamContainerVM(),
            new StreamContainerVM(),
        ];

        [ObservableProperty]
        public partial DateOnly SelectedEventDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [ObservableProperty]
        public partial ComposerStreamVM? SelectedStream { get; set; }

        [ObservableProperty]
        public partial DateOnly DateFrom { get; set; }

        [ObservableProperty]
        public partial DateOnly DateTo { get; set; }

        [ObservableProperty]
        public partial ComposerStreamType StreamType { get; set; }

        public Task Initialize() => CurrentWeek();
        public Task ShowDialog(string message) => Page.ShowDialog(message);

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
            OrderType = ReviewOrderType.Donation;
            TrackUrl = faker.Internet.Url();
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
        private async Task MoveUpReviewOrder()
        {
            _ = int.TryParse(PaymentAmount, out int paymentAmount);

            await reviewOrderService.Post(IdempotencyKey, new MoveUpReviewOrderRequest
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
            _ = int.TryParse(Rating, out int rating);

            await reviewOrderService.Post(new CompleteReviewOrderRequest
            {
                ReviewOrderId = SelectedOrder?.Id ?? 0,
                Rating = rating,
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
        private Task CurrentWeek() => UpdateStreamSchedule(StartOfWeek(DateOnly.FromDateTime(DateTime.Now)));

        [RelayCommand]
        private Task PreviousWeek() => UpdateStreamSchedule(DateFrom.AddDays(-7));

        [RelayCommand]
        private Task NextWeek() => UpdateStreamSchedule(DateFrom.AddDays(7));

        private async Task UpdateStreamSchedule(DateOnly dateFrom)
        {
            DateFrom = dateFrom;
            DateTo = dateFrom.AddDays(6);

            IEnumerable<ComposerStreamDto> streams = await composerStreamService.Find(DateFrom, DateTo);

            DateOnly date = dateFrom;
            foreach (StreamContainerVM container in StreamSchedule)
            {
                container.Date = date;
                ComposerStreamDto? dto = streams.FirstOrDefault(x => x.EventDate == date);
                container.Stream = (dto is null) ? null : new ComposerStreamVM(dto);
                date = date.AddDays(1);
            }
        }

        [RelayCommand]
        private Task CreateStream() => UpdateStream(composerStreamService.Post(new CreateStreamRequest
        {
            EventDate = SelectedEventDate,
            Type = StreamType,
        }));

        [RelayCommand]
        private Task StartStream() => UpdateStream(composerStreamService.Post(new StartStreamRequest
        {
            ComposerStreamId = SelectedStream?.Id ?? 0,
        }));

        [RelayCommand]
        private Task CompleteStream() => UpdateStream(composerStreamService.Post(new CompleteStreamRequest
        {
            ComposerStreamId = SelectedStream?.Id ?? 0,
        }));

        [RelayCommand]
        private Task CancelStream() => UpdateStream(composerStreamService.Post(new CancelStreamRequest
        {
            ComposerStreamId = SelectedStream?.Id ?? 0,
        }));

        private async Task UpdateStream(Task<ComposerStreamDto> task)
        {
            try
            {
                ComposerStreamDto dto = await task;
                StreamContainerVM? container = StreamSchedule.FirstOrDefault(x => x.Date == dto.EventDate);
                container?.Stream = new ComposerStreamVM(dto);
            }
            catch (Exception ex)
            {
                await App.ShowDialog(ex.Message);
            }
        }
    }
}