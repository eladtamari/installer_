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

            CalibrationFiles = new List<string>();
            ConfigFiles = new List<string>();
            
        }

       
        public static bool Connect { get; set; }
        public static string  Hexagon { get; set; }
        public static bool EnableDebug { get; set; }
        public static Item TextToLog { get; set; }
        Constants con = new Constants();
        public static int Progress { get; set; }
        public static bool Pause { get; set; }
        public string Output { get; set; }
        public string Err { get; set; }
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
                {
                    Connect = true;
                    ConnectionColor = Constants.Pass_Color;
                }
                else
                {
                    Connect = false;
                    ConnectionColor = Constants.Fail_Color;
                    
                }
               
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

        public static List<string> ConfigFiles { get; set; }
        public static  List<string> CalibrationFiles { get; set; }
        public ConsoleContent dc { get; set; }

        public void Check_Hexagon_File()
        {            
            proc(string.Format("adb shell ls {0}", con.Get_Hexagon()));
            Thread.Sleep(500);
            Regex rx = new Regex(@"testsig.*");
            Match matchCon = rx.Match(Output);
            if (matchCon.Success)
            {
                Hexagon = matchCon.Value;
            }
        }


        public void Reboot()
        {
            Progress = 10;
            proc(string.Format("adb reboot"));
            Progress = 90;

            Thread.Sleep(5000);
            Progress = 0;
        }

        public void Check_Config_Files_Etc()
        {            
            string d = ConfigFiles.ToString();
            proc(string.Format("adb shell ls {0}", con.Get_Etc_Iar_Path()));
            Thread.Sleep(500);
            Regex rx = new Regex("config.ini");
            Match matchCon = rx.Match(Output);
            if (matchCon.Success)
            {
                if (!d.Contains(matchCon.Value))
                    ConfigFiles.Add(string.Format(@"\etc\iar\{0}", matchCon.Value));
            }
        }

        public void Check_Calib_Files()
        {
            string c = CalibrationFiles.ToString();
            string d = ConfigFiles.ToString();
            proc(string.Format("adb shell ls {0}", con.Calib()));
            Thread.Sleep(500);
            Regex rx = new Regex("config.ini");
            if (string.IsNullOrEmpty(Output))
                return;
             
            Match matchCon = rx.Match(Output);
                if (matchCon.Success)
                {
                    if (!d.Contains(matchCon.Value))
                        ConfigFiles.Add(matchCon.Value);
                }


            Regex rxDebug = new Regex("debugConfig.ini");
            Match matchConDebug = rxDebug.Match(Output);
            if (matchConDebug.Success)
            {
                if (!d.Contains(matchConDebug.Value))
                ConfigFiles.Add(matchConDebug.Value);
            }
            foreach (string cal in con.Get_Cal_Files())
            {

                Regex rx_ = new Regex(cal);
                Match match = rx_.Match(Output);
                if (match.Success)
                {  
                    if (!c.Contains(match.Value))
                        CalibrationFiles.Add(match.Value);
                }
                
            }

        }
        //Check if device connected
        public string Find_File(string hint)
        {
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            
            //FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + hint + "*.*");
            FileInfo[] files = dir.GetFiles(string.Format("{0}*", hint), SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
                return files.First().ToString();
            else
                return "";
            
        }


        public void Get_Release_and_Engine_Version()
        {
            Constants con = new Constants();
            string cmd = string.Format("adb shell \"sh {0}/{1}\"",con.Get_Etc_Iar_Path(), con.Get_Versions());
            
            TextToLog.Text += string.Format("{0}\n", cmd);
                        
            proc(cmd);
            var o = Output;
            Regex rx = new Regex(@"Build.*");
            Match match = rx.Match(Output);
            if (!match.Success)
            {
                TextToLog.Text += string.Format("couldn't receive the build number\n");
                //throw new NullReferenceException();
            }
            Release = match.Value;
           
            Regex rx1 = new Regex(@"IAR Engine Version.*");
            Match match1 = rx1.Match(o);
            if (!match1.Success)
            {
                TextToLog.Text += string.Format("couldn't receive the Engine Version\n");
                //throw new NullReferenceException();
            }
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
            Thread.Sleep(1000);
            Regex rx = new Regex(@"\D+\d+.*\t");
            Match match = rx.Match(Output);
            Output = "";
            if (!match.Success)
            {
                proc("fastboot devices");
                Regex rx1 = new Regex(@"fastboot.*");
                Match match1 = rx.Match(Output);
                if (!match1.Success)
                {
                    TextToLog.Text += "cant find devices, and device isn't in fastboot either\n";
                    ConnectionVal = con.Disconnected();
                    return;
                }
                else
                {
                    ConnectionVal = "Fastboot";
                    SerialNum = Output.Split('\t')[0];
                    Output = "";
                    return;
                }
            }
            ConnectionVal = con.Connected();
            if (!string.IsNullOrEmpty(match.Value))
                SerialNum = match.Value.Split('\n')[1];        
            Output = "";                    
        }

        
        public void proc(string cmd, bool log = false, int timeout = 60000, bool python = false )
        {

            if (log)
                TextToLog.Text += string.Format("{0}\n", cmd);
            
            Process process = new Process();
                
            if (python)
            {
                process.StartInfo.FileName = @"C:\projects\Installer\Installer\tools\python\python.exe";
                process.StartInfo.Arguments = String.Format("{0}", cmd);
            }

            else
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = String.Format("/c {0}", cmd);
            }
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
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
                //return;        

            }
            Thread.Sleep(500);
            StreamWriter streamWriter = process.StandardInput;
            StreamReader outputReader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;
            while (!outputReader.EndOfStream)
            {
                string text = outputReader.ReadLine();
                
                streamWriter.WriteLine(text + "\n");               
                Output += text + "\n";
                if (log)
                    TextToLog.Text += string.Format("{0}\n", text);
                Regex re = new Regex(@"Agree\? \[y/n]:");
                Match matchHex = re.Match(text);
                if (matchHex.Success)
                {
                    process.StandardInput.WriteLine("y\n");
                    if (log)
                        TextToLog.Text += string.Format("y\n");
                    Thread.Sleep(1000);
                    process.StandardInput.Close();
                    Thread.Sleep(1000);
                    streamWriter.Close();
                 
                }

            }
          
            while (!errorReader.EndOfStream)
            {
                string text = errorReader.ReadLine();
                streamWriter.WriteLine(text);
                Err += text;
                if (log)
                    TextToLog.Text += string.Format("{0}\n", text);
            }

            streamWriter.Close();
            process.WaitForExit();
               

        }
            //string cv_error = null;
            //Thread et = new Thread(() => { cv_error = process.StandardError.ReadToEnd(); });
            
            //et.Start();
            
            //Err += cv_error;

            //string cv_out = null;
            //Thread ot = new Thread(() => { cv_out = process.StandardOutput.ReadToEnd(); });
            
            //ot.Start();
            //Output += cv_out;

            //process.WaitForExit();
            //ot.Join();
            //et.Join();
            //while ((!process.StandardOutput.EndOfStream))
            //{                 
            //    string line = process.StandardOutput.ReadLine();
            //    //process.WaitForExit();
            //    Output += String.Format("{0}\n",line);
            //    if (log)
            //        TextToLog.Text += string.Format("{0}\n", line);

            //}//while

            //string lineErr = "";
            //while (!process.StandardError.EndOfStream)
            //     lineErr = process.StandardError.ReadLine();
            //    Err += String.Format("{0}\n", lineErr);
            //    if (log)
            //        TextToLog.Text += string.Format("{0}\n", lineErr);

            //if (process.Id != null)
            //    if (!process.HasExited)
            //    {
            //        process.CloseMainWindow();
            //        //process.Kill();
            //    }
                
            //process.Close();

        public void Adb_Connect_Disconnect()
        {

            if (Connect)
            {
                proc(string.Format("adb connect {0}:{1}", con.Get_Adb_Addr_Port()[0], con.Get_Adb_Addr_Port()[1]), true);
                Thread.Sleep(5000);
                Regex rx = new Regex(string.Format("connected to {0}:{1}", con.Get_Adb_Addr_Port()[0], con.Get_Adb_Addr_Port()[1]));
                if (string.IsNullOrEmpty(Output))
                    throw new IOException("output is empty");
                Match matchCon = rx.Match(Output);
                if (!matchCon.Success)
                {
                    throw new IOException("Cannot connect to device");
                }
               
            }
            else
            {
                proc(string.Format("adb disconnect"));
                Thread.Sleep(5000);
            }

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
