using System.Collections.ObjectModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Notifications;

public interface INotificationInbox
{
    ObservableCollection<NotificationItem> Items { get; }
}
