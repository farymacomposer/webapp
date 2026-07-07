using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Services;
using Faryma.Composer.Desktop.ViewModels;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class OrderQueuePageVM(
        OrderQueueService orderQueueService,
        ComposerStreamHttpClient composerStreamService,
        ReviewOrderHttpClient reviewOrderService) : ObservableObject
    {
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
        /// Длительность трека в секундах
        /// </summary>
        [ObservableProperty]
        public partial int TrackDurationSeconds { get; set; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        [ObservableProperty]
        public partial string? PaymentAmount { get; set; }

        /// <summary>
        /// Id жетона пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? UserEntitlementId { get; set; }

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
            TrackDurationSeconds = 60;
            PaymentAmount = faker.Finance.Amount(750, 5000, 0).ToString();
            UserEntitlementId = null;
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
            TrackDurationSeconds = 0;
            PaymentAmount = null;
            UserEntitlementId = null;
            UserComment = null;
        }

        [RelayCommand]
        private async Task CreateReviewOrder()
        {
            _ = long.TryParse(PaymentAmount, out long paymentAmount);
            _ = long.TryParse(UserEntitlementId, out long userEntitlementId);

            string nickname = Nickname ?? string.Empty;
            int? trackDurationSeconds = string.IsNullOrWhiteSpace(TrackUrl) ? null : TrackDurationSeconds;
            bool useToken = (OrderType is ReviewOrderType.Free or ReviewOrderType.OutOfQueue)
                && !string.IsNullOrWhiteSpace(UserEntitlementId);

            if (useToken)
            {
                await reviewOrderService.CreateToken(IdempotencyKey, new CreateTokenReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = TrackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    UserEntitlementId = userEntitlementId,
                    UserComment = UserComment,
                });

                return;
            }

            switch (OrderType)
            {
                case ReviewOrderType.Donation:
                    await reviewOrderService.CreateDonation(IdempotencyKey, new CreateDonationReviewOrderRequest
                    {
                        UserNickname = nickname,
                        TrackUrl = TrackUrl,
                        TrackDurationSeconds = trackDurationSeconds,
                        PaymentAmount = paymentAmount,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        UserComment = UserComment,
                    });
                    break;

                case ReviewOrderType.OutOfQueue:
                    await reviewOrderService.CreateOutOfQueue(IdempotencyKey, new CreateOutOfQueueReviewOrderRequest
                    {
                        UserNickname = nickname,
                        TrackUrl = TrackUrl,
                        TrackDurationSeconds = trackDurationSeconds,
                        UserComment = UserComment,
                    });
                    break;

                case ReviewOrderType.Free:
                    await reviewOrderService.CreateFree(IdempotencyKey, new CreateFreeReviewOrderRequest
                    {
                        UserNickname = nickname,
                        TrackUrl = TrackUrl,
                        TrackDurationSeconds = trackDurationSeconds,
                        UserComment = UserComment,
                    });
                    break;

                case ReviewOrderType.Charity:
                    await reviewOrderService.CreateCharity(IdempotencyKey, new CreateCharityReviewOrderRequest
                    {
                        UserNickname = nickname,
                        TrackUrl = TrackUrl,
                        TrackDurationSeconds = trackDurationSeconds,
                        UserComment = UserComment,
                    });
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported review order type '{OrderType}'");
            }
        }

        [RelayCommand]
        private Task<ReviewOrderDto> AddTrackUrl() => reviewOrderService.AddTrackUrl(SelectedOrder?.Id ?? 0, TrackUrl!, TrackDurationSeconds);

        [RelayCommand]
        private Task TakeOrderInProgress() => reviewOrderService.TakeOrderInProgress(SelectedOrder?.Id ?? 0);

        [RelayCommand]
        private async Task CompleteReviewOrder()
        {
            _ = int.TryParse(Rating, out int rating);

            await reviewOrderService.Complete(SelectedOrder?.Id ?? 0, rating);
        }

        [RelayCommand]
        private Task FreezeReviewOrder() => reviewOrderService.Freeze(SelectedOrder?.Id ?? 0);

        [RelayCommand]
        private Task UnfreezeReviewOrder() => reviewOrderService.Unfreeze(SelectedOrder?.Id ?? 0);

        [RelayCommand]
        private Task CancelReviewOrder() => reviewOrderService.Cancel(SelectedOrder?.Id ?? 0);

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
        private Task CreateStream() => UpdateStream(composerStreamService.Create(SelectedEventDate, StreamType));

        [RelayCommand]
        private Task StartStream() => UpdateStream(composerStreamService.Start(SelectedStream?.Id ?? 0));

        [RelayCommand]
        private Task CompleteStream() => UpdateStream(composerStreamService.Complete(SelectedStream?.Id ?? 0));

        [RelayCommand]
        private Task CancelStream() => UpdateStream(composerStreamService.Cancel(SelectedStream?.Id ?? 0));

        private async Task UpdateStream(Task<ComposerStreamDto> task)
        {
            ComposerStreamDto dto = await task;
            StreamContainerVM? container = StreamSchedule.FirstOrDefault(x => x.Date == dto.EventDate);
            container?.Stream = new ComposerStreamVM(dto);
        }
    }
}
