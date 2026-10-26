namespace Pesedjet.Client.UI.Utilities.Localization;

public interface ISettingsProtector
{
    byte[] Protect(byte[] plainData);
    bool TryUnprotect(byte[] protectedData, out byte[] plainData);
}
