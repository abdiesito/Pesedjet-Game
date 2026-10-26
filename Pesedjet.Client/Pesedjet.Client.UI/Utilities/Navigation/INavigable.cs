namespace Pesedjet.Client.UI.Utilities.Navigation;

public interface INavigable
{
    bool ShowsTopBar => false;
    
    bool ShowsGuestIndicator => ShowsTopBar;

    void OnNavigatedTo()
    {
    }

    void OnNavigatedFrom()
    {
    }
}
