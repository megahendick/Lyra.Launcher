using System.Windows;
using System.Windows.Controls;

namespace Lyra.Launcher.Styles;

public partial class Notification : UserControl
{
    public string Message { get; set; }
    
    public Notification()
    {
        InitializeComponent();
        this.DataContext = this;

        Margin = new Thickness(0,0,ActualWidth * -1 - 35,0);
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Animations.NotificationClose(this, ActualWidth);
        await Task.Delay(250);
        ((Panel)this.Parent).Children.Remove(this);
    }

    private void Notification_OnLoaded(object sender, RoutedEventArgs e)
    {
        Height = ActualHeight;
    }
}