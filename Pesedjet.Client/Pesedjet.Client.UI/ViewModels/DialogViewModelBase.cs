using System;

namespace Pesedjet.Client.UI.ViewModels;

public abstract class DialogViewModelBase : ViewModelBase
{
    public event EventHandler? CloseRequested;

    public bool IsConfirmed { get; protected set; }

    public virtual void Dismiss()
    {
        RequestClose();
    }

    protected void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
