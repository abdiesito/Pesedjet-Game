using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Dialogs;

public sealed class DialogRegistry
{
    private readonly Dictionary<Type, Type> _viewTypesByViewModelType = new();

    public void Register<TViewModel, TView>()
        where TViewModel : DialogViewModelBase
        where TView : Control
    {
        _viewTypesByViewModelType[typeof(TViewModel)] = typeof(TView);
    }

    public Type GetViewType(Type viewModelType)
    {
        bool isRegistered = _viewTypesByViewModelType.TryGetValue(viewModelType, out Type? viewType);

        if (!isRegistered || viewType is null)
        {
            throw new InvalidOperationException($"No dialog view is registered for {viewModelType.Name}.");
        }

        return viewType;
    }
}
