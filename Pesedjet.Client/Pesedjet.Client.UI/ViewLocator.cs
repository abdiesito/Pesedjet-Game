using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Pesedjet.Client.UI.ViewModels;

namespace Pesedjet.Client.UI;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        Control? view = null;

        if (param is not null)
        {
            string viewModelName = param.GetType().FullName ?? string.Empty;
            string viewName = viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);
            Type? viewType = Type.GetType(viewName);

            if (viewType is not null)
            {
                view = Activator.CreateInstance(viewType) as Control;
            }
            else
            {
                view = new TextBlock { Text = "Not Found: " + viewName };
            }
        }

        return view;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}