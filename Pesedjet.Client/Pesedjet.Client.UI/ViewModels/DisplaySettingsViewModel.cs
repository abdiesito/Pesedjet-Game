using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.ViewModels;

public partial class DisplaySettingsViewModel : ViewModelBase
{
    private const string DefaultWindowSize = "1366 × 768";

    private static readonly string[] AvailableWindowSizes =
    [
        "1280 × 720",
        DefaultWindowSize,
        "1600 × 900",
        "1920 × 1080"
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWindowed), nameof(IsFullScreen))]
    private DisplayMode _displayMode = DisplayMode.Windowed;

    [ObservableProperty]
    private string _selectedWindowSize = DefaultWindowSize;

    [ObservableProperty]
    private bool _reduceMotion;

    public IReadOnlyList<string> WindowSizes => AvailableWindowSizes;

    public bool IsWindowed => DisplayMode == DisplayMode.Windowed;

    public bool IsFullScreen => DisplayMode == DisplayMode.FullScreen;

    [RelayCommand]
    private void SelectWindowed()
    {
        DisplayMode = DisplayMode.Windowed;
    }

    [RelayCommand]
    private void SelectFullScreen()
    {
        DisplayMode = DisplayMode.FullScreen;
    }

    /*
     TODO: Apply the mode and size to the main window with the "keep this size?" confirmation, and save them on
     this device (CU-005). Offer only the sizes that fit the monitor.
    */
}
