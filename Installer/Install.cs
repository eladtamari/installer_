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


        public void Install_APKs(string dir)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();
           
            string[] filePaths = Directory.GetFiles(dir, "*.apk",
                                         SearchOption.TopDirectoryOnly);

            foreach (var apk in filePaths)
            {
                string cmd = string.Format("adb install {0}", apk);
                util.proc(cmd);
                Thread.Sleep(5000);
            }

        }
        
        public void Enter_Boot_Loader(string[] installDir)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();
            JsonParser J_S = new JsonParser();
            FirsrHirc jsonObject = J_S.Parse();

            
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

            Utilities.Pause = true;

            foreach (var obj in jsonObject.install.list)
            {
                boot = string.Format(@"fastboot flash {0} {1}\{2}", obj.partition, installDir[0], obj.image);
                util.proc(boot);
                int i = 0;
                
                while (!String.Equals(Utilities.ConnectionVal, "Fastboot") && i < 20 )
                {
                    Thread.Sleep(5000);
                    i++;
                }
                                                          
            }
            Utilities.Pause = false;

            boot = "fastboot reboot";
            util.proc(boot);

        }
    }
}
