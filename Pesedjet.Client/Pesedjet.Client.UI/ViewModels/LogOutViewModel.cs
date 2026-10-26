using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class LogOutViewModel : DialogViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand), nameof(ConfirmLogOutCommand))]
    private bool _isBusy;

    public LogOutViewModel(INavigator navigator, IPlayerSession playerSession)
    {
        _navigator = navigator;
        _playerSession = playerSession;
    }

    // The dialog cannot be dismissed while the server request is in flight.
    public override void Dismiss()
    {
        if (!IsBusy)
        {
            RequestClose();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task ConfirmLogOutAsync()
    {
        IsBusy = true;

        try
        {
            await RequestServerLogOutAsync();
        }
        finally
        {
            IsBusy = false;
        }

        _playerSession.CurrentPlayer = null;
        RequestClose();
        _navigator.NavigateAndClear<AccessMenuViewModel>();
    }

    private bool CanRespond()
    {
        return !IsBusy;
    }

    private static Task RequestServerLogOutAsync()
    {
        // TODO: Send LOGOUT_REQUEST to the server with the 15 second timeout (CU-019), forget the remembered session of
        // this device (RN18) and, if the server does not answer, clear the session locally anyway (RNF-13).
        return Task.CompletedTask;
    }
}
