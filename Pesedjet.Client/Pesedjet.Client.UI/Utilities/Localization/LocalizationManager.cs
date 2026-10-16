using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pesedjet.Client.UI.Resources;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private const string DefaultCultureCode = "es-MX";
    
    private const string AllPropertiesChanged = "";
    
    private static readonly string[] SupportedCultureCodes = [DefaultCultureCode, "en-US"];

    public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } =
        SupportedCultureCodes.Select(LanguageOption.FromCultureCode).ToList();

    public static LocalizationManager Instance { get; } =
        new(new UserSettingsStore(new AesGcmSettingsProtector()));

    private readonly UserSettingsStore _settingsStore;
    private readonly object _cultureChangeLock = new();
    private readonly object _settingsWriteLock = new();
    private volatile CultureInfo _activeCulture = null!;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    private LocalizationManager(UserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        var savedCultureCode = FindSupportedCultureCode(_settingsStore.Load()?.Language);
        ApplyCulture(savedCultureCode ?? DefaultCultureCode);
    }

    public string this[string key] =>
        Strings.ResourceManager.GetString(key, _activeCulture) ?? $"[{key}]";
    
    public LanguageOption CurrentLanguage =>
        AvailableLanguages.First(language => language.CultureCode == _activeCulture.Name);
    
    public bool ChangeLanguage(string cultureCode)
    {
        var supportedCultureCode = FindSupportedCultureCode(cultureCode);
        if (supportedCultureCode is null)
        {
            return false;
        }

        lock (_cultureChangeLock)
        {
            if (supportedCultureCode == _activeCulture.Name)
            {
                return true;
            }

            ApplyCulture(supportedCultureCode);
        }
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(AllPropertiesChanged));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
        
        Task.Run(SaveActiveLanguagePreference);
        return true;
    }

    private void SaveActiveLanguagePreference()
    {
        lock (_settingsWriteLock)
        {
            try
            {
                _settingsStore.Save(new UserSettings(_activeCulture.Name));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                //TODO: Manage Exception when the language applies to this session but could not be saved for the next one.
            }
        }
    }

    private void ApplyCulture(string cultureCode)
    {
        _activeCulture = CultureInfo.GetCultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = _activeCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _activeCulture;
        CultureInfo.CurrentCulture = _activeCulture;
        CultureInfo.CurrentUICulture = _activeCulture;
    }
    
    private static string? FindSupportedCultureCode(string? cultureCode) =>
        SupportedCultureCodes.FirstOrDefault(
            supportedCode => string.Equals(supportedCode, cultureCode, StringComparison.OrdinalIgnoreCase));
}
