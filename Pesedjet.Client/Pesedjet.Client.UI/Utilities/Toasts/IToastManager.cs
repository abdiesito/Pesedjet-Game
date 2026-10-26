using System.ComponentModel;

namespace Pesedjet.Client.UI.Utilities.Toasts;

public interface IToastManager : INotifyPropertyChanged
{
    string Message { get; }
    bool IsVisible { get; }
    void Show(string message);
}
