using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Server.Contracts.Faults;

namespace Pesedjet.Client.UI.Utilities.Social;

public static class FriendshipErrors
{
    private const string ConnectionErrorKey = "Common.ConnectionError";
    private const string GeneralErrorKey = "Messages.MSG-GEN-001";

    public static string Describe<TValue>(ClientResult<TValue> result)
    {
        string key = GeneralErrorKey;

        if (result.IsConnectionFailure)
        {
            key = ConnectionErrorKey;
        }
        else if (result.Failure == FailureKind.Business && IsKnownMessageKey(result.MessageKey))
        {
            key = result.MessageKey;
        }

        return LocalizationManager.Instance[key];
    }

    private static bool IsKnownMessageKey(string messageKey)
    {
        return messageKey is ServiceMessageKeys.AlreadyFriends
            or ServiceMessageKeys.FriendshipBlocked
            or ServiceMessageKeys.FriendRequestNotFound
            or ServiceMessageKeys.LobbyInviteNotAllowed;
    }
}
