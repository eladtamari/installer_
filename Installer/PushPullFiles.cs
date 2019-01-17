using Microsoft.WindowsAPICodePack.Dialogs;
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

        public PushPullFiles()
        {
            TextToLog = new Item();
        }
        public static Item TextToLog { get; set; }
        
        public void PushHex(string[] results, bool progress = false)
        {
            Constants con = new Constants();
            Utilities util = new Utilities();
                       


            if (progress)
                Utilities.Progress = 10;
            util.proc(con.Push_Item(results[0], con.Get_Iar_Path()), true, 5000);
            if (progress)
            {
                Utilities.Progress = 100;
                Thread.Sleep(1000);
                Utilities.Progress = 0;
            }
        }
          

        public void PushCalib(string[] results, bool progress = false)
        {
            //push calib files
            Constants con = new Constants();
            Utilities util = new Utilities();
            if (progress)
                Utilities.Progress = 20;
            foreach (string cal in results)
            {
                util.proc(con.Push_Item(cal, con.Get_Iar_Path()), true, 5000);
                if (progress)
                    Utilities.Progress += 20;
                
            }
            if (progress)
            {
                Utilities.Progress = 100;
                Thread.Sleep(1000);
                Utilities.Progress = 0;
            }

        }

        
        public void Pull(bool progress = false)
        {
            //ConsoleContent dc = new ConsoleContent();
            Utilities util = new Utilities();
            Constants con = new Constants();
            // pull the hexagon
            try
            {
                TextToLog.Text += string.Format("Pull Hexagon files");
                if (progress)
                    Utilities.Progress = 10;
                util.proc(con.Pull_Item("", con.Get_Hexagon()), true, 10000);
                if (progress)
                    Utilities.Progress = 30;
                bool fe = File.Exists(con.Get_Hexagon()) ? true : false;
                if (!fe)
                {
                    TextToLog.Text += string.Format("couldn't find file {0}", con.Get_Hexagon());
                    throw new FileNotFoundException();
                }

            }
            catch (FileNotFoundException ex)
            {
                TextToLog.Text += string.Format(ex.Message);
            }

            //pull the calibration files        
            try
            {
                foreach (string cal in con.Get_Cal_Files())
                {
                    util.proc(con.Pull_Item(con.Get_Iar_Path(), cal), true, 20000);
                    if (progress)
                        Utilities.Progress += 25;
                    bool fe = File.Exists(cal) ? true : false;
                    if (!fe)
                    {
                        TextToLog.Text += string.Format("couldn't Pull file {0}", cal);
                        throw new FileNotFoundException();
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                TextToLog.Text += string.Format(ex.Message);
                Console.WriteLine(ex);
            }

            //pull the configuration files
            try
            {
                util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_Config_file()), true, 10000);
                if (progress)
                    Utilities.Progress += 5;
                util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_debugConfig_File()), true, 10000);

                foreach (var c in new string[] { con.Get_Config_file(), con.Get_debugConfig_File() })
                {
                    bool fe = File.Exists(c) ? true : false;
                    if (!fe)
                    {
                        TextToLog.Text += string.Format("couldn't find file {0}", c);
                        throw new FileNotFoundException();
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                TextToLog.Text += string.Format(ex.Message);
                Console.WriteLine(ex);
            }
            if (progress)
            {
                Utilities.Progress = 100;
                Thread.Sleep(500);
                Utilities.Progress = 0;
            }


        }

        public void pullEditPush(Utilities util)
        {
            //Utilities util = new Utilities();
            Constants con = new Constants();
            Utilities.Progress = 10;
            util.proc(con.Pull_Item(con.Get_Iar_Path(), con.Get_debugConfig_File()), true, 10000);

            //edit 
            try
            {
                SearchEditValue();
                Utilities.Progress = 50;
            }
            catch (Exception ex)
            {
                TextToLog.Text += string.Format(ex.Message);
                throw ex;
            }


            //to sdcard
            util.proc(con.Push_Item(con.Get_debugConfig_File(), con.Get_Iar_Path()), true, 10000);
            Utilities.Progress = 70;
            //to etc
            util.proc(con.Push_Item(con.Get_debugConfig_File(), con.Get_Etc_Iar_Path()), true, 10000);
            Utilities.Progress = 100;
            Thread.Sleep(500);
            Utilities.Progress = 0;

        }

        
        public void SearchEditValue()
        {
            Constants con = new Constants();
            bool flag = false;
            
            var t = File.ReadLines(con.Get_debugConfig_File());

            if (t.Count() == 0)
            {
                TextToLog.Text += string.Format("couldn't find file {0}", con.Get_debugConfig_File());
                throw new FileNotFoundException();
            }

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
                    
                    
                    if (Utilities.EnableDebug)
                        f[i] = "disableAllLogs = false";
                    else
                        f[i] = "disableAllLogs = true";

                    string d = string.Join("\n", f);
                    try
                    {
                        File.WriteAllText(con.Get_debugConfig_File(), d);
                    }
                    catch (Exception ex)
                    {
                        TextToLog.Text += "cannot write into the debugConfig.ini";
                        throw new Exception("cannot write into the debugConfig.ini");
                        
                    }
                    break;
                }
            }
            if (!flag)
            {
                TextToLog.Text += "couldn't find match for disableAllLogs.*";
                throw new InvalidDataException();
            }

            
        }

       
    }
}
