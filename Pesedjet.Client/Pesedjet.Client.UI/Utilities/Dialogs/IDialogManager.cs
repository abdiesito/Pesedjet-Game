using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Dialogs;

public interface IDialogManager : INotifyPropertyChanged
{
    Control? CurrentDialogView { get; }
    bool HasOpenDialog { get; }
    Task<TViewModel> ShowDialogAsync<TViewModel>() where TViewModel : DialogViewModelBase;
    Task<TViewModel> ShowDialogAsync<TViewModel, TParameter>(TParameter parameter)
        where TViewModel : DialogViewModelBase, IDialogParameter<TParameter>;
    void CloseCurrentDialog();
}
