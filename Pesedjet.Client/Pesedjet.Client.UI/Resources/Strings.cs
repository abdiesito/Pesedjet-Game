using System.Resources;

namespace Pesedjet.Client.UI.Resources;
internal static class Strings
{
    public static ResourceManager ResourceManager { get; } =
        new("Pesedjet.Client.UI.Resources.Strings", typeof(Strings).Assembly);
}
