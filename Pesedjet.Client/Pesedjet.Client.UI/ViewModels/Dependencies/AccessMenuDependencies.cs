using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record AccessMenuDependencies(
    INavigator Navigator,
    IDialogManager DialogManager,
    IAuthClient AuthClient,
    IPlayerSession PlayerSession,
    UserSettingsStore UserSettingsStore);
