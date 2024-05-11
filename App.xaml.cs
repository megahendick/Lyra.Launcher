using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace Lyra.Launcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.Dispatcher.UnhandledException += OnDispatcherUnhandledException; 
    }

    public static void ChangeLanguage(Uri uri)
    {
        Application.Current.Resources.MergedDictionaries[0].Source = uri;
    }
    
    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) 
    {
        if (e.Exception.InnerException != null)
        {
            var errorMessage = $"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.TargetSite}\n{e.Exception.StackTrace}\n{e.Exception.Source}\n{e.Exception.InnerException.Message}\n{e.Exception.InnerException.TargetSite}\n{e.Exception.InnerException.StackTrace}\n{e.Exception.InnerException.Source}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            var errorMessage = $"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.TargetSite}\n{e.Exception.StackTrace}\n{e.Exception.Source}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        e.Handled = true;
        Environment.Exit(1);
    }
}