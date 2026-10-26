using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels;

public partial class BlockedPlayersViewModel : ViewModelBase, IDisposable
{
    private const string CountOneKey = "Blocked.CountOne";
    private const string CountManyKey = "Blocked.CountMany";
    private const string BlockedOnKey = "Blocked.BlockedOn";
    private const string UnblockedToastKey = "Blocked.UnblockedToast";

    private readonly ISocialCircle _socialCircle;
    private readonly IDialogManager _dialogManager;
    private readonly IToastManager _toastManager;

    public ObservableCollection<BlockedPlayerItem> BlockedPlayers => _socialCircle.BlockedPlayers;

    public bool IsEmpty => BlockedPlayers.Count == 0;

    public string CountText => BlockedPlayers.Count == 1
        ? LocalizationManager.Instance[CountOneKey]
        : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[CountManyKey], BlockedPlayers.Count);

    public BlockedPlayersViewModel(
        ISocialCircle socialCircle,
        IDialogManager dialogManager,
        IToastManager toastManager)
    {
        _socialCircle = socialCircle;
        _dialogManager = dialogManager;
        _toastManager = toastManager;

        foreach (BlockedPlayerItem blockedPlayer in BlockedPlayers)
        {
            FillBlockedPlayer(blockedPlayer);
        }

        BlockedPlayers.CollectionChanged += HandleBlockedPlayersChanged;
    }

    public void Dispose()
    {
        BlockedPlayers.CollectionChanged -= HandleBlockedPlayersChanged;
    }

    [RelayCommand]
    private async Task UnblockAsync(BlockedPlayerItem blockedPlayer)
    {
        PlayerActionConfirmationViewModel dialog =
            await _dialogManager.ShowDialogAsync<PlayerActionConfirmationViewModel, PlayerActionRequest>(
                new PlayerActionRequest(PlayerAction.UnblockPlayer, blockedPlayer.Username));

        if (dialog.IsConfirmed)
        {
            // TODO: Send the unblock to the server (CU-009); unblocking does not restore the friendship.
            BlockedPlayers.Remove(blockedPlayer);
            _toastManager.Show(string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[UnblockedToastKey],
                blockedPlayer.Username));
        }
    }

    protected override void OnLanguageChanged()
    {
        foreach (BlockedPlayerItem blockedPlayer in BlockedPlayers)
        {
            FillBlockedPlayer(blockedPlayer);
        }

        base.OnLanguageChanged();
    }

    private void HandleBlockedPlayersChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (eventArgs.NewItems is not null)
        {
            foreach (BlockedPlayerItem blockedPlayer in eventArgs.NewItems)
            {
                FillBlockedPlayer(blockedPlayer);
            }
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CountText));
    }

    private void FillBlockedPlayer(BlockedPlayerItem blockedPlayer)
    {
        blockedPlayer.UnblockCommand = UnblockCommand;
        blockedPlayer.BlockedOnText = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[BlockedOnKey],
            blockedPlayer.BlockedAt.ToLocalTime().ToString("d", CultureInfo.CurrentCulture));
    }
}
