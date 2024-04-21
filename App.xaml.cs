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

    void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) 
    {
        if (e.Exception.InnerException != null)
        {
            var errorMessage = $"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.InnerException.Message}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            var errorMessage = $"An unhandled exception occurred: {e.Exception.Message}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        e.Handled = true;
        Environment.Exit(1);
    }
}