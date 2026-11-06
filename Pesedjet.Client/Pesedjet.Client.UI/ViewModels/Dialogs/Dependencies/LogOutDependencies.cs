using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;

public sealed record LogOutDependencies(
    INavigator Navigator,
    IPlayerSession PlayerSession,
    IAuthClient AuthClient,
    UserSettingsStore UserSettingsStore);
