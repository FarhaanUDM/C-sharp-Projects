using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PC_Monitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* 
         Speed Needle :
            Rotation  -90 deg -  0%
                      178 deg - 100%
                      2.68 per %
         */

        public MainWindow()
        {
            InitializeComponent();

            //Recuperer les infos du PC
            GetAllSystemInfo();

            //Recuperer les infos disques
            GetDriveInfo();

            //Timer pour Mise a Jour en temps reel
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.75);
            tmr.Tick += tmr_Tick;
            tmr.Start();
        }

        // Function Timer - Refresh screen 2x per sec
        public void tmr_Tick(object sender, EventArgs e)
        {
            // Mise a Jour Info CPU
            cpu.Content = RefreshCpuInfo();
            // Mise a Jour Infos RAM
            RefreshRamInfos();
            // Mise a Jour Temperature
            //RefreshTempInfos();
            // Mise a Jour network
            RefreshNetworkInfos();
        }

        public void GetDriveInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<Disque> drives = new List<Disque>();
            
            foreach (DriveInfo info in allDrives)
            {
                if (info.IsReady == true)
                {
                    drives.Add(new Disque(info.Name, info.DriveFormat, FormatBytes(info.TotalSize), FormatBytes(info.AvailableFreeSpace)));
                }
                listDisk.ItemsSource = drives ;
            }
        }

        public string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i=0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, suffix[i]);
        }   

        public void RefreshNetworkInfos()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return;
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface iface in interfaces)
            {
                if (iface.GetIPv4Statistics().BytesSent > 0)
                    netUp.Content = iface.GetIPv4Statistics().BytesSent/1000 + " KB";
                
                if (iface.GetIPv4Statistics().BytesReceived > 0)
                    netDown.Content = iface.GetIPv4Statistics().BytesReceived / 1000 + " KB";
            }
        }

        public void RefreshTempInfos()
        {
            Double temperature = 0;
            String instanceName = "";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI","select * from MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject obj in searcher.Get())
            {
                temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                // Convert F to C
                temperature = (temperature - 2732) / 10.0;
                instanceName = obj["InstanceName"].ToString();
            }
            temp.Content = temperature + " °C";
        }

        public void RefreshRamInfos()
        {
            ramTotal.Content = "Total : " + FormatSize(GetTotalPhys());
            ramUsed.Content = "Used : " + FormatSize(GetUsedPhys());
            ramFree.Content = "Free : " + FormatSize(GetAvailPhys());
            
            string[] maxVal = FormatSize(GetTotalPhys()).Split(' ');
            ramProgress.Maximum = float.Parse(maxVal[0]);
            string[] memVal = FormatSize(GetUsedPhys()).Split(' ');
            ramProgress.Value = float.Parse(memVal[0]);
        }


        public string RefreshCpuInfo()
        {
            // Recuperer les info du Cpu
            PerformanceCounter pc = new PerformanceCounter();
            pc.CategoryName = "Processor";
            pc.CounterName = "% Processor Time";
            pc.InstanceName = "_Total";
             
            dynamic firstVal = pc.NextValue();
            System.Threading.Thread.Sleep(75); // Always remain zero
            dynamic val = pc.NextValue();

            // Faire tourner l'aiguille
            RotateTransform rotate = new RotateTransform((val*2.68f) - 90);
            aiguille.RenderTransform = rotate;

            decimal roundVal = Convert.ToDecimal(val);
            roundVal = Math.Round(roundVal, 2);

            return roundVal + " %";
        }

        /** public object GetRAMCounter()
        {
            PerformanceCounter ramCounter = new PerformanceCounter();
            ramCounter.CategoryName = "Memory";
            ramCounter.CounterName = "Available MB";

            dynamic firstVal = ramCounter.NextValue();
            System.Threading.Thread.Sleep(100);
            dynamic val = ramCounter.NextValue();

            decimal roundVal = Convert.ToDecimal(val);
            roundVal = Math.Round(roundVal);

            return roundVal;
        }
         */

        // Working with RAM memory
        #region Fonctions specifiques a la RAM
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

        // Structure de l'info de la mÃ©moire
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
            public uint dwLength; // Taille structure
            public uint dwMemoryLoad; // Utilisation mÃ©moire
            public ulong ullTotalPhys; // MÃ©moire physique totale
            public ulong ullAvailPhys; // MÃ©moire physique dispo
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual; // Taille mÃ©moire virtuelle
            public ulong ullAvailVirtual; // MÃ©moire virtuelle dispo
            public ulong ullAvailExtendedVirtual;
        }

        static string FormatSize(double size)
        {
            double d = (double)size;
            int i = 0;
            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }
            string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
        }

        public static MEMORY_INFO GetMemoryStatus()
        {
            MEMORY_INFO mi = new MEMORY_INFO();
            mi.dwLength = (uint)Marshal.SizeOf(mi);
            GlobalMemoryStatusEx(ref mi);
            return mi;
        }

        // RÃ©cupÃ©ration mÃ©moire physique totale dispo
        public static ulong GetAvailPhys()
        {
            MEMORY_INFO mi = GetMemoryStatus();
            return mi.ullAvailPhys;
        }

        // RÃ©cupÃ©ration mÃ©moire utilisÃ©e
        public static ulong GetUsedPhys()
        {
            MEMORY_INFO mi = GetMemoryStatus();
            return (mi.ullTotalPhys - mi.ullAvailPhys);
        }

        // RÃ©cup la mÃ©moire physique totale
        public static ulong GetTotalPhys()
        {
            MEMORY_INFO mi = GetMemoryStatus();
            return mi.ullTotalPhys;
        }

        #endregion
        public void GetAllSystemInfo()
        {
            SystemInfo si = new SystemInfo();
            os.Content = "Operating System : " + si.GetOsInfos("os");
            archi.Content = "Architecture : " + si.GetOsInfos("archi");
            proc.Content = "Processor : " + si.GetCpuInfos();
            gpu.Content = "GPU : " + si.GetGpuInfos();
        }

        private void infoMsg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/profile.php?id=61550858802712");
        }
    
    }

    public class SystemInfo
    {
        // recuperer infos os
        public string GetOsInfos(string param)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in mos.Get())
            {
                switch (param)
                {
                    case "os":
                        return obj["Caption"].ToString();
                    case "archi":
                        return obj["OSArchitecture"].ToString();
                    case "osv":
                        return obj["CSDVersion"].ToString();
                }
            }
            return "";
        }

        public string GetCpuInfos()
        {
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);

            if (processor_name != null)
            {
                  return processor_name.GetValue("ProcessorNameString").ToString();
            }
            return "";
        }

        //Recuperer info carte graphique
        public string GetGpuInfos()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Console.WriteLine("Name :" + obj["Name"]);
                    Console.WriteLine("DriverVersion - " + obj["DriverVersion"]);
                    Console.WriteLine("DeviceID :" + obj["DeviceID"]);
                    Console.WriteLine("Adapter RAM :" + obj["AdapterDACType"]);

                    return obj["Name"].ToString() + "(Version Driver :" + obj["DriverVersion"].ToString() + ")";
                }
            }
            return "";
        }
    }

    public class Disque
    {
        private string name;
        private string format;
        private string totalSpace;
        private string freeSpace;

        public Disque(string n,string f,string t,string l)
        {
            name = n;
            format = f;
            totalSpace = t;
            freeSpace = l;
        }

        public override string ToString()
        {
            return name + "(" + format + ")"+freeSpace+ "free /"+ totalSpace ;
        }

    }

}
