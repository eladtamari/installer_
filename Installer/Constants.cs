using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer
{

    
    enum Images {boot, userdata, system, persist}
    class Constants

    {
        public static string Pass_Color { get; set; }
        public static string Fail_Color { get; set; }
        public static string Fastboot_Color { get; set; }
        public static string Warning_Color { get; set; }
       
        public Constants()
        {
            Pass_Color = "#99e699";//"#00b33c";
            Fail_Color =  "#ff6666";  //"#FFDC9D96";
            Fastboot_Color = "#94b8b8";
            Warning_Color = "#ffc266";
            
        }

        const string adbIpAddress = "192.168.137.1";
        const string adbPort = "5556";

        const string hexagonPyCreator = "elfsigner.py";
        const string toInstall = "ToInstall.txt";
        const string jsonFile = "Config.JSON";
        const string connected = "Connected";
        const string disconnected = "Disconnected";
        const string adb = "adb";
        const string shell = "shell";
        const string pull = "pull";
        const string push = "push";
        const string reboot = "reboot";
        const string flash = "flash";
        const string fastboot = "fastboot";
        const string hexagonFile = "testsig";
        const string bootloader = "bootloader";

        const string versionsScript = "iarversions.sh";
        const string soc0Id = "/sys/devices/soc0/serial_number";
        const string hexagonCreatorTool = "tools/elfsigner";
        const string voltage = "/sys/class/power_supply/battery/voltage_now";
        const string hexagonPath = "/system/lib/rfsa/adsp";
        const string calPath = "/sdcard/iar/";
        const string etcPath = "/etc/iar/";
        string[] calFiles = {"imu.cal", "display.cal", "camera.cal"};
        const string configFile = "config.ini";
        const string debugConfigFile = "debugConfig.ini";
        const string devices = "adb devices";
       
        string fastBootDevices = "fastboot devices";

        public string[] Get_Adb_Addr_Port()
        {
            string[] ipAddressAndPort = { adbIpAddress, adbPort };
            return ipAddressAndPort;
        }

        public string Get_Elfsigner_Py()
        {
            return hexagonPyCreator;
        }

        public string Get_Elfsigner()
        {
            return hexagonCreatorTool;
        }

        public string Get_Soc0_Id()
        {
            return soc0Id;
        }
        
        
        public string Get_ToInstall()
        {
            return toInstall;
        }

        public string Get_Json_file_name()
        {
            return jsonFile;
        }

       

        public string Get_Versions()
        {
            return versionsScript;
        }

        public string Get_Voltage()
        {
            return voltage;
        }

        public string Get_Bootloader()
        {
            return bootloader;
        }

        public string Connected()
        {
            return connected;
        }

        public string Disconnected()
        {
            return disconnected;
        }


        public string Get_Hexagon_File()
        {
            return hexagonFile;
        }

        public string Get_Devices()
        {
            return devices;
        }

        public string Get_Etc_Iar_Path()
        {
            return etcPath;
        }

        public string Get_Iar_Path()
        {
            return calPath;
        }

        public string Get_Config_file()
        {
            return configFile;
        }

        public string Get_debugConfig_File()
        {
            return debugConfigFile;
        }

        public string Calib()
        {
            return calPath;
        }

        public string Get_Hexagon()
        {
            return hexagonPath;
        }

        public string Boot_loader()
        {
            return String.Format("{0} {1} bootloader", adb, reboot);
        }

        public string Flash_Image(string target, string source)
        {
            return String.Format("{0} {1} {2} {3}", fastboot, flash, target, source);
        }

        public string Pull_Item(string path, string item)
        {
            return String.Format("{0} {1} {2}/{3}", adb, pull, path, item);
        }

        public string Push_Item(string item, string target)
        {
            return String.Format("{0} {1} {2} {3}", adb, push, item, target);
        }

        public string[] Get_Cal_Files()
        {
            return calFiles;
        }

        
    }

    public static class Calibrations
    {       

        public class CalibFiles
        {

            public System.Collections.Generic.IEnumerable<CalibFile> NextCalibFile
            {
                get
                {
                    yield return new CalibFile { Calib = "imu.cal" };
                    yield return new CalibFile { Calib = "display.cal" };
                    yield return new CalibFile { Calib = "camera.cal" };
                }
            }
        }

        public class CalibFile
        {
            public string Calib { get; set; }
        }


    }

   
}
