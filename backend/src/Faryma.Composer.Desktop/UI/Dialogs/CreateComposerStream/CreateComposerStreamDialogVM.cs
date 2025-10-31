using Faryma.Composer.Desktop.Api.ComposerStream;
using Faryma.Composer.Desktop.Navigation;
using Faryma.Composer.Desktop.Validation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class CreateComposerStreamDialogVM(
        ComposerStreamHttpClient composerStreamHttpClient,
        MessageService messageService,
        ValidationService validationService,
        DialogService dialogService) : DialogVM(dialogService)
    {
    }
}