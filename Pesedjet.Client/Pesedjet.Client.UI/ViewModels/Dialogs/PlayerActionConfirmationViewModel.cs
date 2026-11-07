using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Models.DialogParameters;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class PlayerActionConfirmationViewModel : DialogViewModelBase, IDialogParameter<PlayerActionRequest>
{
    private const string RemovePrefix = "PlayerAction.Remove";
    private const string BlockPrefix = "PlayerAction.Block";
    private const string UnblockPrefix = "PlayerAction.Unblock";
    private const string KickPrefix = "PlayerAction.Kick";
    private const string LeavePrefix = "PlayerAction.Leave";
    private const string PlainSuffix = "Plain";
    private const string AbandonPrefix = "PlayerAction.AbandonMatch";
    private const string AtumRaPrefix = "PlayerAction.PlayAtumRa";
    private const string CancelSuffix = "Cancel";
    private const string CommonCancelKey = "Common.Cancel";

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _confirmLabel = string.Empty;

    [ObservableProperty]
    private bool _isDestructive;

    [ObservableProperty]
    private string _cancelLabel = string.Empty;

    private PlayerActionRequest? _request;

    public void Initialize(PlayerActionRequest request)
    {
        _request = request;
        string prefix = GetPrefix(request.Action);
        string messageKey = prefix + "Message";

        // Leaving a lobby as a plain participant has no host to hand the lobby over to.
        if (request.Action == PlayerAction.LeaveLobby && request.Username.Length == 0)
        {
            messageKey += PlainSuffix;
        }

        Title = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[prefix + "Title"],
            request.Username);
        Message = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[messageKey],
            request.Username);
        ConfirmLabel = LocalizationManager.Instance[prefix + "Confirm"];
        IsDestructive = request.Action is PlayerAction.BlockPlayer
            or PlayerAction.KickFromLobby
            or PlayerAction.PlayAtumRa;
        CancelLabel = request.Action is PlayerAction.AbandonMatch or PlayerAction.PlayAtumRa
            ? LocalizationManager.Instance[prefix + CancelSuffix]
            : LocalizationManager.Instance[CommonCancelKey];
    }

    protected override void OnLanguageChanged()
    {
        if (_request is not null)
        {
            Initialize(_request);
        }
    }

    private static string GetPrefix(PlayerAction action)
    {
        string prefix = UnblockPrefix;

        if (action == PlayerAction.RemoveFriend)
        {
            prefix = RemovePrefix;
        }
        else if (action == PlayerAction.BlockPlayer)
        {
            prefix = BlockPrefix;
        }
        else if (action == PlayerAction.KickFromLobby)
        {
            prefix = KickPrefix;
        }
        else if (action == PlayerAction.LeaveLobby)
        {
            prefix = LeavePrefix;
        }
        else if (action == PlayerAction.AbandonMatch)
        {
            prefix = AbandonPrefix;
        }
        else if (action == PlayerAction.PlayAtumRa)
        {
            prefix = AtumRaPrefix;
        }

        return prefix;
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand]
    private void Confirm()
    {
        IsConfirmed = true;
        RequestClose();
    }
}
