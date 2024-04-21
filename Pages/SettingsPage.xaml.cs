using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Lyra.Launcher.Functions;
using Lyra.Launcher.Styles;
using Microsoft.Win32;

namespace Lyra.Launcher.Pages;

public partial class SettingsPage : Page
{
    public static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe", "RoamingState", "Lyra", "Config");
    
    public SettingsPage()
    {
        InitializeComponent();
        CustomDLLButton.IsChecked = Config.UseCustomDLL;
        MinimizeToTrayButton.IsChecked = Config.CloseToTray;
        RPCEnabledButton.IsChecked = Config.RPCEnabled;
        CustomDLLTextBox.Text = Config.CustomDLLPath;
        MCUsernameTextBox.Text = Config.MCUsername;

        AccentColorTextBox.Text = App.Current.Resources["AccentColor"].ToString();
        WindowColorTextBox.Text = App.Current.Resources["WindowColor"].ToString();
        BackgroundColorTextBox.Text = App.Current.Resources["BackgroundColor"].ToString();
        ForegroundColorTextBox.Text = App.Current.Resources["ForegroundColor"].ToString();
        SecondaryForegroundColorTextBox.Text = App.Current.Resources["SecondaryForegroundColor"].ToString();

        if (!Directory.Exists(ConfigPath))
            return;
        
        foreach (var path in Directory.GetFiles(ConfigPath))
        {
            ConfigStackPanel.Children.Add(new ConfigItem{ Tag = path.Remove(path.Length - 5).Remove(0, ConfigPath.Length + 1) });
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (Config.RPCEnabled != RPCEnabledButton.IsChecked)
            MainWindow.CreateNotification("Please restart the launcher to apply these changes");
        
        Config.UseCustomDLL = CustomDLLButton.IsChecked ?? false;
        Config.CloseToTray = MinimizeToTrayButton.IsChecked ?? false;
        Config.RPCEnabled = RPCEnabledButton.IsChecked ?? false;
        Config.CustomDLLPath = CustomDLLTextBox.Text;
        Config.MCUsername = MCUsernameTextBox.Text;

        try { App.Current.Resources["AccentColor"] = ColorConverter.ConvertFromString(AccentColorTextBox.Text); }
        catch (Exception) { MainWindow.CreateNotification("Accent color is invalid"); }
        try { App.Current.Resources["WindowColor"] = ColorConverter.ConvertFromString(WindowColorTextBox.Text); }
        catch (Exception) { MainWindow.CreateNotification("Window color is invalid"); }
        try { App.Current.Resources["BackgroundColor"] = ColorConverter.ConvertFromString(BackgroundColorTextBox.Text); }
        catch (Exception) { MainWindow.CreateNotification("Background color is invalid"); }
        
        try
        {
            App.Current.Resources["ForegroundColor"] = ColorConverter.ConvertFromString(ForegroundColorTextBox.Text);
            App.Current.Resources["ForegroundBrush"] = (SolidColorBrush)new BrushConverter().ConvertFrom(ForegroundColorTextBox.Text);
        }
        catch (Exception) { MainWindow.CreateNotification("Foreground color is invalid"); }
        try
        {
            App.Current.Resources["SecondaryForegroundColor"] = ColorConverter.ConvertFromString(SecondaryForegroundColorTextBox.Text);
            App.Current.Resources["SecondaryForegroundBrush"] = (SolidColorBrush)new BrushConverter().ConvertFrom(SecondaryForegroundColorTextBox.Text);

        }
        catch (Exception) { MainWindow.CreateNotification("Secondary foreground color is invalid"); }

        Config.saveConfig();
    }

    private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
    {
        var diag = new OpenFileDialog
        {
            Filter = "dll files (*.dll)|*.dll",
            RestoreDirectory = true
        };

        if (diag.ShowDialog().GetValueOrDefault())
            ((TextBox)((Control)e.OriginalSource).TemplatedParent).Text = diag.FileName;
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(ConfigPath))
        {
            MainWindow.CreateNotification("Config directory not found");
            return;
        }
        
        if ((string)ExportButton.Content == "Export")
        {
            Animations.ConfigStackPanelOpen(ConfigStackPanel);

            foreach (ConfigItem configItem in ConfigStackPanel.Children)
            {
                configItem.IsChecked = false;
            }
            
            ExportButton.Content = "Confirm";
            ImportButton.Content = "Cancel";
        }
        else if ((string)ExportButton.Content == "Confirm")
        {
            var i = 0;
            var diag = new OpenFolderDialog();
            if (diag.ShowDialog() ?? false)
            {
                foreach (ConfigItem config in ConfigStackPanel.Children)
                {
                    if (!config.IsChecked) continue;
                    var fileName = config.Tag + ".json";
                    var source = Path.Combine(ConfigPath, fileName);
                    var destination = Path.Combine(diag.FolderName, fileName);
                
                    FileRoutines.CopyFile(new FileInfo(source), new FileInfo(destination), CopyFileOptions.AllowDecryptedDestination);
                    i++;
                }
                
                MainWindow.CreateNotification($"Exported {i} config(s) to {diag.FolderName}");
            }
            
            Animations.ConfigStackPanelClose(ConfigStackPanel);
            
            ExportButton.Content = "Export";
            ImportButton.Content = "Import";
        }
    }

