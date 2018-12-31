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
        public string Pass_Color { get; set; }
        public string Fail_Color { get; set; }
       
        public Constants()
        {
            Pass_Color = "#00b33c";
            Fail_Color = "#FFDC9D96";
        }


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

        const string hexagonPath = "/system/lib/rfsa/adsp";
        const string calPath = "/sdcard/iar/";
        const string debugEtcPath = "/etc/iar/";
        string[] calFiles = {"imu.cal", "display.cal", "camera.cal"};
        const string configFile = "config.ini";
        const string debugConfigFile = "debugConfig.ini";
        const string devices = "adb devices";

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
            return debugEtcPath;
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

        public string Hexagon()
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
            return String.Format("{0} {1} {2}", adb, push, item, target);
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
