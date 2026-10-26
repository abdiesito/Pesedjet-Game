using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.ViewModels;

public partial class AudioSettingsViewModel : ViewModelBase
{
    private const double DefaultMasterVolume = 80;
    private const double DefaultMusicVolume = 60;
    private const double DefaultEffectsVolume = 70;

    [ObservableProperty]
    private double _masterVolume = DefaultMasterVolume;

    [ObservableProperty]
    private double _musicVolume = DefaultMusicVolume;

    [ObservableProperty]
    private double _effectsVolume = DefaultEffectsVolume;

    [ObservableProperty]
    private bool _isMuted;

    // TODO: Apply the volumes to the audio output and save them on this device (CU-005).
}
