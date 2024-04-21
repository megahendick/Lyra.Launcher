using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Lyra.Launcher.Functions;
using Lyra.Launcher.Pages;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

namespace Lyra.Launcher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static readonly string LauncherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lyra", "Launcher");
    public static readonly RoutedCommand ShowCommand = new();
    public static string? CurrentVersion;
    private static StackPanel _nsp;
    public static bool DllDownloaded;
    public static bool DllDownloadedFailed;
    
    public MainWindow()
    {
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lyra", "Launcher")))
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lyra", "Launcher"));
        
        DownloadDll();
        Minecraft.Init();
        CurrentVersion = Minecraft.GetVersion().ToString();
        InitializeComponent();
        _nsp = NotificationStackPanel;
        Config.loadConfig();
        IsVersionSupported();
        if (Config.RPCEnabled)
            Dispatcher.InvokeAsync((Action)(() => Functions.DiscordRPC.Initialize()));
    }

    public static void CreateNotification(string message)
    {
        var n = new Styles.Notification { Message = message };
        _nsp.Children.Insert(0, n);
    }

    private async void DownloadDll()
    {
        try
        {
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync("https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/Lyra.dll");
            using var fs = new FileStream(LauncherPath + "\\Lyra.dll", FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);

            DllDownloaded = true;
        }
        catch (Exception)
        {
            DllDownloadedFailed = true;
            CreateNotification("Unable to download DLL");
        }
    }
    
    private void IsVersionSupported()
    {
        try
        {
            var fileContent = new WebClient().DownloadString("https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/versions.txt")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (fileContent.Any(version => version.Trim() == CurrentVersion))
                return;

            CreateNotification(CurrentVersion + " is unsupported, please use one of the following versions: " +
                               string.Join(' ', fileContent));
        }
        catch (Exception)
        {
            CreateNotification("Unable to get supported versions");
        }
    }
    
    private void ShowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Navigate_Home(object sender, RoutedEventArgs e)
    {
        Animations.PageScrollAnimation(PageStackPanel, 0);
    }
    
    private void Navigate_News(object sender, RoutedEventArgs e)
    {
        Animations.PageScrollAnimation(PageStackPanel, -430);
    }
    
    private void Navigate_Settings(object sender, RoutedEventArgs e)
    {
        Animations.PageScrollAnimation(PageStackPanel, -860);
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        RadioButton1.IsChecked = true;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (Config.CloseToTray == false)
            Close();
        
        TaskbarIcon.Visibility = Visibility.Visible;
        Hide();
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItem_Inject(object sender, RoutedEventArgs e)
    {
        if (Config.UseCustomDLL)
        {
            Injector.Inject(Config.CustomDLLPath);
        }
        else
        {
            if (DllDownloadedFailed)
            {
                CreateNotification("The DLL download failed, please check your internet connection or disable your antivirus software");
                return;
            }
            
            if (!DllDownloaded)
            {
                CreateNotification("Please wait a few seconds for the dll to finish downloading");
                return;
            }
            
            Injector.Inject(LauncherPath + "\\Lyra.dll");
        }
    }
}
