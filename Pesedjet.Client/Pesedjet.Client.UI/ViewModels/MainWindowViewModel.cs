using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Toasts;
using Pesedjet.Client.UI.ViewModels.Dependencies;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public INavigator Navigator { get; }

    public IDialogManager DialogManager { get; }

    public IToastManager ToastManager { get; }

    public TopBarViewModel TopBar { get; }

    public bool IsTopBarVisible => Navigator.CurrentViewModel is INavigable { ShowsTopBar: true };
    
    public bool IsGuestIndicatorVisible =>
        Navigator.CurrentViewModel is INavigable { ShowsGuestIndicator: true } && TopBar.IsGuest;

    public MainWindowViewModel(MainWindowDependencies dependencies)
    {
        Navigator = dependencies.Navigator;
        DialogManager = dependencies.DialogManager;
        ToastManager = dependencies.ToastManager;
        TopBar = dependencies.TopBar;
        Navigator.PropertyChanged += HandleNavigatorPropertyChanged;
        TopBar.PropertyChanged += HandleTopBarPropertyChanged;
        Navigator.NavigateAndClear<AccessMenuViewModel>();
    }

    [RelayCommand]
    private void CloseCurrentDialog()
    {
        DialogManager.CloseCurrentDialog();
    }

    private void HandleTopBarPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(TopBarViewModel.IsGuest))
        {
            OnPropertyChanged(nameof(IsGuestIndicatorVisible));
        }
    }

    private void HandleNavigatorPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(INavigator.CurrentViewModel))
        {
            OnPropertyChanged(nameof(IsTopBarVisible));
            OnPropertyChanged(nameof(IsGuestIndicatorVisible));
            LocalizationManager.Instance.ReleaseDetachedBindings();
        }
    }
}
