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
            Message.Text = options.Message;
            SubMessage.Text = options.SubMessage;
            FirstButton.Content = options.FirstButtonText;
            SecondButton.Content = options.SecondButtonText;
            FirstButton.Visibility = string.IsNullOrEmpty(options.FirstButtonText) ? Visibility.Collapsed : Visibility.Visible;
            SecondButton.Visibility = string.IsNullOrEmpty(options.SecondButtonText) ? Visibility.Collapsed : Visibility.Visible;
        }

        public Task<MessageDialogResponse> WaitResponse() => _responseAwaiter.Task;
        private void FirstButton_Click(object _, RoutedEventArgs __) => _responseAwaiter.SetResult(MessageDialogResponse.FirstButton);
        private void SecondButton_Click(object _, RoutedEventArgs __) => _responseAwaiter.SetResult(MessageDialogResponse.SecondButton);
    }
}