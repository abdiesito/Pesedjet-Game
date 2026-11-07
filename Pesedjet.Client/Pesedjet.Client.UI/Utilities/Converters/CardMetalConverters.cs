using Avalonia.Data.Converters;

namespace Pesedjet.Client.UI.Utilities.Converters;

public static class CardMetalConverters
{
    public static IValueConverter Brush { get; } = new MetalBrushConverter();

    public static IValueConverter Name { get; } = new MetalNameConverter();
}
