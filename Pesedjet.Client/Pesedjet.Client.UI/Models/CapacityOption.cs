using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class CapacityOption : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public ICommand? SelectCommand { get; set; }

    [ObservableProperty]
    private bool _isAvailable = true;

    public required int Value { get; init; }
}
