using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(INavigator navigator)
    {
        Navigator = navigator;
        Navigator.NavigateAndClear<AccessMenuViewModel>();
    }
    
    public INavigator Navigator { get; }
}
