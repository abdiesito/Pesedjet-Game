using System;
using System.IO;
using System.Text.Json;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed class UserSettingsStore
{
    private const int MaxSettingsFileSizeInBytes = 4096;
    private const string SettingsFolderName = "Pesedjet";
    private const string SettingsFileName = "user.settings";
    private const string TemporaryFileExtension = ".tmp";

    private readonly ISettingsProtector _protector;
    private readonly string _settingsFilePath;

    public UserSettingsStore(ISettingsProtector protector, string? settingsFilePath = null)
    {
        _protector = protector;
        _settingsFilePath = settingsFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            SettingsFolderName,
            SettingsFileName);
    }

    public UserSettings? Load()
    {
        UserSettings? settings = null;

        try
        {
            var fileInfo = new FileInfo(_settingsFilePath);

            if (fileInfo.Exists && fileInfo.Length > 0 && fileInfo.Length <= MaxSettingsFileSizeInBytes)
            {
                byte[] protectedData = File.ReadAllBytes(_settingsFilePath);

                if (_protector.TryUnprotect(protectedData, out byte[] plainData))
                {
                    settings = JsonSerializer.Deserialize<UserSettings>(plainData);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            settings = null;
        }

        return settings;
    }

    public void Save(UserSettings settings)
    {
        string settingsDirectory = Path.GetDirectoryName(_settingsFilePath) ?? string.Empty;
        Directory.CreateDirectory(settingsDirectory);

        byte[] protectedData = _protector.Protect(JsonSerializer.SerializeToUtf8Bytes(settings));

        string temporaryFilePath = _settingsFilePath + TemporaryFileExtension;
        File.WriteAllBytes(temporaryFilePath, protectedData);
        File.Move(temporaryFilePath, _settingsFilePath, overwrite: true);
    }
}
