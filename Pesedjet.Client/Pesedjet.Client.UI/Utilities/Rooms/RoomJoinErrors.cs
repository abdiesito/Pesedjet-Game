using System.Globalization;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.Utilities.Rooms;


public static class RoomJoinErrors
{
    private const string NotFoundKey = "GameMenu.CodeNotFound";
    private const string FullKey = "GameMenu.CodeFull";
    private const string StartedKey = "GameMenu.CodeStarted";

    public static RoomJoinResult FromMessageKey(string messageKey)
    {
        RoomCodeError error = messageKey switch
        {
            ServiceMessageKeys.RoomNotFound => RoomCodeError.NotFound,
            ServiceMessageKeys.RoomFull => RoomCodeError.Full,
            ServiceMessageKeys.RoomAlreadyStarted => RoomCodeError.Started,
            _ => RoomCodeError.NotFound
        };

        return new RoomJoinResult(error, 0, 0, null);
    }

    public static string Describe(RoomJoinResult? result)
    {
        string text = string.Empty;

        if (result?.Error == RoomCodeError.NotFound)
        {
            text = LocalizationManager.Instance[NotFoundKey];
        }
        else if (result?.Error == RoomCodeError.Full)
        {
            text = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[FullKey],
                result.Occupancy,
                result.Capacity);
        }
        else if (result?.Error == RoomCodeError.Started)
        {
            text = LocalizationManager.Instance[StartedKey];
        }

        return text;
    }
}
