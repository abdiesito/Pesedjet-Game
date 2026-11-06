using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record MainMenuDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IPlayerSession PlayerSession,
    IToastManager ToastManager);
