using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Desktop.Api.ReviewOrder;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Utils;
using Faryma.Composer.Desktop.Validation;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Desktop.UI
{
    public enum CreateReviewOrderStep
    {
        OrderType = 0,
        Details = 1,
    }

    public sealed partial class CreateReviewOrderDialogVM(
        ReviewOrderHttpClient reviewOrderHttpClient,
        MessageService messageService,
        ValidationService validationService,
        DialogService dialogService
        ) : DialogVM(dialogService)
    {
        private Guid _idempotencyKey;

        public ReviewOrderTypeOptionVM[] OrderTypes { get; } =
        [
            new(ReviewOrderType.Donation),
            new(ReviewOrderType.OutOfQueue),
            new(ReviewOrderType.Free),
            new(ReviewOrderType.Charity),
        ];

        public bool IsPaymentAmountVisible => OrderType is ReviewOrderType.Donation;
        public bool IsUserEntitlementIdVisible => OrderType is ReviewOrderType.Free or ReviewOrderType.OutOfQueue;
        public bool IsOrderTypeStepVisible => CurrentStep is CreateReviewOrderStep.OrderType;
        public bool IsDetailsStepVisible => CurrentStep is CreateReviewOrderStep.Details;
        public bool IsBackButtonVisible => CurrentStep is not CreateReviewOrderStep.OrderType;
        public bool IsNextButtonVisible => CurrentStep is CreateReviewOrderStep.OrderType;
        public bool IsCreateButtonVisible => CurrentStep is CreateReviewOrderStep.Details;

        /// <summary>
        /// Тип заказа
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPaymentAmountVisible))]
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
        /// Id жетона пользователя
        /// </summary>
        [ObservableProperty]
        public partial string? UserEntitlementId { get; set; }

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOrderTypeStepVisible))]
        [NotifyPropertyChangedFor(nameof(IsDetailsStepVisible))]
        [NotifyPropertyChangedFor(nameof(IsBackButtonVisible))]
        [NotifyPropertyChangedFor(nameof(IsNextButtonVisible))]
        [NotifyPropertyChangedFor(nameof(IsCreateButtonVisible))]
        [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
        private partial CreateReviewOrderStep CurrentStep { get; set; }

        public override Task OnNavigatedTo(object? parameter)
        {
            Refresh();

            return Task.CompletedTask;
        }

        partial void OnOrderTypeChanged(ReviewOrderType value)
        {
            foreach (ReviewOrderTypeOptionVM orderType in OrderTypes)
            {
                orderType.IsSelected = orderType.Type == value;
            }

            PaymentAmount = null;
            UserEntitlementId = null;
            OnPropertyChanged(nameof(IsUserEntitlementIdVisible));
        }

        [RelayCommand]
        private void SelectOrderType(ReviewOrderTypeOptionVM? orderType)
        {
            if (orderType is null)
            {
                return;
            }

            OrderType = orderType.Type;
        }

        [RelayCommand]
        private void GoBack() => CurrentStep = CreateReviewOrderStep.OrderType;

        [RelayCommand]
        private void GoNext() => CurrentStep = CreateReviewOrderStep.Details;

        [RelayCommand]
        private async Task Create()
        {
            _ = long.TryParse(PaymentAmount, out long paymentAmount);
            _ = long.TryParse(UserEntitlementId, out long userEntitlementId);

            int? trackDurationSeconds = null;
            if (TrackDuration is not null)
            {
                TimeSpan.TryParseExact(TrackDuration, "mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan trackDuration);
                trackDurationSeconds = (int)trackDuration.TotalSeconds;
            }

            CreateReviewOrderRequestBase request = CreateRequest(paymentAmount, userEntitlementId, trackDurationSeconds);

            if (await validationService.Check(request))
            {
                await messageService.HandleException(async () =>
                {
                    await SendCreateRequest(request);

                    HideDialog();
                });
            }
        }

        private CreateReviewOrderRequestBase CreateRequest(long paymentAmount, long userEntitlementId, int? trackDurationSeconds)
        {
            string nickname = Nickname?.Trim() ?? string.Empty;
            string? trackUrl = string.IsNullOrWhiteSpace(TrackUrl) ? null : TrackUrl;
            string? userComment = string.IsNullOrWhiteSpace(UserComment) ? null : UserComment;
            bool useToken = (OrderType is ReviewOrderType.Free or ReviewOrderType.OutOfQueue)
                && !string.IsNullOrWhiteSpace(UserEntitlementId);

            if (useToken)
            {
                return new CreateTokenReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = trackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    UserEntitlementId = userEntitlementId,
                    UserComment = userComment,
                };
            }

            return OrderType switch
            {
                ReviewOrderType.Donation => new CreateDonationReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = trackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    PaymentAmount = paymentAmount,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    UserComment = userComment,
                },
                ReviewOrderType.OutOfQueue => new CreateOutOfQueueReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = trackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    UserComment = userComment,
                },
                ReviewOrderType.Free => new CreateFreeReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = trackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    UserComment = userComment,
                },
                ReviewOrderType.Charity => new CreateCharityReviewOrderRequest
                {
                    UserNickname = nickname,
                    TrackUrl = trackUrl,
                    TrackDurationSeconds = trackDurationSeconds,
                    UserComment = userComment,
                },
                _ => throw new InvalidOperationException($"Unsupported review order type '{OrderType}'"),
            };
        }

        private Task<ReviewOrderDto> SendCreateRequest(CreateReviewOrderRequestBase request)
        {
            return request switch
            {
                CreateDonationReviewOrderRequest donationRequest => reviewOrderHttpClient.CreateDonation(_idempotencyKey, donationRequest),
                CreateOutOfQueueReviewOrderRequest outOfQueueRequest => reviewOrderHttpClient.CreateOutOfQueue(_idempotencyKey, outOfQueueRequest),
                CreateFreeReviewOrderRequest freeRequest => reviewOrderHttpClient.CreateFree(_idempotencyKey, freeRequest),
                CreateTokenReviewOrderRequest tokenRequest => reviewOrderHttpClient.CreateToken(_idempotencyKey, tokenRequest),
                CreateCharityReviewOrderRequest charityRequest => reviewOrderHttpClient.CreateCharity(_idempotencyKey, charityRequest),
                _ => throw new InvalidOperationException($"Unsupported create request type '{request.GetType().Name}'"),
            };
        }

        private void Refresh()
        {
            _idempotencyKey = Guid.NewGuid();
            CurrentStep = CreateReviewOrderStep.OrderType;
            OrderType = ReviewOrderType.Donation;
            Nickname = null;
            PaymentAmount = null;
            UserEntitlementId = null;
            TrackUrl = null;
            TrackDuration = null;
            UserComment = null;
        }
    }

    public sealed partial class ReviewOrderTypeOptionVM(ReviewOrderType type) : ObservableObject
    {
        public ReviewOrderType Type { get; } = type;
        public string Title => EnumHelper.GetDescription(Type) ?? Type.ToString();

        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }
}
