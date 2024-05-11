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
using System.Windows.Threading;
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
    public static bool VersionSupported;

    private static Panel _dsp;
    private static Grid _mg;
    private static Grid _dg;
    private static Border _db;
    private static TextBlock _dTitle;
    private static TextBlock _dBody;
    private static SolidColorBrush _dbgSolidColorBrushb;
    
    public MainWindow()
    {
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lyra", "Launcher")))
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lyra", "Launcher"));
        
        DownloadDll();
        try
        {
            Minecraft.Init();
            CurrentVersion = Minecraft.GetVersion().ToString();
        }
        catch (Exception e)
        {
            CreateNotification(Utils.GetTranslation("Unable to find minecraft version, please try running the launcher as admin"));
            CurrentVersion = Utils.GetTranslation("Unknown Version");
        }
        InitializeComponent();
        _nsp = NotificationStackPanel;
        _mg = MainGrid;
        _db = DialogBorder;
        _dsp = DialogStackPanel;
        _dTitle = DialogTitle;
        _dBody = DialogBody;
        _dbgSolidColorBrushb = SolidColorBrush;
        _dg = DialogGrid;
        
        Config.loadConfig();
        IsVersionSupported();
        if (Config.RPCEnabled)
            Dispatcher.InvokeAsync((Action)(() => Functions.DiscordRPC.Initialize()));
    }
    
    public static async Task<int> CreateDialog(string title, string body, string[] buttons)
    {
        const double blurAmount = 10;
        var result = buttons.Length - 1;
        var _event = new SemaphoreSlim(0);

        _dg.Visibility = Visibility.Visible;
        _dsp.Children.Clear();
        Animations.ServerPopUpOpen(_db);
        Animations.BlurElement(_mg, 0, blurAmount, TimeSpan.FromMilliseconds(250));
        Animations.AnimateBrushOpacity(_dbgSolidColorBrushb, 0, 0.3, TimeSpan.FromMilliseconds(250));

        _dTitle.Text = title;
        _dBody.Text = body;
        foreach (var text in buttons)
        {
            var btn = new Button
            {
                Content = text,
                Margin = new Thickness(2.5,5,2.5,0),
                Template = (ControlTemplate)App.Current.Resources["SmallButtonStyle"],
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btn.Click += BtnOnClick;

            _dsp.Children.Add(btn);
            continue;

            async void BtnOnClick(object sender, RoutedEventArgs e)
            {
                result = _dsp.Children.IndexOf(sender as UIElement);
                _event.Release();
                Animations.ServerPopUpClose(_db);
                Animations.BlurElement(_mg, blurAmount, 0, TimeSpan.FromMilliseconds(250));
                Animations.AnimateBrushOpacity(_dbgSolidColorBrushb, 0.3, 0, TimeSpan.FromMilliseconds(250));
                await Task.Delay(250);
                _dg.Visibility = Visibility.Hidden;
            }
        }

        await _event.WaitAsync();
        return result;
    }
    
    public static void CreateNotification(string message)
        => _nsp.Children.Insert(0, new Styles.Notification { Message = message });
    
    public static async void DownloadDll()
    {
        try
        {
            using var client = new HttpClient();
            await using var s = await client.GetStreamAsync("https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/Lyra.dll");
            await using var fs = new FileStream(LauncherPath + "\\Lyra.dll", FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
            
            DllDownloaded = true;
        }
        catch (Exception)
        {
            DllDownloadedFailed = true;
            CreateNotification(Utils.GetTranslation("Unable to download DLL"));
        }
    }
    
    private static void IsVersionSupported()
    {
        try
        {
            var fileContent = new WebClient().DownloadString("https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/versions.txt")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (fileContent.Any(version => version.Trim() == CurrentVersion))
            {
                VersionSupported = true;
                return;
            }

            CreateNotification(Utils.GetTranslation("{0} is unsupported, please use one of the following versions: {1}", [CurrentVersion, string.Join(' ', fileContent)]));
        }
        catch (Exception)
        {
            CreateNotification(Utils.GetTranslation("Unable to get supported versions"));
        }

        VersionSupported = false;
    }
    
    private void ShowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Navigate_Home(object sender, RoutedEventArgs e)
        => Animations.PageScrollAnimation(PageStackPanel, 0);
    
    private void Navigate_News(object sender, RoutedEventArgs e)
        => Animations.PageScrollAnimation(PageStackPanel, -430);
    
    private void Navigate_Settings(object sender, RoutedEventArgs e)
        => Animations.PageScrollAnimation(PageStackPanel, -860);

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        => RadioButton1.IsChecked = true;

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (Config.CloseToTray == false)
            Close();
        
        TaskbarIcon.Visibility = Visibility.Visible;
        Hide();
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e) => Close();

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
                CreateNotification(Utils.GetTranslation("The DLL download failed, please check your internet connection or disable your antivirus software"));
                return;
            }
            
            if (!DllDownloaded)
            {
                CreateNotification(Utils.GetTranslation("Please wait a few seconds for the DLL to finish downloading"));
                return;
            }
            
            Injector.Inject(LauncherPath + "\\Lyra.dll");
        }
    }
}