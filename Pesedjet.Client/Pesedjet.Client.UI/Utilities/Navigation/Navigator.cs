using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Navigation;

public partial class Navigator : ObservableObject, INavigator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _navigationStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private ViewModelBase? _currentViewModel;

    public bool CanGoBack => _navigationStack.Count > 0;

    public Navigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        Dispatcher.UIThread.Post(ShowViewModel<TViewModel>);
    }

    public void GoBack()
    {
        Dispatcher.UIThread.Post(ShowPreviousViewModel);
    }

    public void NavigateAndClear<TViewModel>() where TViewModel : ViewModelBase
    {
        Dispatcher.UIThread.Post(ShowOnlyViewModel<TViewModel>);
    }

    private void ShowViewModel<TViewModel>() where TViewModel : ViewModelBase
    {
        // A double click queues the same request twice. The second one finds its screen already shown and is
        // dropped before anything is built, so no duplicate screen is stacked and no ViewModel is created for nothing.
        if (!IsShowing<TViewModel>())
        {
            TViewModel viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            NotifyNavigatedFrom(CurrentViewModel);

            if (CurrentViewModel is not null)
            {
                _navigationStack.Push(CurrentViewModel);
            }

            SetCurrentViewModel(viewModel);
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    private void ShowPreviousViewModel()
    {
        if (_navigationStack.Count > 0)
        {
            NotifyNavigatedFrom(CurrentViewModel);
            DisposeViewModel(CurrentViewModel);

            ViewModelBase previousViewModel = _navigationStack.Pop();
            SetCurrentViewModel(previousViewModel);
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    private void ShowOnlyViewModel<TViewModel>() where TViewModel : ViewModelBase
    {
        // Already the only screen: rebuilding it would only dispose and recreate an identical one.
        bool isAlreadyOnlyScreen = IsShowing<TViewModel>() && _navigationStack.Count == 0;

        if (!isAlreadyOnlyScreen)
        {
            TViewModel viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            NotifyNavigatedFrom(CurrentViewModel);
            DisposeViewModel(CurrentViewModel);

            while (_navigationStack.Count > 0)
            {
                ViewModelBase stackedViewModel = _navigationStack.Pop();
                DisposeViewModel(stackedViewModel);
            }

            SetCurrentViewModel(viewModel);
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    private bool IsShowing<TViewModel>() where TViewModel : ViewModelBase
    {
        return CurrentViewModel?.GetType() == typeof(TViewModel);
    }

    private void SetCurrentViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;

        if (CurrentViewModel is INavigable navigable)
        {
            navigable.OnNavigatedTo();
        }
    }

    private static void NotifyNavigatedFrom(ViewModelBase? viewModel)
    {
        if (viewModel is INavigable navigable)
        {
            navigable.OnNavigatedFrom();
        }
    }

    private static void DisposeViewModel(ViewModelBase? viewModel)
    {
        if (viewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
