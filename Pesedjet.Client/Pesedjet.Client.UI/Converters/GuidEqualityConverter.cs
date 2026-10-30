#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Pesedjet.Client.UI.Converters;

public sealed class GuidEqualityConverter : IMultiValueConverter
{
    public static readonly GuidEqualityConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool areEqual = values.Count == 2
                        && values[0] is Guid first
                        && values[1] is Guid second
                        && first == second;

        return areEqual;
    }
}