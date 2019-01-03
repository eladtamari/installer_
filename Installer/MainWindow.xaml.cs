using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ConsoleContent dc = new ConsoleContent();
        //public string Connection_Val { get; set; }
        //public string Connection_Color { get; set; }
        
        public string Release_Val { get; set; }
        public string Serial_Val { get; set; }
        Utilities util = new Utilities();
        Constants con = new Constants();


        // public DataContext MyProperty { get; set; }
        public MainWindow()
        {
            InitializeComponent();
           
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();

            BackgroundWorker worker_check_connection = new BackgroundWorker();
            worker_check_connection.DoWork += worker_CheckConnection;
            worker_check_connection.RunWorkerAsync();

            BackgroundWorker worker_check_battery = new BackgroundWorker();
            worker_check_battery.DoWork += worker_CheckBattary;
            worker_check_battery.RunWorkerAsync();

            BackgroundWorker worker_check_release = new BackgroundWorker();
            worker_check_release.DoWork += worker_CheckRelease;
            worker_check_release.RunWorkerAsync();

            //util.PropertyChanged += util.Configuration_PropertyChanged;


            // util.dc = new ConsoleContent();
            DataContext = util.dc;
            Loaded += MainWindow_Loaded;
            var t = Task.Run(() =>
            {
                try
                {
                    util.Check_devices();
                  
                }
                catch (Exception ex)
                {
                   
                    //util.change_color = true;
                    Console.WriteLine(ex);
                }
            });


        }



        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                util.dc.ConsoleInput = InputBlock.Text;
                util.dc.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }

        private void b_install_Click(object sender, RoutedEventArgs e)
        {
            string[] dirs = {};
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            //CommonFileDialogResult result = dialog.ShowDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                 dirs = dialog.FileNames.ToArray();
            }
            Install install = new Install();

            
            var run = Task.Run(() => install.Enter_Boot_Loader(dirs)).ContinueWith(failedTask => Console.WriteLine("Device is not responding"),
                             TaskContinuationOptions.OnlyOnFaulted); 
            
        }

        private void b_pull_Click(object sender, RoutedEventArgs e)
        {

            util.dc.ConsoleInput = "Start Pull Files...";
            var t = Task.Run(() => pull());
            util.dc.ConsoleInput = "Done Pull Files...";
        }

        private void b_push_Click(object sender, RoutedEventArgs e)
        {
            util.dc.ConsoleInput = "Start Push Files...";
            var t = Task.Run(() => push());
            util.dc.ConsoleInput = "Done Push Files...";

        }

        private void b_enable_disable_debug_Click(object sender, RoutedEventArgs e)
        {
            var t = Task.Run(() => pullEditPush());
        }

        private void pull()
        {
            PushPullFiles pushPull = new PushPullFiles();
            Utilities util = new Utilities();
            try
            {
                util.Check_devices();
                pushPull.Pull();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

        }

        private void push()
        {
            Utilities util = new Utilities();
            util.dc.ConsoleInput = "Start Pushing";
            Constants con = new Constants();

            PushPullFiles pushPull = new PushPullFiles();


            try
            {
                util.Check_devices();
                pushPull.Push();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        private void pullEditPush()
        {
            Utilities util = new Utilities();
            PushPullFiles pushPull = new PushPullFiles();
            try
            {
                util.Check_devices();
                pushPull.pullEditPush();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }

        private void b_browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.ShowDialog();
        }


        void worker_CheckBattary(object sender, DoWorkEventArgs e)
        {
            Utilities util = new Utilities();
            while (true)
            {
                if (!Utilities.Pause)
                {
                    try
                    {
                        util.Check_Battary_Level();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
 
                }
                Thread.Sleep(5000);
            }
        }

        void worker_CheckRelease(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!Utilities.Pause)
                {
                    try
                    {
                        util.Get_Release_and_Engine_Version();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        void worker_CheckConnection(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    util.Check_devices();
                    //Utilities.ConnectionVal = con.Connected();
                   
                }
                catch (Exception ex)
                {
                    Utilities.ConnectionVal = con.Disconnected();
                   
                }
                Thread.Sleep(5000);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            var bc = new BrushConverter();
            while (true)
            {

                //(sender as BackgroundWorker).ReportProgress(Configuration.Instance.ProgressValue);
                this.Dispatcher.Invoke(() =>
                {
                   l_connected.Content = Utilities.ConnectionVal;

                });

                this.Dispatcher.Invoke(() =>
               {
                   l_connected.Background = (Brush)bc.ConvertFromString(Utilities.ConnectionColor);

               });

                if (!Utilities.Pause)
                {
                    this.Dispatcher.Invoke(() =>
                   {
                       l_serial_val.Content = Utilities.SerialNum;

                   });

                    this.Dispatcher.Invoke(() =>
                    {
                        float a = Utilities.BatteryLevel / 1000000f;
                        l_battery.Content = string.Format("{0}", a);

                    });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_battery.Background = (Brush)bc.ConvertFromString(Utilities.BackColor);

                    });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_release_val.Content = Utilities.Release;

                    });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_engine_val.Content = Utilities.Engine;

                    });

                }
                else 
                {
                    this.Dispatcher.Invoke(() =>
                   {
                       l_battery.Content = "N/A";
                   });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_release_val.Content = "N/A";

                    });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_engine_val.Content = "N/A";

                    });
                }

                Thread.Sleep(1000);
            }



        }

        private void b_install_apk_Click(object sender, RoutedEventArgs e)
        {
            string[] dirs = { };
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                dirs = dialog.FileNames.ToArray();
            }
            Install install = new Install();
            if (Directory.Exists(dirs[0]))
                Task.Run(() => install.Install_APKs(dirs[0]));
            else
                throw new FileNotFoundException(string.Format("Cant find path {0}", dirs[0]));
            
        }

       
    }
}
