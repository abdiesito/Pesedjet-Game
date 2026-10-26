using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class TopBarViewModel : ViewModelBase, IDisposable
{
    private const int AvatarInitialsLength = 2;
    private const int MaximumBadgeCount = 99;
    private const string OverflowBadgeText = "99+";
    private const string ProfileStatsKey = "MainMenu.ProfileStats";
    private const string GuestIndicatorKey = "Guest.Indicator";

    private readonly IPlayerSession _playerSession;
    private readonly IDialogManager _dialogManager;
    private readonly INotificationInbox _notificationInbox;

    private PlayerProfile _player;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingNotifications), nameof(NotificationBadgeText))]
    private int _pendingNotificationCount;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsRegistered => !_playerSession.IsGuest;

    public string GuestIndicatorText => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[GuestIndicatorKey],
        _playerSession.GuestAlias);

    public string Username => _player.Username;

    public string? AvatarIconData => AvatarCatalog.Find(_player.AvatarId)?.IconData;

    public bool HasAvatar => AvatarIconData is not null;

    public string AvatarInitials => Username[..Math.Min(AvatarInitialsLength, Username.Length)];

    public string ProfileStats => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[ProfileStatsKey],
        _player.Points,
        _player.Victories);

    public bool HasPendingNotifications => PendingNotificationCount > 0;

    public string NotificationBadgeText => PendingNotificationCount > MaximumBadgeCount
        ? OverflowBadgeText
        : PendingNotificationCount.ToString(CultureInfo.CurrentCulture);

    public TopBarViewModel(
        IPlayerSession playerSession,
        IDialogManager dialogManager,
        INotificationInbox notificationInbox)
    {
        _playerSession = playerSession;
        _dialogManager = dialogManager;
        _notificationInbox = notificationInbox;
        _player = playerSession.CurrentPlayer ?? new PlayerProfile(string.Empty, 0, 0);
        _pendingNotificationCount = notificationInbox.Items.Count;

        _playerSession.PropertyChanged += HandleSessionChanged;
        _notificationInbox.Items.CollectionChanged += HandleNotificationsChanged;
        LocalizationManager.Instance.LanguageChanged += HandleLanguageChanged;
    }

    public void Dispose()
    {
        _playerSession.PropertyChanged -= HandleSessionChanged;
        _notificationInbox.Items.CollectionChanged -= HandleNotificationsChanged;
        LocalizationManager.Instance.LanguageChanged -= HandleLanguageChanged;
    }

    [RelayCommand]
    private async Task OpenGuestInformationAsync()
    {
        await _dialogManager.ShowDialogAsync<GuestNoticeViewModel, GuestNoticeMode>(GuestNoticeMode.Informative);
    }

    [RelayCommand]
    private async Task OpenMenuAsync()
    {
        await _dialogManager.ShowDialogAsync<HamburgerMenuViewModel>();
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await _dialogManager.ShowDialogAsync<NotificationsViewModel>();
    }

    [RelayCommand]
    private async Task OpenProfileEditorAsync()
    {
        await _dialogManager.ShowDialogAsync<ProfileEditorViewModel>();
    }

    private void HandleSessionChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        _player = _playerSession.CurrentPlayer ?? new PlayerProfile(string.Empty, 0, 0);

        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(AvatarInitials));
        OnPropertyChanged(nameof(AvatarIconData));
        OnPropertyChanged(nameof(HasAvatar));
        OnPropertyChanged(nameof(ProfileStats));
        OnPropertyChanged(nameof(IsGuest));
        OnPropertyChanged(nameof(IsRegistered));
        OnPropertyChanged(nameof(GuestIndicatorText));
    }

    private void HandleNotificationsChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        PendingNotificationCount = _notificationInbox.Items.Count;
    }

    private void HandleLanguageChanged(object? sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(ProfileStats));
        OnPropertyChanged(nameof(GuestIndicatorText));
        OnPropertyChanged(nameof(NotificationBadgeText));
    }
}
