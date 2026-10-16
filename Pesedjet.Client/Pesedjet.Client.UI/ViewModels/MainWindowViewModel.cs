using System.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(INavigator navigator)
    {
        Navigator = navigator;
        Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        Navigator.NavigateAndClear<AccessMenuViewModel>();
    }

    public INavigator Navigator { get; }
    
    private static void OnNavigatorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigator.CurrentViewModel))
        {
            LocalizationManager.Instance.ReleaseDetachedBindings();
        }
    }
}
