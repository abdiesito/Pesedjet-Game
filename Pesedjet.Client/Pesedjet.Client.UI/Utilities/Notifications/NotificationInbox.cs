using System.Collections.ObjectModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Notifications;

// TODO: Fill this inbox from the server (pending notifications at login and live ones through callbacks, CU-010).
public sealed class NotificationInbox : INotificationInbox
{
    public ObservableCollection<NotificationItem> Items { get; } = new();
}
