using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace Pesedjet.Client.UI.Utilities.Clipboard;

public sealed class ClipboardManager : IClipboardManager
{
    public async Task CopyTextAsync(string text)
    {
        IClipboard? clipboard = null;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            clipboard = desktop.MainWindow?.Clipboard;
        }

        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
