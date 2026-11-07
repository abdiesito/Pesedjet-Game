using System;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Social;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels.Dependencies;

public sealed record FriendListDependencies(
    ISocialCircle SocialCircle,
    IDialogManager DialogManager,
    IToastManager ToastManager,
    TimeProvider TimeProvider);
