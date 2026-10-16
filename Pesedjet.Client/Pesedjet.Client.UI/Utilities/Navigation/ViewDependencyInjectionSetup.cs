using System;
using Microsoft.Extensions.DependencyInjection;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI.Utilities.Navigation;

public static class ViewDependencyInjectionSetup
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INavigator, Navigator>();
        
        // services.AddSingleton<IDialogManager, DialogManager>(); // TODO: When having Dialogs Views, add here

        services.AddTransient<AccessMenuViewModel>();
        services.AddTransient<RegisterAccountViewModel>();
        //services.AddTransient<MenuViewModel>();
        //services.AddTransient<ForgotPasswordViewModel>();
        
        services.AddSingleton<MainWindowViewModel>();
        
        return services.BuildServiceProvider();
    }
}