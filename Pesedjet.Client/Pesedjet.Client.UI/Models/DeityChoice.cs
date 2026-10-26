using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class DeityChoice : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public required DeityCard Card { get; init; }

    public required bool IsNamable { get; init; }

    public string Label => $"{Card.Value} {Card.Name}";

    public System.Windows.Input.ICommand? SelectCommand { get; set; }
}
