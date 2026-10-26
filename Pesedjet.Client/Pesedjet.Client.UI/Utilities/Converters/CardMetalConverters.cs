using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.Utilities.Converters;

public static class CardMetalConverters
{
    public static IValueConverter Brush { get; } = new MetalBrushConverter();

    public static IValueConverter Name { get; } = new MetalNameConverter();

    private sealed class MetalBrushConverter : IValueConverter
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

    private sealed class MetalNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is CardMetal metal ? LocalizationManager.Instance["Card.Metal." + metal] : string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
