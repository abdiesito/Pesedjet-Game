using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GuestNoticeViewModel : DialogViewModelBase, IDialogParameter<GuestNoticeMode>
{
    private const string AliasPrefix = "ANONIMO";
    private const int AliasNumberMaximum = 1000;

    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntry))]
    private GuestNoticeMode _mode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand), nameof(ContinueCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasConnectionError;

    public bool IsEntry => Mode == GuestNoticeMode.Entry;

    public GuestNoticeViewModel(INavigator navigator, IPlayerSession playerSession)
    {
        _navigator = navigator;
        _playerSession = playerSession;
    }

    public void Initialize(GuestNoticeMode parameter)
    {
        Mode = parameter;
    }

    // Back is not allowed while the identity is being created, so no half-made identity is left (CU-003).
    public override void Dismiss()
    {
        if (!IsBusy)
        {
            RequestClose();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void Back()
    {
        RequestClose();
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task ContinueAsync()
    {
        IsBusy = true;
        HasConnectionError = false;

        try
        {
            _playerSession.GuestAlias = await RequestGuestIdentityAsync();
        }
        finally
        {
            IsBusy = false;
        }

        RequestClose();
        _navigator.NavigateAndClear<GameMenuViewModel>();
    }

    private bool CanRespond()
    {
        return !IsBusy;
    }

    private static Task<string> RequestGuestIdentityAsync()
    {
        // TODO: Send GUEST_LOGIN_REQUEST to the server with the 15 second timeout (CU-003): the server creates the
        // identity ANONIMOxxx. On a communication failure (EX-01) set HasConnectionError and stay in the notice.
        int number = Random.Shared.Next(AliasNumberMaximum);

        return Task.FromResult(AliasPrefix + number.ToString("D3"));
    }
}
