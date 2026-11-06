using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public class ViewModelBase : ObservableObject
{
    private const string AllProperties = "";

    protected ViewModelBase()
    {
        WeakLanguageSubscription.Attach(this);
    }
    
    protected virtual void OnLanguageChanged()
    {
        OnPropertyChanged(AllProperties);
    }

    internal void RaiseLanguageChanged()
    {
        OnLanguageChanged();
    }
}
