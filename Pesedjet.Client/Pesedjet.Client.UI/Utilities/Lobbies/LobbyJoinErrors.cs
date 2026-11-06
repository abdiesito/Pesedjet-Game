using System.Globalization;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Client.UI.Models.Lobbies;

namespace Pesedjet.Client.UI.Utilities.Lobbies;


public static class LobbyJoinErrors
{
    private const string NotFoundKey = "GameMenu.CodeNotFound";
    private const string FullKey = "GameMenu.CodeFull";
    private const string FullWithoutCountKey = "GameMenu.CodeFullWithoutCount";
    private const string StartedKey = "GameMenu.CodeStarted";

    public static LobbyJoinResult FromMessageKey(string messageKey)
    {
        return FromMessageKey(messageKey, 0, 0);
    }

    // The lobby list knows how many players a lobby has; the server fault does not carry it.
    public static LobbyJoinResult FromMessageKey(string messageKey, int occupancy, int capacity)
    {
        LobbyCodeError error = messageKey switch
        {
            ServiceMessageKeys.LobbyNotFound => LobbyCodeError.NotFound,
            ServiceMessageKeys.LobbyFull => LobbyCodeError.Full,
            ServiceMessageKeys.LobbyAlreadyStarted => LobbyCodeError.Started,
            _ => LobbyCodeError.NotFound
        };

        return new LobbyJoinResult(error, occupancy, capacity);
    }

    public static string Describe(LobbyJoinResult? result)
    {
        string text = string.Empty;

        if (result?.Error == LobbyCodeError.NotFound)
        {
            text = LocalizationManager.Instance[NotFoundKey];
        }
        else if (result?.Error == LobbyCodeError.Full && result.Capacity == 0)
        {
            text = LocalizationManager.Instance[FullWithoutCountKey];
        }
        else if (result?.Error == LobbyCodeError.Full)
        {
            text = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[FullKey],
                result.Occupancy,
                result.Capacity);
        }
        else if (result?.Error == LobbyCodeError.Started)
        {
            text = LocalizationManager.Instance[StartedKey];
        }

        return text;
    }
}
