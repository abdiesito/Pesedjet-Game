using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GuestExitViewModel : DialogViewModelBase
{
    private const string AliasNoticeKey = "GuestExit.AliasNotice";

    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;

    public string AliasNotice => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[AliasNoticeKey],
        _playerSession.GuestAlias);

    public GuestExitViewModel(INavigator navigator, IPlayerSession playerSession)
    {
        _navigator = navigator;
        _playerSession = playerSession;
    }

    [RelayCommand]
    private void Stay()
    {
        RequestClose();
    }

    [RelayCommand]
    private void SignUp()
    {
        DiscardGuestIdentity();
        RequestClose();
        _navigator.NavigateAndClear<AccessMenuViewModel>();
        _navigator.NavigateTo<RegisterAccountViewModel>();
    }

    [RelayCommand]
    private void Leave()
    {
        DiscardGuestIdentity();
        RequestClose();
        _navigator.NavigateAndClear<AccessMenuViewModel>();
    }

    private void DiscardGuestIdentity()
    {
        // TODO: Tell the server to discard the guest identity (CU-003 FA-04); the points of a guest never pass to a
        // new account (RP-05).
        _playerSession.GuestAlias = null;
    }
}
