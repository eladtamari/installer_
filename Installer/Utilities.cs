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



        public static string BackColor { get; set; }
        public static string ConnectionVal { get; set; }
        public static string CurrentRelease { get; set; }
        public static string SerialNum { get; set; }
        

        public ConsoleContent dc { get; set; }
        public Utilities()
        {
            dc = new ConsoleContent();
        }


        

        //Check if device connected
        public string Find_File(string hint)
        {
            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            
            //FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + hint + "*.*");
            FileInfo[] files = dir.GetFiles(string.Format("{0}*", hint), SearchOption.TopDirectoryOnly);
            return files.First().ToString();
            
        }


        public void Check_devices()
        {
            Constants con = new Constants();
            proc(con.Get_Devices());
            Regex rx = new Regex(@"\D+\d+.*\t");
            Match match = rx.Match(Output);
            Output = "";
            if (!match.Success)
                throw new NullReferenceException();
            SerialNum = match.Value.Split('\n')[1];
            
           
        }

        public string Output { get; set; }
        public void proc(string cmd)
        {
            //string output = "";
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
            if (process.WaitForExit(2000))
            {
                var exitCode = process.ExitCode;
            }
            else
            {
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
