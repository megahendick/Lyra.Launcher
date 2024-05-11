using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Lyra.Launcher.Functions;

public class Config
{
    public static bool UseCustomDLL = false;
    public static string CustomDLLPath = string.Empty;
    public static string MCUsername = string.Empty;
    public static bool CloseToTray = false;
    public static bool RPCEnabled = true;

    public static string Path = $"{MainWindow.LauncherPath}\\settings.json";

    public static async Task saveConfig()
    {
        if (!File.Exists(Path))
        {
            File.Create(Path);


            await Task.Delay(1000);

        }

        var ts = new ConfigData()
        {
            shouldUseCustomDLL = UseCustomDLL,
            custom_dll_path = CustomDLLPath,
            closeToTray = CloseToTray,
            rpc_enabled = RPCEnabled,
            mc_username = MCUsername,
            theme = new Theme
            {
                accentColor = App.Current.Resources["AccentColor"].ToString(),
                windowColor = App.Current.Resources["WindowColor"].ToString(),
                backgroundColor = App.Current.Resources["BackgroundColor"].ToString(),
                foregroundColor = App.Current.Resources["ForegroundColor"].ToString(),
                secondaryForegroundColor = App.Current.Resources["SecondaryForegroundColor"].ToString()
            }
        };

        var tss = JsonConvert.SerializeObject(ts);

        File.WriteAllText(Path, tss);

        MainWindow.CreateNotification(Utils.GetTranslation("Settings Saved"));
    }

    public static void loadConfig()
    {
        ConfigData config = Config.getConfig();

        if (config == null)
        {
            CustomDLLPath = "";
            MCUsername = "";
            CloseToTray = false;
            UseCustomDLL = false;
            RPCEnabled = true;
            return;
        }

        CustomDLLPath = config.custom_dll_path;
        CloseToTray = config.closeToTray;
        UseCustomDLL = config.shouldUseCustomDLL;
        RPCEnabled = config.rpc_enabled;
        MCUsername = config.mc_username;

        if (config.theme == null)
        {
            config.theme = new Theme
            {
                accentColor = "#DD44DD",
                windowColor = "#0D0D11",
                backgroundColor = "#1C1D25",
                foregroundColor = "#FFFFFF",
                secondaryForegroundColor = "#523E59"
            };
        }
        
        App.Current.Resources["AccentColor"] = ColorConverter.ConvertFromString(config.theme.accentColor);
        App.Current.Resources["WindowColor"] = ColorConverter.ConvertFromString(config.theme.windowColor);
        App.Current.Resources["BackgroundColor"] = ColorConverter.ConvertFromString(config.theme.backgroundColor);
        App.Current.Resources["ForegroundColor"] = ColorConverter.ConvertFromString(config.theme.foregroundColor);
        App.Current.Resources["SecondaryForegroundColor"] = ColorConverter.ConvertFromString(config.theme.secondaryForegroundColor);
        App.Current.Resources["ForegroundBrush"] = (SolidColorBrush)new BrushConverter().ConvertFrom(config.theme.foregroundColor);
        App.Current.Resources["SecondaryForegroundBrush"] = (SolidColorBrush)new BrushConverter().ConvertFrom(config.theme.secondaryForegroundColor);

        if (CustomDLLPath != "amongus")
        {
            //CustomDllButton.IsChecked = true;
            //dllTextBox.Visibility = Visibility.Visible;
            //dllTextBox.Text = custom_dll_path;
        }


        //TrayButton.IsChecked = closeToTray;

        //BetaDLLButton.IsChecked = shouldUseBetaDLL;
        //AutoLoginButton.IsChecked = autoLogin;
    }

    public static ConfigData getConfig()
    {
        if (!File.Exists(Path))
            return null;

        if (File.ReadAllText(Path).Length == 0)
        {
            return new ConfigData()
            {
                closeToTray = false,
                shouldUseCustomDLL = false,
                custom_dll_path = string.Empty,
                mc_username = string.Empty,
                rpc_enabled = true,
                theme = new Theme
                {
                    accentColor = "#DD44DD",
                    windowColor = "#0D0D11",
                    backgroundColor = "#1C1D25",
                    foregroundColor = "#FFFFFF",
                    secondaryForegroundColor = "#523E59"
                }
            };
        }
        var s = File.ReadAllText(Path);

        return JsonConvert.DeserializeObject<ConfigData>(s);
    }
}

public class ConfigData
{
    public string custom_dll_path;
    public string mc_username;
    public bool shouldUseCustomDLL;
    public bool closeToTray;
    public bool rpc_enabled;
    public Theme theme;
}

public class Theme
{
    public string accentColor;
    public string windowColor;
    public string backgroundColor;
    public string foregroundColor;
    public string secondaryForegroundColor;
}