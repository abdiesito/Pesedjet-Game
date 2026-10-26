using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Social;

namespace Pesedjet.Client.UI.ViewModels;

public partial class InviteFriendsViewModel : DialogViewModelBase, IDialogParameter<int>
{
    private const string FreeSlotsOneKey = "InviteFriends.FreeSlotOne";
    private const string FreeSlotsManyKey = "InviteFriends.FreeSlotsMany";
    private const string SendOneKey = "InviteFriends.SendOne";
    private const string SendManyKey = "InviteFriends.SendMany";
    private const string SendNoneKey = "InviteFriends.Send";
    private const string UnavailableKey = "InviteFriends.InMatchUnavailable";

    private readonly ISocialCircle _socialCircle;
    private readonly TimeProvider _timeProvider;

    private int _freeSlots;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SendLabel))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private int _selectedCount;

    public IReadOnlyList<InviteCandidate> Candidates { get; private set; } = [];

    public bool HasCandidates => Candidates.Count > 0;

    public string FreeSlotsText => _freeSlots == 1
        ? LocalizationManager.Instance[FreeSlotsOneKey]
        : string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[FreeSlotsManyKey], _freeSlots);

    public string SendLabel => SelectedCount switch
    {
        0 => LocalizationManager.Instance[SendNoneKey],
        1 => LocalizationManager.Instance[SendOneKey],
        _ => string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[SendManyKey], SelectedCount)
    };

    public InviteFriendsViewModel(ISocialCircle socialCircle, TimeProvider timeProvider)
    {
        _socialCircle = socialCircle;
        _timeProvider = timeProvider;
    }

    public void Initialize(int freeSlots)
    {
        _freeSlots = freeSlots;

        List<InviteCandidate> candidates = new();

        foreach (FriendItem friend in _socialCircle.Friends)
        {
            if (!friend.IsOffline)
            {
                candidates.Add(new InviteCandidate
                {
                    Username = friend.Username,
                    IsAvailable = friend.IsOnline,
                    FriendPresence = friend.Presence,
                    FriendLastSeenAt = friend.LastSeenAt
                });
            }
        }

        foreach (InviteCandidate candidate in candidates)
        {
            candidate.PropertyChanged += HandleCandidateChanged;
        }

        Candidates = candidates;
        FillStatusTexts();
    }

    protected override void OnLanguageChanged()
    {
        FillStatusTexts();
        base.OnLanguageChanged();
    }

    private void FillStatusTexts()
    {
        foreach (InviteCandidate candidate in Candidates)
        {
            candidate.StatusText = candidate.IsAvailable
                ? PresenceFormatter.Format(
                    candidate.FriendPresence,
                    candidate.FriendLastSeenAt,
                    _timeProvider.GetUtcNow())
                : LocalizationManager.Instance[UnavailableKey];
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private void Send()
    {
        // TODO: Send the room invitations to the selected friends through the server (CU-014 step 5).
        IsConfirmed = true;
        RequestClose();
    }

    private bool CanSend()
    {
        return SelectedCount > 0 && SelectedCount <= _freeSlots;
    }

    private void HandleCandidateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        int selected = 0;

        foreach (InviteCandidate candidate in Candidates)
        {
            if (candidate.IsSelected)
            {
                selected++;
            }
        }

        SelectedCount = selected;
    }
}
