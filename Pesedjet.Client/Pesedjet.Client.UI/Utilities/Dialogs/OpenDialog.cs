using System.Threading.Tasks;
using Avalonia.Controls;
using Pesedjet.Client.UI.ViewModels;
using Pesedjet.Client.UI.ViewModels.Dialogs;

namespace Pesedjet.Client.UI.Utilities.Dialogs;

public sealed record OpenDialog(DialogViewModelBase ViewModel, Control View, TaskCompletionSource Completion);
