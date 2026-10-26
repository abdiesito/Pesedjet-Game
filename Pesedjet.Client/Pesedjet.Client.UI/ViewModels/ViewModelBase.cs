using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;

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
    
    private sealed class WeakLanguageSubscription
    {
        private readonly WeakReference<ViewModelBase> _target;

        private WeakLanguageSubscription(ViewModelBase target)
        {
            _target = new WeakReference<ViewModelBase>(target);
        }

        public static void Attach(ViewModelBase target)
        {
            WeakLanguageSubscription subscription = new(target);

            LocalizationManager.Instance.LanguageChanged += subscription.Handle;
        }

        private void Handle(object? sender, EventArgs eventArgs)
        {
            if (_target.TryGetTarget(out ViewModelBase? target))
            {
                target.RaiseLanguageChanged();
            }
            else
            {
                LocalizationManager.Instance.LanguageChanged -= Handle;
            }
        }
    }
}
