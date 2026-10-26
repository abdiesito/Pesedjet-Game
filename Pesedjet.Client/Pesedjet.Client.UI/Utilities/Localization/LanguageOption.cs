using System.Globalization;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed record LanguageOption(string CultureCode, string DisplayName)
{
    private const int FirstCharacterIndex = 0;
    private const int SecondCharacterIndex = 1;

    public static LanguageOption FromCultureCode(string cultureCode)
    {
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
        string nativeName = culture.NativeName;
        string capitalizedNativeName =
            char.ToUpper(nativeName[FirstCharacterIndex], culture) + nativeName[SecondCharacterIndex..];

        return new LanguageOption(cultureCode, capitalizedNativeName);
    }
}
