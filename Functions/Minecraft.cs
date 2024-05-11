using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        // https://github.com/flarialmc/launcher/pull/7/commits/cf11941c79fe5fe64625e3e7731c7ec51dc7ed50
        [DllImport("kernel32.dll")]
        static extern long GetPackagesByPackageFamily(
        [MarshalAs(UnmanagedType.LPWStr)] string packageFamilyName,
        ref uint count,
        IntPtr packageFullNames,
        ref uint bufferLength,
        IntPtr buffer);

        [ComImport]
        [Guid("f27c3930-8029-4ad1-94e3-3dba417810c1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPackageDebugSettings
        {
            long EnableDebugging(
            [MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
            [MarshalAs(UnmanagedType.LPWStr)] string debuggerCommandLine,
            [MarshalAs(UnmanagedType.LPWStr)] string environment);
            long DisableDebugging([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long Suspend([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long Resume([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long TerminateAllProcesses([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long SetTargetSessionId(ulong sessionId);
            long StartServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long StopServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long StartSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, ulong sessionId);
            long StopSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long GetPackageExecutionState([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, IntPtr packageExecutionState);
            long RegisterForPackageStateChanges(
            [MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
            IntPtr pPackageExecutionStateChangeNotification,
            IntPtr pdwCookie);
            long UnregisterForPackageStateChanges(ulong dwCookie);
        }

        public static void SetMinimizeFix(bool state)
        {
            // state: false = off, true = on
            // https://github.com/flarialmc/launcher/pull/7/commits/cf11941c79fe5fe64625e3e7731c7ec51dc7ed50
            IPackageDebugSettings pPackageDebugSettings = (IPackageDebugSettings)Activator.CreateInstance(
                Type.GetTypeFromCLSID(new Guid(0xb1aec16f, 0x2383, 0x4852, 0xb0, 0xe9, 0x8f, 0x0b, 0x1d, 0xc6, 0x6b, 0x4d)));
            uint count = 0, bufferLength = 0;
            GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, IntPtr.Zero, ref bufferLength, IntPtr.Zero);
            IntPtr packageFullNames = Marshal.AllocHGlobal((int)(count * IntPtr.Size)),
                buffer = Marshal.AllocHGlobal((int)(bufferLength * 2));
            GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, packageFullNames, ref bufferLength, buffer);
            for (int i = 0; i < count; i++)
            {
                if (state) { pPackageDebugSettings.EnableDebugging(Marshal.PtrToStringUni(Marshal.ReadIntPtr(packageFullNames)), null, null); }
                else { pPackageDebugSettings.DisableDebugging(Marshal.PtrToStringUni(Marshal.ReadIntPtr(packageFullNames))); }
                packageFullNames += IntPtr.Size;
            }
            Marshal.FreeHGlobal(packageFullNames);
            Marshal.FreeHGlobal(buffer);
        }
        
        public static void Init()
        {
            if (!IsInstalled())
            {
                MessageBox.Show("Minecraft is not installed!");
                Environment.Exit(1);
            }
            
            InitManagers();
            FindPackage();

            var mcIndex = System.Diagnostics.Process.GetProcessesByName("Minecraft.Windows");
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
            Process[] mc = System.Diagnostics.Process.GetProcessesByName("Minecraft.Windows");
            return mc.Length > 0;
        }
}