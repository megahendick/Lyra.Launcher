using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Lyra.Launcher;

public class Animations
{
    public static void PageScrollAnimation(StackPanel stackPanel, double num)
    {
        var animation = new ThicknessAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut },
            To = new Thickness(0,num,0,0)
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTargetName(animation, stackPanel.Name);
        Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
        
        storyboard.Begin(stackPanel);
    }

    public static void NotificationClose(UserControl userControl, double num)
    {
        var animation = new ThicknessAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseIn },
            To = new Thickness(0,0,num * -1 - 35,0)
        };
        
        var animation2 = new DoubleAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            BeginTime = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut },
            To = 0
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        storyboard.Children.Add(animation2);
        Storyboard.SetTarget(animation, userControl);
        Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
        Storyboard.SetTarget(animation2, userControl);
        Storyboard.SetTargetProperty(animation2, new PropertyPath(FrameworkElement.HeightProperty));
        
        storyboard.Begin(userControl);
    }

    public static void ServerPopUpOpen(Border border)
    {
        var animationX = new DoubleAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase() {EasingMode = EasingMode.EaseOut},
            To = 1
        };

        var animationY = animationX.Clone();

        var storyboard = new Storyboard();
        storyboard.Children.Add(animationX);
        storyboard.Children.Add(animationY);
        Storyboard.SetTarget(animationX, border);
        Storyboard.SetTarget(animationY, border);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
        Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
        
        storyboard.Begin(border);
    }
    
    public static void ServerPopUpClose(Border border)
    {
        var animationX = new DoubleAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase() {EasingMode = EasingMode.EaseIn},
            To = 0
        };

        var animationY = animationX.Clone();

        var storyboard = new Storyboard();
        storyboard.Children.Add(animationX);
        storyboard.Children.Add(animationY);
        Storyboard.SetTarget(animationX, border);
        Storyboard.SetTarget(animationY, border);
        Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
        Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
        
        storyboard.Begin(border);
    }

    public static void ConfigStackPanelOpen(StackPanel stackPanel)
    {
        var storyboard = new Storyboard();
        
        var animation = new ThicknessAnimation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            To = new Thickness(0,0,0,0),
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseOut}
        };
        
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, stackPanel);
        Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
        
        storyboard.Begin(stackPanel);
    }
    
    public static void ConfigStackPanelClose(StackPanel stackPanel)
    {
        var storyboard = new Storyboard();
        
        var animation = new ThicknessAnimation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            To = new Thickness(-20,0,0,0),
            EasingFunction = new QuadraticEase{ EasingMode = EasingMode.EaseIn}
        };
        
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, stackPanel);
        Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
        
        storyboard.Begin(stackPanel);
    }
    
    public static void FriendsListOpen(Border border)
    {
        var animation = new DoubleAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut },
            To = 1
        };


        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, border);
        Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
        
        storyboard.Begin(border);
    }
    
    public static void FriendsListClose(FrameworkElement border)
    {
        var animation = new DoubleAnimation()
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut },
            To = 0
        };


        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, border);
        Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Begin();
    }
    
    public static void BlurElement(FrameworkElement control, double fromRadius, double toRadius, TimeSpan duration)
    {
        var animation = new DoubleAnimation
        {
            From = fromRadius,
            To = toRadius,
            Duration = duration
        };
        control.Effect = new BlurEffect();
        control.Effect.BeginAnimation(BlurEffect.RadiusProperty, animation);
    }
    
    public static void AnimateBrushOpacity(SolidColorBrush brush, double fromRadius, double toRadius, TimeSpan duration)
    {
        var animation = new DoubleAnimation
        {
            From = fromRadius,
            To = toRadius,
            Duration = duration
        };
        
        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, brush);
        Storyboard.SetTargetProperty(animation, new PropertyPath(Brush.OpacityProperty));
        
        storyboard.Begin();
    }
}