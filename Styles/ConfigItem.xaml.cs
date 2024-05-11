using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lyra.Launcher.Functions;
using Lyra.Launcher.Pages;
using Microsoft.VisualBasic.ApplicationServices;

namespace Lyra.Launcher.Styles;

public partial class ConfigItem : UserControl
{
    public bool IsChecked
    {
        get => CheckBox.IsChecked ?? false;
        set => CheckBox.IsChecked = value;
    }
    
    public ConfigItem()
    {
        InitializeComponent();
    }

    private void Edit(object sender, RoutedEventArgs e)
    {
        var process = new Process();
        process.StartInfo = new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = $"{SettingsPage.ConfigPath}\\{Tag}.json"
        };

        process.Start();
    }
    
    private void Delete(object sender, RoutedEventArgs e)
    {
        File.Delete($"{SettingsPage.ConfigPath}\\{Tag}.json");
        MainWindow.CreateNotification(Utils.GetTranslation("Deleted the {0} config", [$"\"{Tag}\""]));

        var storyboard = new Storyboard();
        
        var animation = new ThicknessAnimation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            To = new Thickness(-480,0,0,0),
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseIn}
        };
        
        var animation2 = new DoubleAnimation()
        {
            Duration = TimeSpan.FromMilliseconds(150),
            BeginTime = TimeSpan.FromMilliseconds(150),
            To = 0,
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseOut}
        };
        
        storyboard.Children.Add(animation);
        storyboard.Children.Add(animation2);
        Storyboard.SetTarget(animation, this);
        Storyboard.SetTarget(animation2, this);
        Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
        Storyboard.SetTargetProperty(animation2, new PropertyPath(FrameworkElement.HeightProperty));
        
        storyboard.Begin();
    }

    private void CheckBox_OnClick(object sender, RoutedEventArgs e)
        => IsChecked = CheckBox.IsChecked ?? false;

    private void ConfigItem_OnLoaded(object sender, RoutedEventArgs e)
    {
        Height = ActualHeight;
    }
}