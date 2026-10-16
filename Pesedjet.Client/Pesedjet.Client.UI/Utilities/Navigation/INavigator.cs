using System.ComponentModel;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Navigation;

public interface INavigator : INotifyPropertyChanged
{
    ViewModelBase? CurrentViewModel { get; }
    bool CanGoBack { get; }
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateAndClear<TViewModel>() where TViewModel : ViewModelBase;
    void GoBack();
}