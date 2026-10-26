using System.Collections.Generic;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.ViewModels;

public class LanguageSettingsViewModel : ViewModelBase
{
    public IReadOnlyList<LanguageOption> AvailableLanguages => LocalizationManager.AvailableLanguages;

    public LanguageOption CurrentLanguage
    {
        get
        {
            return LocalizationManager.Instance.CurrentLanguage;
        }
        set
        {
            LocalizationManager.Instance.CurrentLanguage = value;
            OnPropertyChanged();
        }
    }

    // TODO: Also save the language in the player's account (RN15); today it is only saved on this device.
}
