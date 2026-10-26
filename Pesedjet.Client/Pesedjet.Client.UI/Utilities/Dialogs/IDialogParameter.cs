namespace Pesedjet.Client.UI.Utilities.Dialogs;

public interface IDialogParameter<in TParameter>
{
    void Initialize(TParameter parameter);
}
