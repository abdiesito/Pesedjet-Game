using System.Globalization;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed record LanguageOption(string CultureCode, string DisplayName)
{
    private const int FirstCharacterIndex = 0;
    private const int SecondCharacterIndex = 1;

    public static LanguageOption FromCultureCode(string cultureCode)
    {
        var culture = CultureInfo.GetCultureInfo(cultureCode);
        var nativeName = culture.NativeName;
        var capitalizedNativeName =
            char.ToUpper(nativeName[FirstCharacterIndex], culture) + nativeName[SecondCharacterIndex..];
        return new LanguageOption(cultureCode, capitalizedNativeName);
    }
}
