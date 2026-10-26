using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.ViewModels;

// MOD-005: choose the opponent (and, for Neftis, the deity to name) before playing a card.
public partial class ObjectiveSelectionViewModel : DialogViewModelBase, IDialogParameter<ObjectiveRequest>
{
    private const string ProtectedKey = "Objective.Protected";
    private const string OutKey = "Objective.OutOfRound";
    private const string ImmuneKey = "Objective.ImmuneToNeftis";
    private const string UnavailableKey = "Objective.Unavailable";
    private const string TitleKey = "Objective.PlayTitle";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title), nameof(CardEffect), nameof(NeedsDeity))]
    private DeityCard _card = DeityCatalog.Find(DeityCatalog.NeftisValue);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private ObjectiveOption? _selectedTarget;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private DeityChoice? _selectedDeity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoTargets), nameof(ShowsDeities))]
    private bool _hasEligibleTargets;

    public IReadOnlyList<ObjectiveOption> Targets { get; private set; } = [];

    public IReadOnlyList<DeityChoice> Deities { get; private set; } = [];

    public string Title => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[TitleKey],
        Card.Name,
        Card.Value);

    public string CardEffect => LocalizationManager.Instance["Card.Effect." + Card.Value];

    public bool NeedsDeity => Card.NeedsNamedDeity;

    public bool HasNoTargets => !HasEligibleTargets;

    public bool ShowsDeities => NeedsDeity && HasEligibleTargets;

    public string? TargetName => SelectedTarget?.Username;

    public int? NamedDeity => SelectedDeity?.Card.Value;

    public void Initialize(ObjectiveRequest request)
    {
        Card = DeityCatalog.Find(request.CardValue);

        List<ObjectiveOption> targets = new();

        foreach (PlayerSnapshot opponent in request.Opponents)
        {
            string reasonKey = BuildReasonKey(opponent);

            targets.Add(new ObjectiveOption
            {
                Username = opponent.Username,
                IsEligible = reasonKey.Length == 0,
                ReasonKey = reasonKey,
                SelectCommand = SelectTargetCommand
            });
        }

        List<DeityChoice> deities = new();

        foreach (DeityCard deity in DeityCatalog.All)
        {
            deities.Add(new DeityChoice
            {
                Card = deity,
                IsNamable = deity.Value != DeityCatalog.NeftisValue,
                SelectCommand = SelectDeityCommand
            });
        }

        Targets = targets;
        Deities = deities;
        HasEligibleTargets = ContainsEligible(targets);
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void SelectTarget(ObjectiveOption option)
    {
        if (option.IsEligible)
        {
            foreach (ObjectiveOption target in Targets)
            {
                target.IsSelected = target == option;
            }

            SelectedTarget = option;
        }
    }

    [RelayCommand]
    private void SelectDeity(DeityChoice choice)
    {
        if (choice.IsNamable)
        {
            foreach (DeityChoice deity in Deities)
            {
                deity.IsSelected = deity == choice;
            }

            SelectedDeity = choice;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }

    // With no eligible opponent the card is played without effect and the play is still legal (RJ-10).
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        IsConfirmed = true;
        RequestClose();
    }

    private bool CanConfirm()
    {
        bool hasTarget = HasNoTargets || SelectedTarget is not null;
        bool hasDeity = !NeedsDeity || HasNoTargets || SelectedDeity is not null;

        return hasTarget && hasDeity;
    }

    protected override void OnLanguageChanged()
    {
        foreach (ObjectiveOption target in Targets)
        {
            target.Refresh();
        }

        base.OnLanguageChanged();
    }

    private static bool ContainsEligible(List<ObjectiveOption> targets)
    {
        bool found = false;

        foreach (ObjectiveOption target in targets)
        {
            found = found || target.IsEligible;
        }

        return found;
    }

    private string BuildReasonKey(PlayerSnapshot opponent)
    {
        string reason = string.Empty;

        if (opponent.Status == PlayerStatus.Protected)
        {
            reason = ProtectedKey;
        }
        else if (opponent.Status == PlayerStatus.OutOfRound)
        {
            reason = OutKey;
        }
        else if (opponent.Status == PlayerStatus.ImmuneToNeftis && Card.Value == DeityCatalog.NeftisValue)
        {
            reason = ImmuneKey;
        }
        else if (opponent.Status is PlayerStatus.Expelled or PlayerStatus.Disconnected)
        {
            reason = UnavailableKey;
        }

        return reason;
    }
}
