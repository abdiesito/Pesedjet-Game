using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Pesedjet.Client.UI.Models.Cards;

namespace Pesedjet.Client.UI.Utilities.Converters;

public sealed class MetalBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        IBrush? brush = null;

        if (value is CardMetal metal && Application.Current is not null)
        {
            Application.Current.TryGetResource("ColorMetal" + metal, null, out object? resource);
            brush = resource as IBrush;
        }

        return brush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
