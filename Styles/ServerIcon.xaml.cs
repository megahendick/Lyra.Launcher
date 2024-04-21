using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Lyra.Launcher.Pages;

namespace Lyra.Launcher.Styles;

public partial class ServerIcon : UserControl
{
    public string ServerName { get; set; }
    public string IconURL { get; set; }
    public string Discord { get; set; }
    public string Ip { get; set; }
    public string Port { get; set; }
    
    public ServerIcon()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        HomePage.AddButton.Tag = Port == "realm" ? "Add Realm" : "Add Server";
        HomePage.ServerName.Text = ServerName;
        HomePage.ServerDiscord = Discord;
        HomePage.ServerPort = Port;
        HomePage.ServerIP = Ip;
        HomePage.ServerBorder.Visibility = Visibility.Visible;
        Animations.ServerPopUpOpen(HomePage.ServerBorder);
    }
}

[ValueConversion(typeof(String), typeof(BitmapImage))]
public class ImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;

        string fullFilePath = value.ToString();

        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
        bitmap.EndInit();

        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 0;
    }
}