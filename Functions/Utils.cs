namespace Lyra.Launcher.Functions;

public class Utils
{
    public static string GetTranslation(string input, string[]? args = null)
    {
        try
        {
            if (args is not null)
            {
                var temp = (string)App.Current.TryFindResource(input);
                for (var i = 0; i < args.Length; i++)
                {
                    temp = temp.Replace($"{{{i}}}", args[i]);
                }
                return temp;
            }
            var result = App.Current.TryFindResource(input);
            if (result is not null)
                return (string)result;
            MainWindow.CreateNotification("Error getting translation for: " + input);
            return input;
        }
        catch (Exception e)
        {
            MainWindow.CreateNotification("Error getting translation for: " + input);
            return input;
        }
    }
}