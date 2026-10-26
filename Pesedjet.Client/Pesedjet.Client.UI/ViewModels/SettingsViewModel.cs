using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly AudioSettingsViewModel _audioSettings;
    private readonly DisplaySettingsViewModel _displaySettings;
    private readonly LanguageSettingsViewModel _languageSettings;
    private readonly AccountSettingsViewModel _accountSettings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(CurrentSectionViewModel),
        nameof(IsAudioSelected),
        nameof(IsDisplaySelected),
        nameof(IsLanguageSelected),
        nameof(IsAccountSelected))]
    private SettingsSection _selectedSection = SettingsSection.Audio;

    public bool IsAccountAvailable { get; }

    public bool IsAudioSelected => SelectedSection == SettingsSection.Audio;

    public bool IsDisplaySelected => SelectedSection == SettingsSection.Display;

    public bool IsLanguageSelected => SelectedSection == SettingsSection.Language;

    public bool IsAccountSelected => SelectedSection == SettingsSection.Account;

    public ViewModelBase CurrentSectionViewModel => SelectedSection switch
    {
        SettingsSection.Display => _displaySettings,
        SettingsSection.Language => _languageSettings,
        SettingsSection.Account => _accountSettings,
        _ => _audioSettings
    };

    public SettingsViewModel(
        AudioSettingsViewModel audioSettings,
        DisplaySettingsViewModel displaySettings,
        LanguageSettingsViewModel languageSettings,
        AccountSettingsViewModel accountSettings,
        IPlayerSession playerSession)
    {
        _audioSettings = audioSettings;
        _displaySettings = displaySettings;
        _languageSettings = languageSettings;
        _accountSettings = accountSettings;
        
        IsAccountAvailable = playerSession.CurrentPlayer is not null;
    }

    [RelayCommand]
    private void ShowAudio()
    {
        SelectedSection = SettingsSection.Audio;
    }

    [RelayCommand]
    private void ShowDisplay()
    {
        SelectedSection = SettingsSection.Display;
    }

    [RelayCommand]
    private void ShowLanguage()
    {
        SelectedSection = SettingsSection.Language;
    }

    [RelayCommand]
    private void ShowAccount()
    {
        SelectedSection = SettingsSection.Account;
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }
}
