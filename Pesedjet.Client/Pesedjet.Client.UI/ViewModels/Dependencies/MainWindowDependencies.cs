using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record MainWindowDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IToastManager ToastManager,
    TopBarViewModel TopBar);
