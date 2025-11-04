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

        public Task HandleException(Func<Task> action, string? message = null) => HandleException(action(), message);
        public Task HandleException(Action action, string? message = null) => HandleException(async () => action(), message);

        public async Task HandleException(Task task, string? message = null)
        {
            try
            {
                if (!task.IsCompleted)
                {
                    messenger.Send<ShowProgressCommand>();
                }

                await task;
            }
            catch (ApiException ex)
            {
                logger.LogWarning(ex, "{message}\n{@Result}", message, ex.Result);

                await ShowMessage(new MessageOptions
                {
                    Title = "Ошибка",
                    Message = message,
                    SubMessage = ex.Result.Message
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{message}", message);

                await ShowWarning(ex, new MessageOptions
                {
                    Title = "Ошибка",
                    Message = message
                });
            }
            finally
            {
                messenger.Send<HideProgressCommand>();
            }
        }

        public Task ShowMessage(MessageOptions options) => ShowMessageInternal(options);
        public Task<MessageDialogResponse> ShowQuestion(MessageOptions options) => ShowMessageInternal(options);
        public Task ShowWarning(Exception ex, MessageOptions options) => ShowMessageInternal(options, ex.ToString());
        public Task ShowWarning(IEnumerable<string> warnings, MessageOptions options) => ShowMessageInternal(options, string.Join("\n", warnings));

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