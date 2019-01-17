using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Installer
{
    class Install
    {

        public static List<string> apkFilePaths { get; set; }
        public static List<string> imgFilePaths { get; set; }
        public static List<string> calFilePaths { get; set; }
        public static List<string> iniFilePaths { get; set; }
        public static List<string> hexFilePaths { get; set; }

        public Install()
        {
            apkFilePaths  = new List<string>();
            imgFilePaths  = new List<string>();
            calFilePaths  = new List<string>();
            iniFilePaths  = new List<string>();
            hexFilePaths  = new List<string>();
        }


        public void Install_APKs(string[] filePaths)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();


            if (filePaths.Length < 1)
                return;
            

            int l = 0;
            l = filePaths.Length;
            int count = 100 / l;
            Utilities.Progress = 5;
            foreach (var apk in filePaths)
            {
                               
                string cmd = string.Format("adb install \"{0}\"", apk);
                util.proc(cmd, true);                
                Regex rx = new Regex(@"Success");
                Thread.Sleep(1000);
                Match match = rx.Match(util.Output);
                if (!match.Success)
                    Console.WriteLine("APK alreasy Exist");
                Utilities.Progress = l + count;
                
                
            }
            Utilities.Progress = 0;
           

        }
                
        
        public void Enter_Boot_Loader(string[] images)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();
            JsonParser J_S = new JsonParser();
            FirsrHirc jsonObject = J_S.Parse();

            
            //create Hexagon
            //Create_Hexagon();

            int l = 0;

            try
            {
                util.Check_devices();
            }
            catch 
            {
                throw new AccessViolationException("The Device not responding  may be disconnected");
            }
            
            string boot = "adb reboot bootloader";

            if (!string.Equals(Utilities.ConnectionVal, "Fastboot"))
            {
                util.proc(boot, true);
                string out_ = util.Output;

                try
                {
                    Thread.Sleep(1000);
                    util.Check_devices();
                }
                catch
                {
                    throw new AccessViolationException("The Device not responding  may be disconnected");
                    //// need to add write to console the debug went down to boot
                }

                
            }


            //check id there are images as input if not take it from the file
             string[] results;
            if (images.Length < 1)
            {
                results = System.IO.File.ReadAllLines(con.Get_ToInstall());
                if (results.Length < 1)
                {
                    return;
                }
            }
            else
                results = images;


            Utilities.Pause = true;
            l = jsonObject.install.list.Count;
            int count = 100 / l;
            Utilities.Progress = 10;
            bool flag;
            foreach (var obj in jsonObject.install.list)
            {
                flag = false;
                foreach (var j in results)
                    if (j.Contains(obj.image))
                    {
                        flag = true;
                        break;
                    }
                if (!flag)
                    continue;
               
                boot = string.Format(@"fastboot flash {0} {1}\{2}", obj.partition, Path.GetDirectoryName(results[0]), obj.image);
                util.proc(boot, true);
                    
                
                int i = 0;
                while (!String.Equals(Utilities.ConnectionVal, "Fastboot") && i < 20 )
                {
                    Thread.Sleep(5000);
                    i++;
                }

                Utilities.Progress = l + count;                 
            }
            Utilities.Pause = false;

            
            boot = "fastboot reboot";
            util.proc(boot, true);
            
           
            int cnt = 0;
            while (!Utilities.ConnectionVal.Equals("Connected") && cnt < 30)
            {           
                
                Thread.Sleep(10000);
                util.Check_devices();
                cnt++;
                Console.WriteLine(string.Format("try number {0} check if connected\n", cnt.ToString()));
            }
            
            Thread.Sleep(60000);

            Utilities.Progress = 100;
            Thread.Sleep(1000);
            Utilities.Progress = 0;

        }
  

        public bool One_shot_Install()
        {
        //form to summerize the preinstall
            //in the form img, apk, cal, hexagon, config
 
            //install img
            //install apks
            //copy cal to /sdcard/iar
            //copy hexagon to "/system/lib/rfsa/adsp"

            //Dialog
            string[] dirs = { };
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)            
                dirs = dialog.FileNames.ToArray();            
            else            
                return false;
            

            if (dirs.Length < 1)          
               return false;
            

            //show all apks in the folder
             apkFilePaths = Directory.GetFiles(dirs[0], "*.apk",
                                         SearchOption.AllDirectories).ToList();

             imgFilePaths = Directory.GetFiles(dirs[0], "*.img",
                                         SearchOption.AllDirectories).ToList();

             hexFilePaths = Directory.GetFiles(dirs[0], "test*.so",
                                         SearchOption.AllDirectories).ToList();
            
             iniFilePaths = Directory.GetFiles(dirs[0], "*.ini",
                                         SearchOption.AllDirectories).ToList();

             calFilePaths = Directory.GetFiles(dirs[0], "*.cal",
                                         SearchOption.AllDirectories).ToList();



            //filter out the images not for burn

             List<string> files_ = new List<string>();
             List<string> inGame = new List<string>();

             JsonParser J_S = new JsonParser();
             FirsrHirc jsonObject = J_S.Parse();
             var t = jsonObject.install.list;

             foreach (var im in t)
                 inGame.Add(im.image);

             foreach (var file in imgFilePaths)
             {
                 string g = System.IO.Path.GetFileName(file);
                 if (inGame.Contains(g))
                     files_.Add(file);

             }

             imgFilePaths = files_;
             return true;
        }
    }
}
