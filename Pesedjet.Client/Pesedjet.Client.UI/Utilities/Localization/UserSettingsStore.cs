using System;
using System.IO;
using System.Text.Json;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed record UserSettings(string Language);

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
        try
        {
            var fileInfo = new FileInfo(_settingsFilePath);
            if (!fileInfo.Exists || fileInfo.Length is 0 or > MaxSettingsFileSizeInBytes)
            {
                return null;
            }

            var protectedData = File.ReadAllBytes(_settingsFilePath);
            return _protector.TryUnprotect(protectedData, out var plainData)
                ? JsonSerializer.Deserialize<UserSettings>(plainData)
                : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    public void Save(UserSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);

        var protectedData = _protector.Protect(JsonSerializer.SerializeToUtf8Bytes(settings));
        
        var temporaryFilePath = _settingsFilePath + TemporaryFileExtension;
        File.WriteAllBytes(temporaryFilePath, protectedData);
        File.Move(temporaryFilePath, _settingsFilePath, overwrite: true);
    }
}
