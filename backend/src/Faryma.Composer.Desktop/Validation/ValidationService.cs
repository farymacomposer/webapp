using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Desktop.Navigation;

namespace Faryma.Composer.Desktop.Validation
{
    public sealed class ValidationService(MessageService messageService)
    {
        public async Task<bool> Check(CreateReviewOrderRequestBase request)
        {
            List<ValidationResult> results = [];
            Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);

            if (string.IsNullOrWhiteSpace(request.UserNickname))
            {
                results.Add(new ValidationResult("Не задан никнейм пользователя"));
            }

            await ShowWarning(results);

            return results.Count == 0;
        }

        private async Task ShowWarning(List<ValidationResult> results)
        {
            if (results.Count > 0)
            {
                await messageService.ShowWarnings(
                    results.Select(result => result.ErrorMessage ?? "Некорректные данные").Distinct(),
                    new MessageOptions
                    {
                        Title = "Некорректные данные",
                    });
            }
        }
    }
}
