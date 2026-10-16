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
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        Dispatcher.UIThread.Post(() =>
        {
            if (CurrentViewModel is INavigable navigable)
            {
                navigable.OnNavigatedFrom();
            }

            if (CurrentViewModel != null)
            {
                _navigationStack.Push(CurrentViewModel);
            }

            SetCurrentViewModel(viewModel);
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    public void GoBack()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_navigationStack.Count == 0) return; 

            if (CurrentViewModel is INavigable currentNavigable)
            {
                currentNavigable.OnNavigatedFrom();
            }
            
            if (CurrentViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            var previousViewModel = _navigationStack.Pop();
            SetCurrentViewModel(previousViewModel);
            OnPropertyChanged(nameof(CanGoBack));
        });
    }
    
    public void NavigateAndClear<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        Dispatcher.UIThread.Post(() =>
        {
            if (CurrentViewModel is INavigable currentNavigable)
            {
                currentNavigable.OnNavigatedFrom();
            }
            if (CurrentViewModel is IDisposable currentDisposable)
            {
                currentDisposable.Dispose();
            }
            
            while (_navigationStack.Count > 0)
            {
                var vm = _navigationStack.Pop();
                if (vm is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            SetCurrentViewModel(viewModel);
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    private void SetCurrentViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        
        if (CurrentViewModel is INavigable navigable)
        {
            navigable.OnNavigatedTo();
        }
    }
}