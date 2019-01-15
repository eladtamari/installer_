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


        public void Install_APKs(string[] filePaths)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();
            
            int l = 0;
            l = filePaths.Length;
            int count = 100 / l;
            Utilities.Progress = 5;
            foreach (var apk in filePaths)
            {
                               
                string cmd = string.Format("adb install {0}", apk);
                util.proc(cmd, true);                
                Regex rx = new Regex(@"Success");
                Match match = rx.Match(util.Output);
                if (!match.Success)
                    Console.WriteLine("APK alreasy Exist");
                Utilities.Progress = l + count;
                
                
            }
            Utilities.Progress = 0;
           

        }
                
        
        public void Enter_Boot_Loader()
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

            var results = System.IO.File.ReadAllLines(con.Get_ToInstall());
            if (results.Length < 1)
            {
                return;
            }   

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
            Utilities.Progress = 100;
            Thread.Sleep(1000);
            Utilities.Progress = 0;


            


        }
    }
}
