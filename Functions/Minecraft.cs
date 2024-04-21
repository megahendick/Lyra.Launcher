using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Management.Deployment;
namespace Lyra.Launcher.Functions;

public class Minecraft
{
        public static Process Process;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.ApplicationModel.PackageId PackageId => Package.Id ?? throw new NullReferenceException();
        public static Windows.System.ProcessorArchitecture PackageArchitecture => PackageId.Architecture;
        public static Windows.ApplicationModel.PackageVersion PackageVersion => PackageId.Version;
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static void Init()
        {
            if (!IsInstalled())
            {
                MessageBox.Show("Minecraft is not installed!");
                Environment.Exit(1);
            }
            
            InitManagers();
            FindPackage();

            

            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }

        }

        private static bool IsInstalled()
        {
            PackageManager = new PackageManager();
            var packages = PackageManager.FindPackagesForUser(string.Empty,FamilyName);
            return packages.Any();
        }

        private static void InitManagers()
        {
            PackageManager = new PackageManager();
            var packages = PackageManager.FindPackagesForUser(string.Empty,FamilyName);
            
            if (!packages.Any())
            {

                MessageBox.Show("Minecraft needs to be installed");
                Environment.Exit(0);
            }
            else
            {
                ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
            }

            //  ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);


        }

        private static void FindPackage()
        {

            if (PackageManager is null) throw new NullReferenceException();
            var packages = PackageManager.FindPackagesForUser(string.Empty,FamilyName);

            if (packages.Count() != 0)
            {
                Package = packages.First();
            }
        }

        private static double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }
        public static Version GetVersion()
        {
            var v = PackageVersion;

          double buildVersion =  (double)v.Build;
           var Ok = RoundToSignificantDigits((int)buildVersion, 2);
          ;
            return new Version(v.Major, v.Minor,
               int.Parse(Ok.ToString().Substring(0, 2)));
        }
        
        public static bool IsGameOpen()
        {
            Process[] mc = Process.GetProcessesByName("Minecraft.Windows");
            return mc.Length > 0;
        }
        
        public static async Task WaitForModules()
        {
            if(Process is { HasExited: true }) Process = null;
            
            while (Process == null)
            {
                var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
                if (mcIndex.Length > 0)
                {
                    
                    Process = mcIndex[0];

                }
                
                await Task.Delay(100);
            }

            while (true)
            {
                Process.Refresh();
                if (Process.HasExited) continue;
                if (Process.Modules.Count <= 160) continue;
                await Task.Delay(1500);
                break;
            }
        }
}