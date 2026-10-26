using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Dialogs;

public sealed partial class DialogManager : ObservableObject, IDialogManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DialogRegistry _dialogRegistry;
    private readonly List<OpenDialog> _openDialogs = new();

    [ObservableProperty]
    private Control? _currentDialogView;

    [ObservableProperty]
    private bool _hasOpenDialog;

    public DialogManager(IServiceProvider serviceProvider, DialogRegistry dialogRegistry)
    {
        _serviceProvider = serviceProvider;
        _dialogRegistry = dialogRegistry;
    }

    public async Task<TViewModel> ShowDialogAsync<TViewModel>() where TViewModel : DialogViewModelBase
    {
        TViewModel viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        await ShowAsync(viewModel);

        return viewModel;
    }

    public async Task<TViewModel> ShowDialogAsync<TViewModel, TParameter>(TParameter parameter)
        where TViewModel : DialogViewModelBase, IDialogParameter<TParameter>
    {
        TViewModel viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        viewModel.Initialize(parameter);
        await ShowAsync(viewModel);

        return viewModel;
    }

    public void CloseCurrentDialog()
    {
        if (_openDialogs.Count > 0)
        {
            _openDialogs[^1].ViewModel.Dismiss();
        }
    }

    private async Task ShowAsync(DialogViewModelBase viewModel)
    {
        Type viewType = _dialogRegistry.GetViewType(viewModel.GetType());
        Control view = (Control)_serviceProvider.GetRequiredService(viewType);
        TaskCompletionSource completion = new();

        view.DataContext = viewModel;
        viewModel.CloseRequested += HandleCloseRequested;
        _openDialogs.Add(new OpenDialog(viewModel, view, completion));
        ShowTopDialog();

        await completion.Task;
    }

    private void HandleCloseRequested(object? sender, EventArgs eventArgs)
    {
        OpenDialog? closingDialog = null;

        foreach (OpenDialog openDialog in _openDialogs)
        {
            if (openDialog.ViewModel == sender)
            {
                closingDialog = openDialog;
            }
        }

        if (closingDialog is not null)
        {
            CloseDialog(closingDialog);
        }
    }

    private void CloseDialog(OpenDialog dialog)
    {
        dialog.ViewModel.CloseRequested -= HandleCloseRequested;
        _openDialogs.Remove(dialog);
        ShowTopDialog();

        if (dialog.ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }

        dialog.Completion.SetResult();
    }

    private void ShowTopDialog()
    {
        bool hasDialogs = _openDialogs.Count > 0;

        CurrentDialogView = hasDialogs ? _openDialogs[^1].View : null;
        HasOpenDialog = hasDialogs;
    }

    private sealed record OpenDialog(DialogViewModelBase ViewModel, Control View, TaskCompletionSource Completion);
}
