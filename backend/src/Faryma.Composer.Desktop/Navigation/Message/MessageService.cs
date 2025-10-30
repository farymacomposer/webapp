using CommunityToolkit.Mvvm.Messaging;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Messages.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed class MessageService(IMessenger messenger, ILogger<MessageService> logger)
    {
        private ContentControl _frame = null!;

        public void SetFrame(ContentControl frame) => _frame = frame;

        public Task HandleException(Func<Task> action) => HandleException(action());
        public Task HandleException(Action action) => HandleException(async () => action());

        public async Task HandleException(Task task)
        {
            messenger.Send<ShowProgressCommand>();

            try
            {
                await task;
            }
            catch (ApiException ex)
            {
                logger.LogWarning("{@Result}", ex.Result);
                await ShowMessage(new MessageOptions { Title = "Ошибка", Message = ex.Result.Message });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "");
                await ShowWarning(ex, new MessageOptions { Title = "Ошибка" });
            }
            finally
            {
                messenger.Send<HideProgressCommand>();
            }
        }

        public Task ShowMessage(MessageOptions options) => ShowMessageInternal(options);
        public Task<MessageDialogResponse> ShowQuestion(MessageOptions options) => ShowMessageInternal(options);
        public Task ShowWarning(Exception ex, MessageOptions options) => ShowMessageInternal(options, ex.ToString());
        public Task ShowWarning(IEnumerable<string> warnings, MessageOptions options) => ShowMessageInternal(options, string.Join("\n\n", warnings));

        private async Task<MessageDialogResponse> ShowMessageInternal(MessageOptions options, string? subMessage = null)
        {
            options.SubMessage = subMessage;
            MessageDialog dialog = new(options);
            _frame.Content = dialog;

            MessageDialogResponse response = await dialog.WaitResponse();

            _frame.Content = null;

            return response;
        }
    }
}