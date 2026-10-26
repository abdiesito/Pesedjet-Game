using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.ViewModels;

public partial class CardHelpViewModel : DialogViewModelBase, IDialogParameter<CardHelpRequest>
{
    private const string CountsKey = "CardHelp.Counts";

    [ObservableProperty]
    private DeityCard _card = DeityCatalog.Find(DeityCatalog.NeftisValue);

    [ObservableProperty]
    private string _effect = string.Empty;

    [ObservableProperty]
    private string _counts = string.Empty;

    private CardHelpRequest? _request;

    public void Initialize(CardHelpRequest parameter)
    {
        _request = parameter;
        Card = DeityCatalog.Find(parameter.CardValue);
        Effect = LocalizationManager.Instance["Card.Effect." + parameter.CardValue];
        Counts = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[CountsKey],
            Card.Copies,
            parameter.Remaining);
    }

    protected override void OnLanguageChanged()
    {
        if (_request is not null)
        {
            Initialize(_request);
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }
}
