using System.Threading.Tasks;

namespace Pesedjet.Client.UI.Utilities.Clipboard;

public interface IClipboardManager
{
    Task CopyTextAsync(string text);
}
