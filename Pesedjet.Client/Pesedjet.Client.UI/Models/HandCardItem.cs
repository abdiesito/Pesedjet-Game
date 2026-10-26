using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class HandCardItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public ICommand? SelectCommand { get; set; }

    public required DeityCard Card { get; init; }

    public required string KeyHint { get; init; }
}
