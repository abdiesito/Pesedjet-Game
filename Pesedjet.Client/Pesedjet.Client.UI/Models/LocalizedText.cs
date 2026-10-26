using System.Globalization;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.Models;

public sealed record LocalizedText(string Key, params object[] Arguments)
{
    public string Resolve()
    {
        string template = LocalizationManager.Instance[Key];
        object[] values = new object[Arguments.Length];

        for (int index = 0; index < Arguments.Length; index++)
        {
            values[index] = Arguments[index] is LocalizedText nested ? nested.Resolve() : Arguments[index];
        }

        return values.Length == 0 ? template : string.Format(CultureInfo.CurrentCulture, template, values);
    }
}
