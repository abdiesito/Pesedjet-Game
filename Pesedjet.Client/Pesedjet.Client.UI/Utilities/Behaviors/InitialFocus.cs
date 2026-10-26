using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Pesedjet.Client.UI.Utilities.Behaviors;

public static class InitialFocus
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(InitialFocus));

    static InitialFocus()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(HandleIsEnabledChanged);
    }

    public static bool GetIsEnabled(Control control)
    {
        return control.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(Control control, bool value)
    {
        control.SetValue(IsEnabledProperty, value);
    }

    private static void HandleIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.NewValue is true)
        {
            control.Loaded += HandleLoaded;
        }
        else
        {
            control.Loaded -= HandleLoaded;
        }
    }

    private static void HandleLoaded(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is Control control)
        {
            control.Focus(NavigationMethod.Tab);
        }
    }
}
