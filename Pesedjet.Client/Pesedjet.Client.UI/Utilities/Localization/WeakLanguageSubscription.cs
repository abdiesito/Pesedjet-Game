using System;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Localization;

internal sealed class WeakLanguageSubscription
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
