using System;
using Microsoft.Extensions.DependencyInjection;
using Pesedjet.Client.Networking.Proxies;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Clipboard;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Matches;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;
using Pesedjet.Client.UI.ViewModels;
using Pesedjet.Client.UI.Views;

namespace Pesedjet.Client.UI.Utilities.Navigation;

public static class ViewDependencyInjectionSetup
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        services.AddCoreUtilities()
                .AddNetworkingClients()
                .AddDialogSystem()
                .AddViews()
                .AddViewModels();

        return services.BuildServiceProvider();
    }

    private static IServiceCollection AddCoreUtilities(this IServiceCollection services)
    {
        services.AddSingleton<INavigator, Navigator>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPlayerSession, PlayerSession>();
        services.AddSingleton<INotificationInbox, NotificationInbox>();
        services.AddSingleton<ISocialCircle, SocialCircle>();
        services.AddSingleton<IRoomSession, RoomSession>();
        services.AddSingleton<IClipboardManager, ClipboardManager>();
        services.AddSingleton<IToastManager, ToastManager>();
        services.AddSingleton<UserSettingsStore>();
        services.AddSingleton<ISettingsProtector, AesGcmSettingsProtector>();
        
        return services;
    }

    private static IServiceCollection AddNetworkingClients(this IServiceCollection services)
    {
        services.AddSingleton<IAuthClient, AuthClient>(); 
        services.AddSingleton<IAccountClient, AccountClient>();
        services.AddSingleton<IRoomClient, RoomClient>();
        
        //TODO: This is a simulated gateway only for testing. Must remove
        services.AddSingleton<IMatchGateway, SimulatedMatchGateway>();
        
        return services;
    }

    private static IServiceCollection AddDialogSystem(this IServiceCollection services)
    {
        DialogRegistry dialogRegistry = new();
        
        dialogRegistry.Register<SettingsViewModel, SettingsView>();
        dialogRegistry.Register<LogOutViewModel, LogOutView>();
        dialogRegistry.Register<ProfileEditorViewModel, ProfileEditorView>();
        dialogRegistry.Register<NotificationsViewModel, NotificationsView>();
        dialogRegistry.Register<PlayerActionConfirmationViewModel, PlayerActionConfirmationView>();
        dialogRegistry.Register<GuestNoticeViewModel, GuestNoticeView>();
        dialogRegistry.Register<MatchNoticeViewModel, MatchNoticeView>();
        dialogRegistry.Register<CardHelpViewModel, CardHelpView>();
        dialogRegistry.Register<AreYouThereViewModel, AreYouThereView>();
        dialogRegistry.Register<MatchMenuViewModel, MatchMenuView>();
        dialogRegistry.Register<RoundSummaryViewModel, RoundSummaryView>();
        dialogRegistry.Register<MatchResultViewModel, MatchResultView>();
        dialogRegistry.Register<ObjectiveSelectionViewModel, ObjectiveSelectionView>();
        dialogRegistry.Register<InviteFriendsViewModel, InviteFriendsView>();
        dialogRegistry.Register<GuestExitViewModel, GuestExitView>();
        dialogRegistry.Register<HamburgerMenuViewModel, HamburgerMenuView>();

        services.AddSingleton(dialogRegistry);
        services.AddSingleton<IDialogManager, DialogManager>();
        
        return services;
    }

    private static IServiceCollection AddViews(this IServiceCollection services)
    {
        services.AddTransient<SettingsView>();
        services.AddTransient<LogOutView>();
        services.AddTransient<ProfileEditorView>();
        services.AddTransient<NotificationsView>();
        services.AddTransient<PlayerActionConfirmationView>();
        services.AddTransient<GuestNoticeView>();
        services.AddTransient<MatchNoticeView>();
        services.AddTransient<CardHelpView>();
        services.AddTransient<AreYouThereView>();
        services.AddTransient<MatchMenuView>();
        services.AddTransient<RoundSummaryView>();
        services.AddTransient<MatchResultView>();
        services.AddTransient<ObjectiveSelectionView>();
        services.AddTransient<InviteFriendsView>();
        services.AddTransient<GuestExitView>();
        services.AddTransient<HamburgerMenuView>();
        
        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<TopBarViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<AccessMenuViewModel>();
        services.AddTransient<RegisterAccountViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LogOutViewModel>();
        services.AddTransient<ProfileEditorViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<PlayerActionConfirmationViewModel>();
        services.AddTransient<GuestNoticeViewModel>();
        services.AddTransient<GuestExitViewModel>();
        services.AddTransient<HamburgerMenuViewModel>();
        services.AddTransient<GameMenuViewModel>();
        services.AddTransient<PublicRoomListViewModel>();
        services.AddTransient<GameSetupViewModel>();
        services.AddTransient<WaitingRoomViewModel>();
        services.AddTransient<MatchViewModel>();
        services.AddTransient<MatchNoticeViewModel>();
        services.AddTransient<CardHelpViewModel>();
        services.AddTransient<AreYouThereViewModel>();
        services.AddTransient<MatchMenuViewModel>();
        services.AddTransient<RoundSummaryViewModel>();
        services.AddTransient<MatchResultViewModel>();
        services.AddTransient<ObjectiveSelectionViewModel>();
        services.AddTransient<InviteFriendsViewModel>();
        services.AddTransient<FriendsViewModel>();
        services.AddTransient<ScoreboardViewModel>();
        services.AddTransient<FriendListViewModel>();
        services.AddTransient<PlayerSearchViewModel>();
        services.AddTransient<BlockedPlayersViewModel>();
        services.AddTransient<AudioSettingsViewModel>();
        services.AddTransient<DisplaySettingsViewModel>();
        services.AddTransient<LanguageSettingsViewModel>();
        services.AddTransient<AccountSettingsViewModel>();
        
        return services;
    }
}