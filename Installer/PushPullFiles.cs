using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Installer
{
    class PushPullFiles
    {
        
        

        public void Push()
        {
            Constants con = new Constants();
            Utilities util = new Utilities();
            var t = util.Find_File(con.Get_Hexagon_File());
            //push hexagon file  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            util.proc(con.Push_Item(t, con.Hexagon()));

            //push calib files
            foreach (string cal in con.Get_Cal_Files())
            {
                util.proc(con.Push_Item(cal, con.Get_Iar_Path()));
            }
        }

        
        public void Pull()
        {
            //ConsoleContent dc = new ConsoleContent();
            Utilities util = new Utilities();
            Constants con = new Constants();
            // pull the hexagon
            try
            {
                util.dc.ConsoleInput = "pull Hexagon";
                util.proc(con.Pull_Item("", con.Hexagon()));
                bool fe = File.Exists(con.Hexagon()) ? true : false;
                if (!fe)
                    throw new FileNotFoundException();

            }
            catch (FileNotFoundException ex)
            {
                util.dc.ConsoleInput = ex.Message.ToString();
            }

            //pull the calibration files        
            try
            {
                foreach (string cal in con.Get_Cal_Files())
                {
                    util.proc(con.Pull_Item(con.Get_Iar_Path(), cal));
                    bool fe = File.Exists(cal) ? true : false;
                    if (!fe)
                        throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }

            //pull the configuration files
            try
            {
                util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_Config_file()));
                util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_debugConfig_File()));

                foreach (var c in new string[] { con.Get_Config_file(), con.Get_debugConfig_File() })
                {
                    bool fe = File.Exists(c) ? true : false;
                    if (!fe)
                        throw new FileNotFoundException();
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }

        }

        public void pullEditPush()
        {
            Utilities util = new Utilities();
            Constants con = new Constants();
            util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_debugConfig_File()));

            //edit 
            try
            {
                SearchEditValue();
            }
            catch (Exception ex)
            { 
                throw ex;
            }


            //to sdcard
            util.proc(con.Push_Item(con.Get_debugConfig_File(), con.Get_Iar_Path()));
            //to etc
            util.proc(con.Push_Item(con.Get_debugConfig_File(), con.Get_Etc_Iar_Path()));


        }

        
        public void SearchEditValue()
        {
            Constants con = new Constants();
            bool flag = false;
            var t = File.ReadLines(con.Get_debugConfig_File());

            if (t.Count() == 0)
                throw new FileNotFoundException();

            string[] f = t.ToArray();
            
             // Define a regular expression for repeated words.
             Regex rx = new Regex(@"disableAllLogs.*");       
        
            for (int i = 0; i < f.Count(); i++)
            {
                Match match = rx.Match(f[i]);
                if (match.Groups.Count != 0)
                {
                    GroupCollection groups = match.Groups;
                    if (String.IsNullOrEmpty(groups[0].ToString()))
                        continue;

                    Console.WriteLine(groups[0]);                    
                    Console.ReadLine();
                    flag = true;
                    f[i] = "disableAllLogs = false";
                    string d = string.Join("\n", f);
                    break;
                }
            }
            if (!flag)
                throw new InvalidDataException();

            
        }

       
    }
}
