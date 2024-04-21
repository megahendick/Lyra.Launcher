using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Lyra.Launcher.Functions;
using Lyra.Launcher.Styles;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Lyra.Launcher.Pages;

public partial class HomePage : Page
{
    public static Border ServerBorder;
    public static TextBlock ServerName;
    public static Button AddButton;
    public static string ServerIP;
    public static string ServerPort;
    public static string ServerDiscord;

    private Root deserializedServers;
    Dictionary<string, User> users;
    Friends friends;
    System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

    public HomePage()
    {
        InitializeComponent();
        ServerBorder = SB;
        ServerName = SN;
        AddButton = AB;
        
        const string serversUrl = "https://raw.githubusercontent.com/megahendick/Lyra.Launcher.CDN/main/servers.json";
        
        dispatcherTimer.Tick += dispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0,0,15);
        
        try
        {
            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString(serversUrl);
                deserializedServers = JsonConvert.DeserializeObject<Root>(text);
            }

            foreach (Server item in deserializedServers.servers)
            {
                ServerStackPanel.Children.Add(
                    new ServerIcon()
                    {
                        ServerName = item.name,
                        IconURL = item.icon,
                        Discord = item.discord,
                        Ip = item.ip,
                        Port = item.port
                    }
                );
            }
        }
        catch (Exception e)
        {
            MainWindow.CreateNotification("Unable to get partnered servers");
        }
    }

    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString("http://node2.synthetix.host:5004/friends/" + Config.MCUsername);
                friends = JsonConvert.DeserializeObject<Friends>(text);
            }

            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString("http://node2.synthetix.host:5004/users");
                users = JsonConvert.DeserializeObject<Dictionary<string, User>>(text);
            }

            FriendsStackPanel.Children.Clear();

            if (friends.friends.Count == 0)
            {
                var image = new Image();
                var fullFilePath = @"https://m.media-amazon.com/images/M/MV5BNmYxNjc0ZDAtYjJlMi00MzZhLTkzMDctNjZhZGRkYTBhNWE1XkEyXkFqcGdeQXVyNjgyODE4NTE@._V1_.jpg";

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();

                image.Source = bitmap;
                image.Height = 180;
                image.Width = 320;
                image.Stretch = Stretch.UniformToFill;

                FriendsStackPanel.Children.Add(image);
                return;
            }
            
            foreach (var friend in friends.friends)
            {
                FriendsStackPanel.Children.Add(users.TryGetValue(friend, out var user)
                    ? new UserItem { Username = friend, Server = user.server }
                    : new UserItem { Username = friend, Server = "Offline" });
            }
        }
        catch (Exception)
        {
            MainWindow.CreateNotification("Unable to get friends");
        }
    }
    
    private void HomePage_OnLoaded(object sender, RoutedEventArgs e)
    {
        LaunchButton.Content = MainWindow.CurrentVersion;
    }

    private void AddServer(object sender, RoutedEventArgs e)
    {
        Process.Start(ServerPort == "realm"
            ? new ProcessStartInfo($"minecraft://acceptRealmInvite?inviteID={ServerIP}") { UseShellExecute = true }
            : new ProcessStartInfo($"minecraft:?addExternalServer={ServerName.Text}|{ServerIP}:{ServerPort}")
                { UseShellExecute = true });
    }

    private void OpenDiscordLink(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(ServerDiscord) { UseShellExecute = true });
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Animations.ServerPopUpClose(ServerBorder);
        await Task.Delay(200);
        ServerBorder.Visibility = Visibility.Hidden;
    }

    private void LaunchButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Config.UseCustomDLL)
        {
            Injector.Inject(Config.CustomDLLPath);
        }
        else
        {
            if (MainWindow.DllDownloadedFailed)
            {
                MainWindow.CreateNotification("The DLL download failed, please check your internet connection or disable your antivirus software");
                return;
            }
            
            if (!MainWindow.DllDownloaded)
            {
                MainWindow.CreateNotification("Please wait a few seconds for the dll to finish downloading");
                return;
            }

            Injector.Inject(MainWindow.LauncherPath + "\\Lyra.dll");
        }
    }

    private async void Toggle_Friends_List(object sender, RoutedEventArgs e)
    {
        dispatcherTimer_Tick(new object(), EventArgs.Empty);
        
        switch (FriendsBorder.Visibility)
        {
            case Visibility.Hidden when string.IsNullOrEmpty(Config.MCUsername):
                MainWindow.CreateNotification("Please enter your Minecraft username on the settings page");
                break;
            case Visibility.Hidden:
                FriendsBorder.Visibility = Visibility.Visible;
                dispatcherTimer.Start();
                Animations.FriendsListOpen(FriendsBorder);
                break;
            case Visibility.Visible:
                Animations.FriendsListClose(FriendsBorder);
                await Task.Delay(200);
                dispatcherTimer.Stop();
                FriendsBorder.Visibility = Visibility.Hidden;
                break;
        }
    }
}

public class Root
{
    public List<Server> servers { get; set; }
}

public class Server
{
    public string name { get; set; }
    public string icon { get; set; }
    public string discord { get; set; }
    public string ip { get; set; }
    public string port { get; set; }
}

public class User
{
    public long lastbeat { get; set; }
    public string icon { get; set; }
    public string server { get; set; }
}

public class Friends
{
    public List<string> friends { get; set; }
}