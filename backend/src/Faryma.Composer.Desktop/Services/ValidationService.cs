using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Validation;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Services
{
    public sealed class ValidationService(MessageService messageService)
    {
        public async Task<bool> Check(CreateReviewOrderRequest request)
        {
            SimpleValidator validator = new SimpleValidator()
                .Check(string.IsNullOrWhiteSpace(request.Nickname), "Не задан псевдоним пользователя")
                .CheckOptionalUrl(request.TrackUrl, "Некорректная ссылка на трек");

            if (request.OrderType == ReviewOrderType.Donation)
            {
                validator.Check(request.PaymentAmount == 0, "Не задана сумма платежа");
            }

            if (validator.HasWarnings)
            {
                await ShowWarning(validator.Warnings);
            }

            return !validator.HasWarnings;
        }

        private Task ShowWarning(IEnumerable<string> warnings)
        {
            return messageService.ShowWarning(warnings, new MessageOptions
            {
                Title = "Некорректные данные",
            });
        }
    }
}