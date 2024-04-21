using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Lyra.Launcher.Pages;
using Newtonsoft.Json;

namespace Lyra.Launcher.Styles;

public partial class NewsItem : UserControl
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Author { get; set; }
    public string Date { get; set; }

    public NewsItem()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    private void BlurSomething(FrameworkElement control, FrameworkElement control2)
    {
        control2.Visibility = Visibility.Visible;
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 7.5,
            Duration = new TimeSpan(0, 0, 0, 0, 200)
        };
        
        var animation2 = new DoubleAnimation
        {
            From = 0,
            To = 1,
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
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        NewsPage.Title.Text = Title;
        NewsPage.Author.Text = Author;
        NewsPage.Date.Text = Date;
        NewsPage.Body.Children.Clear();
        if (Body.Contains('{'))
        {
            string St = Body;
            int pFrom;
            int pTo = 0;

            while (St.Contains('{'))
            {
                pFrom = St.IndexOf("{") + 1;
                pTo = St.IndexOf("}");

                string result = St.Substring(pFrom - 1, pTo - pFrom + 2);
                var img = JsonConvert.DeserializeObject<NewsImage>(result);
                NewsPage.Body.Children.Add(new TextBlock
                    {
                        Style = this.FindResource("NewsBodyTextStyle") as Style,
                        Text = St.Substring(0, pFrom - 1)
                    }
                );
                NewsPage.Body.Children.Add(new Image()
                    {
                        Height = img.Height,
                        Width = img.Width,
                        Source = new BitmapImage(new Uri(img.URL))
                    }
                );
                St = St.Substring(pTo + 1);
            }
            
            NewsPage.Body.Children.Add(new TextBlock
                {
                    Style = this.FindResource("NewsBodyTextStyle") as Style,
                    Text = St
                }
            );
        }
        else
        {
            NewsPage.Body.Children.Add(new TextBlock
                {
                    Style = this.FindResource("NewsBodyTextStyle") as Style,
                    Text = Body
                }
            );
        }
        BlurSomething(NewsPage.MainGrid, NewsPage.OverlayBorder);
    }
}

public class NewsImage
{
    public double Height { get; set; }
    public double Width { get; set; }
    public string URL { get; set; }
}