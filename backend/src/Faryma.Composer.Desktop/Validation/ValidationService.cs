using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Validation
{
    public sealed class ValidationService(MessageService messageService)
    {
        public async Task<bool> Check(CreateReviewOrderRequest request)
        {
            SimpleValidator validator = new SimpleValidator()
                .WarnIf(string.IsNullOrWhiteSpace(request.Nickname), "Не задан никнейм пользователя")
                .WarnIf(request.OrderType == ReviewOrderType.Donation && request.PaymentAmount == 0, "Не задана сумма платежа")
                .RequireUrlIfProvided(request.TrackUrl, "Некорректная ссылка на трек");

            if (validator.HasWarnings)
            {
                await ShowWarning(validator.Warnings);
            }

            return !validator.HasWarnings;
        }

        private Task ShowWarning(IEnumerable<string> warnings)
        {
            return messageService.ShowWarnings(warnings, new MessageOptions
            {
                Title = "Некорректные данные",
            });
        }
    }
}