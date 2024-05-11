using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Lyra.Launcher.Functions;
using Lyra.Launcher.Styles;
using Newtonsoft.Json;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Lyra.Launcher.Pages;

public partial class NewsPage : Page
{
    public static Grid MainGrid;
    public static Border OverlayBorder;
    public static TextBlock Title;
    public static TextBlock Author;
    public static TextBlock Date;
    public static WrapPanel Body;
    
    NewsRoot deserializedNews;
    
    public NewsPage()
    {
        InitializeComponent();
        MainGrid = Grid;
        OverlayBorder = Border;
        Title = TitleTextBlock;
        Author = AuthorTextBlock;
        Date = DateTextBlock;
        Body = BodyTextPanel;
        
        var newsUrl = (string)App.Current.Resources["NewsUrl"];
        try
        {
            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString(newsUrl);
                deserializedNews = JsonConvert.DeserializeObject<NewsRoot>(text);
            }

            foreach (News item in deserializedNews.News)
            {
                WrapPanel.Children.Add(
                    new NewsItem
                    {
                        Title = item.Title,
                        Body = item.Body,
                        Author = item.Author,
                        Date = item.Date
                    }
                );
            }
        }
        catch (Exception e)
        {
            MainWindow.CreateNotification(Utils.GetTranslation("Unable to get news"));
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        UnBlurSomething(MainGrid, OverlayBorder);
    }
    
    private async void UnBlurSomething(FrameworkElement control, FrameworkElement control2)
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            To = 0,
            From = 7.5,
            Duration = new TimeSpan(0, 0, 0, 0, 200)
        };
        
        var animation2 = new DoubleAnimation
        {
            To = 0,
            From = 1,
            Duration = new TimeSpan(0, 0, 0, 0, 200)
        };
        var effect = new BlurEffect();
        control.Effect = effect;
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, control.Effect);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Radius"));
        storyboard.Children.Add(animation2);
        Storyboard.SetTarget(animation2, control2);
        Storyboard.SetTargetProperty(animation2, new PropertyPath("Opacity"));
        storyboard.Begin();
        control.Effect = new BlurEffect { Radius = 0};
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        control2.Visibility = Visibility.Hidden;
    }
}

public class News
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Author { get; set; }
    public string Date { get; set; }
}

public class NewsRoot
{
    public List<News> News { get; set; }
}
