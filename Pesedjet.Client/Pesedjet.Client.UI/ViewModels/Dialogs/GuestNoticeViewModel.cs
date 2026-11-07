using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.Models.DialogParameters;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class GuestNoticeViewModel : DialogViewModelBase, IDialogParameter<GuestNoticeMode>
{
    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly IAuthClient _authClient;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntry))]
    private GuestNoticeMode _mode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand), nameof(ContinueCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasConnectionError;

    public bool IsEntry => Mode == GuestNoticeMode.Entry;

    public GuestNoticeViewModel(INavigator navigator, IPlayerSession playerSession, IAuthClient authClient)
    {
        _navigator = navigator;
        _playerSession = playerSession;
        _authClient = authClient;
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
    private void Continue()
    {
        IsBusy = true;
        HasConnectionError = false;

        BackgroundJob<object?, ClientResult<UserSession>> job = new()
        {
            Work = CreateGuestSessionOnWorker,
            State = null,
            OnCompleted = OnGuestCompleted,
            OnFailed = OnGuestFailed
        };

        BackgroundWork.Start(job);
    }

    private ClientResult<UserSession> CreateGuestSessionOnWorker(object? unused)
    {
        return _authClient.CreateGuestSession();
    }

    private void OnGuestCompleted(ClientResult<UserSession> result, object? unused)
    {
        ApplyGuestResult(result);
    }

    private void OnGuestFailed(Exception error, object? unused)
    {
        ApplyGuestResult(ClientResult<UserSession>.TechnicalFailure(FailureKind.Unexpected));
    }

    private void ApplyGuestResult(ClientResult<UserSession> result)
    {
        IsBusy = false;

        if (result.IsSuccess)
        {
            UserSession session = result.Value!;

            _playerSession.PlayerId = session.PlayerId;
            _playerSession.GuestAlias = session.Username;
            RequestClose();
            _navigator.NavigateAndClear<GameMenuViewModel>();
        }
        else
        {
            HasConnectionError = true;
        }
    }

    private bool CanRespond()
    {
        return !IsBusy;
    }
}
