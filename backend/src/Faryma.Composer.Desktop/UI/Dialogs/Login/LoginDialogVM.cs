using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Faryma.Composer.Desktop.Auth;
using Faryma.Composer.Desktop.Navigation;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class LoginDialogVM(
        AuthenticationService authenticationService,
        MessageService messageService,
        DialogService dialogService) : DialogVM(dialogService)
    {
        /// <summary>
        /// Логин
        /// </summary>
        [ObservableProperty]
        public partial string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль
        /// </summary>
        [ObservableProperty]
        public partial string Password { get; set; } = string.Empty;

        /// <summary>
        /// Событие успешного входа
        /// </summary>
        public event EventHandler? LoginSucceeded;

        public override Task OnNavigatedTo(object? parameter)
        {
            Password = string.Empty;

            return Task.CompletedTask;
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private Task Login() => messageService.HandleException(async () =>
        {
            await authenticationService.Login(UserName, Password);
            await App.InitializeAuthenticatedSession();

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
            HideDialog();
        }, "Не удалось выполнить вход");

        partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
        partial void OnUserNameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    }
}