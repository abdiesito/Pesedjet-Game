using System.Globalization;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.Utilities.Rooms;


public static class RoomJoinErrors
{
    private const string NotFoundKey = "GameMenu.CodeNotFound";
    private const string FullKey = "GameMenu.CodeFull";
    private const string FullWithoutCountKey = "GameMenu.CodeFullWithoutCount";
    private const string StartedKey = "GameMenu.CodeStarted";

    public static RoomJoinResult FromMessageKey(string messageKey)
    {
        return FromMessageKey(messageKey, 0, 0);
    }

    // The room list knows how many players a room has; the server fault does not carry it.
    public static RoomJoinResult FromMessageKey(string messageKey, int occupancy, int capacity)
    {
        RoomCodeError error = messageKey switch
        {
            ServiceMessageKeys.RoomNotFound => RoomCodeError.NotFound,
            ServiceMessageKeys.RoomFull => RoomCodeError.Full,
            ServiceMessageKeys.RoomAlreadyStarted => RoomCodeError.Started,
            _ => RoomCodeError.NotFound
        };

        return new RoomJoinResult(error, occupancy, capacity);
    }

    public static string Describe(RoomJoinResult? result)
    {
        string text = string.Empty;

        if (result?.Error == RoomCodeError.NotFound)
        {
            text = LocalizationManager.Instance[NotFoundKey];
        }
        else if (result?.Error == RoomCodeError.Full && result.Capacity == 0)
        {
            text = LocalizationManager.Instance[FullWithoutCountKey];
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
