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
                util.proc(cmd);                
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
                util.proc(boot);
                string out_ = util.Output;

                try
                {
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
            foreach (var obj in jsonObject.install.list)
            {
                if (!results.Contains(obj.image))
                    continue;
               
                boot = string.Format(@"fastboot flash {0} {1}\{2}", obj.partition, Path.GetDirectoryName(results[0]), obj.image);
                util.proc(boot);
                    
                
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
            util.proc(boot);
            Utilities.Progress = 0;

        }
    }
}
