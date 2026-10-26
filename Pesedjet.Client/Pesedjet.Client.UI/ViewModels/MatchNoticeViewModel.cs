using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MatchNoticeViewModel : DialogViewModelBase, IDialogParameter<MatchNotice>
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCard))]
    private DeityCard? _card;

    public bool HasCard => Card is not null;

    private MatchNotice? _notice;

    public void Initialize(MatchNotice parameter)
    {
        _notice = parameter;
        FillTexts();
        Card = parameter.CardValue is null ? null : DeityCatalog.Find(parameter.CardValue.Value);
    }

    protected override void OnLanguageChanged()
    {
        FillTexts();
    }

    private void FillTexts()
    {
        if (_notice is not null)
        {
            Title = _notice.Title.Resolve();
            Message = _notice.Message.Resolve();
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }
}
