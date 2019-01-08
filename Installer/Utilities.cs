using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Installer
{
    class Utilities : INotifyPropertyChanged
    {
        
        public Utilities()
        {
            dc = new ConsoleContent();
            ConnectionColor = Constants.Fastboot_Color;
            BackColor = Constants.Fail_Color;

            TextToLog = new Item();
            
        }

        public static Item TextToLog { get; set; }
        Constants con = new Constants();
        public static int Progress { get; set; }
        public static bool Pause { get; set; }
        public string Output { get; set; }
        public static string BackColor { get; set; }
        public static string ConnectionColor { get; set; }
        private static string conVal;
        public static string ConnectionVal { 
            get
        {
            return conVal;
        }
            set
            {
                conVal = value;
                if (value == "Fastboot")
                    ConnectionColor = Constants.Fastboot_Color;
                else if (value == "Connected")
                    ConnectionColor = Constants.Pass_Color;
                else
                    ConnectionColor = Constants.Fail_Color;
               
            }
        }
        public static string CurrentRelease { get; set; }
        public static string SerialNum { get; set; }
        public static string Release { get; set; }
        public static string Engine { get; set; }

        
        private static int level;
        public static int BatteryLevel {
            get
            {
                return level;
            }
            set
            {
                if (value > 4000000)
                {
                    BackColor = Constants.Pass_Color;
                    level = value;
                }
                else if (value < 4000000 && value > 3800000)
                {
                    BackColor = Constants.Warning_Color;
                    level = value;
                }
                else
                {
                    level = value;
                    BackColor = Constants.Fail_Color;
                }
            }
        }


        public ConsoleContent dc { get; set; }
       

        //Check if device connected
        public string Find_File(string hint)
        {
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            
            //FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + hint + "*.*");
            FileInfo[] files = dir.GetFiles(string.Format("{0}*", hint), SearchOption.TopDirectoryOnly);
            return files.First().ToString();
            
        }


        public void Get_Release_and_Engine_Version()
        {
            Constants con = new Constants();
            string cmd = string.Format("adb shell \"sh {0}/{1}\"",con.Get_Etc_Iar_Path(), con.Get_Versions());
                        
            proc(cmd);
            var o = Output;
            Regex rx = new Regex(@"Build.*");
            Match match = rx.Match(Output);
            if (!match.Success)
                throw new NullReferenceException();
            Release = match.Value;
           
            Regex rx1 = new Regex(@"IAR Engine Version.*");
            Match match1 = rx1.Match(o);
            if (!match1.Success)
                throw new NullReferenceException();
            Engine = match1.Value;
            
            Output = "";
           
            
        }

        public void Check_Battary_Level()
        {
            Constants con = new Constants();
            string cmd = string.Format("adb shell \"cat {0}\"",con.Get_Voltage());
            proc(cmd);
            Regex rx = new Regex(@"\d+");
            Match match = rx.Match(Output);
            Output = "";
            if (!match.Success)
                throw new NullReferenceException();
            int bLevel;
            int.TryParse(match.Value.Trim(), out bLevel);
            BatteryLevel = bLevel;
        }


        public void Check_devices()
        {
            Constants con = new Constants();
            proc(con.Get_Devices());
            Regex rx = new Regex(@"\D+\d+.*\t");
            Match match = rx.Match(Output);
            Output = "";
            if (!match.Success)
            {
                proc("fastboot devices");
                Regex rx1 = new Regex(@"fastboot.*");
                Match match1 = rx.Match(Output);
                if (!match1.Success)
                    throw new NullReferenceException();
                else
                {
                    ConnectionVal = "Fastboot";
                    SerialNum = Output.Split('\t')[0]; 
                    Output = "";
                    return;
                }
            }
            ConnectionVal = con.Connected();
            SerialNum = match.Value.Split('\n')[1];        
            Output = "";                    
        }

        
        public void proc(string cmd, bool log = false, int timeout = 60000)
        {
            //string output = "";
            if (log)
                TextToLog.Text += string.Format("{0}\n", cmd);
                //TextToLog = new Item() { Text = string.Format("{0}\n",cmd)};
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = String.Format("/c {0}", cmd);
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            
            process.Start();
            Console.Write(process.Id.ToString());
            if (process.WaitForExit(timeout))
            {
                var exitCode = process.ExitCode;
            }
            else
            {
                if (log)
                    TextToLog.Text += string.Format("Process has exited with timeout\n");
                //process.Close();
                if (process.Id != null)
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        //process.Kill();
                        
                    }
                return;        
                
            }
            Thread.Sleep(500);
            while ((!process.StandardOutput.EndOfStream))
            {
                string line = process.StandardOutput.ReadLine();
                Output += String.Format("{0}\n",line);
                if (log)
                    TextToLog.Text += string.Format("{0}\n", line);

            }//while

            if (process.Id != null)
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    //process.Kill();
                }
                
            //process.Close();
        }

        private bool changeColor;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool change_color
        {
            get { return changeColor; }
            set
            {
                SetField(ref changeColor, value, "changeColor");
            }
        }


        public void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "changeColor")
            {
                ChangeColor();
            }
        }

        public SolidColorBrush ColorBa { get; set; }
        private void ChangeColor()
        {

            Constants con = new Constants();
            var bc = new BrushConverter();
            ColorBa = (SolidColorBrush)bc.ConvertFromString("#00b33c");
           // con.Pass_Color;
        }

       

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
            
           
           
        
    }
}
