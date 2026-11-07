using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Models.Cards;

namespace Pesedjet.Client.UI.Utilities.Converters;

public sealed class MetalNameConverter : IValueConverter
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
