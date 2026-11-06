using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Models.Social;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class NotificationsViewModel : DialogViewModelBase, IDisposable
{
    private const string PendingOneKey = "Notifications.PendingOne";
    private const string PendingManyKey = "Notifications.PendingMany";
    private const string FriendRequestKey = "Notifications.FriendRequest";
    private const string LobbyInvitationKey = "Notifications.LobbyInvitation";
    private const string AcceptKey = "Notifications.Accept";
    private const string AcceptingKey = "Notifications.Accepting";
    private const string JoinKey = "Notifications.Join";
    private const string JoiningKey = "Notifications.Joining";

    private readonly INotificationInbox _inbox;
    private readonly TimeProvider _timeProvider;

    public ObservableCollection<NotificationItem> Items => _inbox.Items;

    public bool IsEmpty => Items.Count == 0;

    public string PendingText => Items.Count == 1
        ? LocalizationManager.Instance[PendingOneKey]
        : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[PendingManyKey], Items.Count);

    public NotificationsViewModel(INotificationInbox inbox, TimeProvider timeProvider)
    {
        _inbox = inbox;
        _timeProvider = timeProvider;

        foreach (NotificationItem item in Items)
        {
            FillTexts(item);
        }

        Items.CollectionChanged += HandleItemsChanged;
    }

    public void Dispose()
    {
        Items.CollectionChanged -= HandleItemsChanged;
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }

    [RelayCommand]
    private async Task AcceptAsync(NotificationItem item)
    {
        item.IsProcessing = true;

        try
        {
            await AnswerOnServerAsync(item, true);
        }
        finally
        {
            item.IsProcessing = false;
        }

        // TODO: A lobby invitation must also leave this panel and open the waiting lobby (GUI-008, CU-010).
        Items.Remove(item);
    }

    [RelayCommand]
    private async Task RejectAsync(NotificationItem item)
    {
        item.IsProcessing = true;

        try
        {
            await AnswerOnServerAsync(item, false);
        }
        finally
        {
            item.IsProcessing = false;
        }

        Items.Remove(item);
    }

    private static Task AnswerOnServerAsync(NotificationItem item, bool isAccepted)
    {
        // TODO: Send the answer to the server (accept or reject the friend request or lobby invitation, CU-010). If
        // the lobby already started, mark the item as expired instead of removing it.
        return Task.CompletedTask;
    }

    protected override void OnLanguageChanged()
    {
        foreach (NotificationItem item in Items)
        {
            FillTexts(item);
        }

        base.OnLanguageChanged();
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (eventArgs.NewItems is not null)
        {
            foreach (NotificationItem item in eventArgs.NewItems)
            {
                FillTexts(item);
            }
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(PendingText));
    }

    private void FillTexts(NotificationItem item)
    {
        item.AcceptCommand = AcceptCommand;
        item.RejectCommand = RejectCommand;

        if (item.IsLobbyInvitation)
        {
            item.Message = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[LobbyInvitationKey],
                item.LobbyCode,
                item.Occupancy,
                item.Capacity);
            item.PrimaryLabel = LocalizationManager.Instance[JoinKey];
            item.ProcessingLabel = LocalizationManager.Instance[JoiningKey];
        }
        else
        {
            item.Message = LocalizationManager.Instance[FriendRequestKey];
            item.PrimaryLabel = LocalizationManager.Instance[AcceptKey];
            item.ProcessingLabel = LocalizationManager.Instance[AcceptingKey];
        }

        item.TimeAgoText = RelativeTimeFormatter.Format(item.ReceivedAt, _timeProvider.GetUtcNow());
    }
}
