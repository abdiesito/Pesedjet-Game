using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Notifications;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record TopBarDependencies(
    IPlayerSession PlayerSession,
    IDialogManager DialogManager,
    INotificationInbox NotificationInbox,
    ISocialSynchronizer SocialSynchronizer);