    private void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(ConfigPath))
        {
            MainWindow.CreateNotification("Config directory not found");
            return;
        }
        
        if ((string)ImportButton.Content == "Import")
        {
            var diag = new OpenFileDialog()
            {
                Filter = "json files (*.json)|*.json",
                RestoreDirectory = true,
                Multiselect = true
            };
            if (!(diag.ShowDialog() ?? false)) return;
            foreach (var file in diag.FileNames)
            {
                var destination = Path.Combine(ConfigPath, Path.GetFileName(file));
                FileRoutines.CopyFile(new FileInfo(file), new FileInfo(destination), CopyFileOptions.AllowDecryptedDestination);
            }

            ConfigStackPanel.Children.Clear();
            
            foreach (var path in Directory.GetFiles(ConfigPath))
            {
                ConfigStackPanel.Children.Add(new ConfigItem{ Tag = path.Remove(path.Length - 5).Remove(0, ConfigPath.Length + 1) });
            }
        }
        else if ((string)ImportButton.Content == "Cancel")
        {
            Animations.ConfigStackPanelClose(ConfigStackPanel);
            
            ExportButton.Content = "Export";
            ImportButton.Content = "Import";
        }
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(ConfigPath))
        {
            MainWindow.CreateNotification("Config directory not found");
            return;
        }
        
        ConfigStackPanel.Children.Clear();
            
        foreach (var path in Directory.GetFiles(ConfigPath))
        {
            ConfigStackPanel.Children.Add(new ConfigItem{ Tag = path.Remove(path.Length - 5).Remove(0, ConfigPath.Length + 1) });
        }
    }

    private async void RedownloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindow.DllDownloaded = false;
        MainWindow.DllDownloadedFailed = false;
        RedownloadButton.Content = "Downloading DLL";
        RedownloadButton.IsEnabled = false;
        
        try
        {
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync("https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/Lyra.dll");
            using var fs = new FileStream(MainWindow.LauncherPath + "\\Lyra.dll", FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);

            MainWindow.DllDownloaded = true;
            MainWindow.CreateNotification("Successfully downloaded DLL");
        }
        catch (Exception)
        {
            MainWindow.DllDownloaded = false;
            MainWindow.CreateNotification("Unable to download DLL");
        }
        
        RedownloadButton.Content = "Download";
        RedownloadButton.IsEnabled = true;
    }
}

[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public class HexToBrushConverter : IValueConverter
{
    private SolidColorBrush _previousValidBrush = Brushes.Black;
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        SolidColorBrush brush;
        
        try
        {
            brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString((string)value));
        }
        catch (Exception e)
        {
            return _previousValidBrush;
        }

        _previousValidBrush = brush;
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Brushes.Black;
    }
}