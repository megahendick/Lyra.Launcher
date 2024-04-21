using System.Windows.Controls;

namespace Lyra.Launcher.Styles;

public partial class UserItem : StackPanel
{
    public string Username { get; set; }
    public string Server { get; set; }

    public UserItem()
    {
        InitializeComponent();
        DataContext = this;
    }
}