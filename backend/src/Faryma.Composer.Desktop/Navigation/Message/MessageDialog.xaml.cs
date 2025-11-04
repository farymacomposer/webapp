using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed partial class MessageDialog : UserControl
    {
        private readonly TaskCompletionSource<MessageDialogResponse> _responseAwaiter = new();

        public MessageDialog(MessageOptions options)
        {
            InitializeComponent();

            Title.Text = options.Title;

            Message.Visibility = GetVisibility(options.Message);
            Message.Text = options.Message;

            SubMessage.Visibility = GetVisibility(options.SubMessage);
            SubMessage.Text = options.SubMessage;

            FirstButton.Visibility = GetVisibility(options.FirstButtonText);
            FirstButton.Content = options.FirstButtonText;

            SecondButton.Visibility = GetVisibility(options.SecondButtonText);
            SecondButton.Content = options.SecondButtonText;
        }

        public Task<MessageDialogResponse> WaitResponse() => _responseAwaiter.Task;
        private static Visibility GetVisibility(string? value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        private void FirstButton_Click(object _, RoutedEventArgs __) => _responseAwaiter.SetResult(MessageDialogResponse.FirstButton);
        private void SecondButton_Click(object _, RoutedEventArgs __) => _responseAwaiter.SetResult(MessageDialogResponse.SecondButton);
    }
}