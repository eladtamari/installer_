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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public string Release_Val { get; set; }
        public string Serial_Val { get; set; }
        Utilities util = new Utilities();
        Constants con = new Constants();
        public Item Error { get; set; }
        
        public static ObservableCollection<Item> logItems; 
       
        public MainWindow()
        {
            InitializeComponent();
            logItems = new ObservableCollection<Item>()
            {
            new Item(){Text="Logger"}
            };
            log.ItemsSource = logItems;


            Utilities.EnableDebug = false;

            string iconsPath;
            //set the infinity icon
            iconsPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "INFINITY_logo.jpg");
            if (File.Exists(iconsPath))
                logo.Source = new BitmapImage(new Uri(iconsPath, UriKind.RelativeOrAbsolute));
          
           
           
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
            //worker_check_release.RunWorkerAsync();            
          

          
            var t = Task.Run(() =>
            {
                try
                {
                    util.Check_devices();
                    Thread.Sleep(1000);
                    l_serial_val.Content = Utilities.SerialNum;
                  
                }
                catch (Exception ex)
                {
                   
                    //util.change_color = true;
                    Console.WriteLine(ex);
                }
            });


        }



        #region Install Images
        private void b_install_Click(object sender, RoutedEventArgs e)
        {
            string[] empty = {};
            string[] dir = { };
            string[] results = { };
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            //CommonFileDialogResult result = dialog.ShowDialog();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                dir = dialog.FileNames.ToArray();
            }
            Install install = new Install();

            if (dir.Length < 1)
            {
               
                return;
            }


            string[] files = Directory.GetFiles(dir[0], "*.img",
                                         SearchOption.TopDirectoryOnly);

            List<string> files_ = new List<string>();
            List<string> inGame = new List<string>();

            JsonParser J_S = new JsonParser();
            FirsrHirc jsonObject = J_S.Parse();
            var t = jsonObject.install.list;

            foreach (var im in t)
                inGame.Add(im.image);

            foreach (var file in files)
            {
                string g = System.IO.Path.GetFileName(file);
                if (inGame.Contains(g))
                    files_.Add(file);

            }


            //show all apks in the folder

            var items = new installedItems(files_.ToList());
            if (!(bool)items.ShowDialog() == true)
            {
                return;

            }


            var uiContext = SynchronizationContext.Current;
            this.Dispatcher.Invoke((Action)delegate
            {

                Task.Run(() => install.Enter_Boot_Loader(empty)).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

            });

            //var run = Task.Run(() => install.Enter_Boot_Loader()).ContinueWith(failedTask => Console.WriteLine("Device is not responding"),
            //                 TaskContinuationOptions.OnlyOnFaulted); 

        }

        private void b_pull_Click(object sender, RoutedEventArgs e)
        {
            var uiContext = SynchronizationContext.Current;
            var t = Task.Run(() => pull()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

        }

        #endregion


        #region Create Hexagon
        public void Create_Hexagon()
        {
            Utilities util = new Utilities();
            Utilities.Progress = 10;
            string hexGetId = string.Format(@"adb shell cat {0}", con.Get_Soc0_Id());
            util.proc(hexGetId, true);

            //search for hexagon 10 digits integer number
            Regex re = new Regex(@"\d+");
            Match matchHex = re.Match(util.Output);
            if (!matchHex.Success)
                Console.WriteLine("couldn't find hexagon decimal id");

            //turn number hexagon to hexadecimal
            long decValue = Convert.ToInt64(matchHex.Value, 10);
            string hexValue = decValue.ToString("X");

            Utilities.Progress = 20;
            //set dir to where hexagon factory ask it 
            string originDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(string.Format(@"../../{0}", con.Get_Elfsigner()));
            string o = Directory.GetCurrentDirectory();
            Console.WriteLine(o);
            Utilities.Progress = 25;
            util.Output = "";
            //string y = Directory.GetCurrentDirectory();
            string v = string.Format("..\\python\\python.exe {0} -t 0x{1} -o output\\{2}\n", con.Get_Elfsigner_Py(), hexValue, Utilities.SerialNum);
            try
            {
                util.proc(v, true, 3000, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                util.Err += ex.Message;
            }
            Utilities.Progress = 60;

            int cnt = 0;
            bool t = false;
            while (!t && cnt < 20)
            {
                t = Directory.EnumerateFiles(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "output", Utilities.SerialNum.Trim()), "test*.so").Any() ? true : false;
                cnt++;
                Thread.Sleep(1000);
            }

            if (!t)
                return;

            //root remount
            string root = "adb root";
            util.proc(root, true);

            Thread.Sleep(1000);
            string remount = "adb remount";
            util.proc(remount, true);
            Utilities.Progress = 66;

            var myFiles = Directory.GetFiles(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "output", Utilities.SerialNum.Trim()), "*.so", SearchOption.TopDirectoryOnly);

            //push the hexagon file to the glasses
            string pushHex = string.Format("adb push {0} /system/lib/rfsa/adsp", myFiles[0]);
            util.proc(pushHex, true);
            Utilities.Progress = 90;
            //return dir to the working dir
            Directory.SetCurrentDirectory(originDir);

            Utilities.Progress = 100;
            Thread.Sleep(1000);
            Utilities.Progress = 0;
        } 
        #endregion

        private void b_push_hex_Click(object sender, RoutedEventArgs e)
        {
            var uiContext = SynchronizationContext.Current;
            
            this.Dispatcher.Invoke((Action)delegate
            {

                Task.Run(() => Create_Hexagon()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

            });
            
            


        }

        #region push calibration files
        private void b_push_cal_Click(object sender, RoutedEventArgs e)
        {


            string[] dirs = { };
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                dirs = dialog.FileNames.ToArray();
            }
            Install install = new Install();

            if (dirs.Length < 1)
            {
                util.dc.ConsoleInput = "Path wasn't specified, to complete APK install, give an exist path";
                return;
            }

            //show all apks in the folder
            string[] filePaths = Directory.GetFiles(dirs[0], "*.cal",
                                         SearchOption.AllDirectories);


            string[] results = { };
            var items = new installedItems(filePaths.ToList());
            if ((bool)items.ShowDialog() == true)
            {
                results = System.IO.File.ReadAllLines(con.Get_ToInstall());

            }

            var uiContext = SynchronizationContext.Current;



            var t = Task.Run(() => pushCalib(results)).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
            if (Utilities.Progress == 100)
                if (t.Status == TaskStatus.Faulted)
                {
                    foreach (var i in t.Exception.InnerExceptions)
                    {
                        Error.Text = i.Message;
                        logItems.Add(Error);

                    }
                }


        } 
        #endregion

        
        private void b_enable_disable_debug_Click(object sender, RoutedEventArgs e)
        {
            
           
        }

        private void pull()
        {
            PushPullFiles pushPull = new PushPullFiles();
            Utilities util = new Utilities();
            try
            {
                util.Check_devices();
                pushPull.Pull(true);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

        }

        private void pushHex(string[] results)
        {
            Utilities util = new Utilities();            
            Constants con = new Constants();

            PushPullFiles pushPull = new PushPullFiles();


            try
            {
                util.Check_devices();
                
                pushPull.PushHex(results, true);
            }
            catch (Exception ex)
            {
               // throw new Exception("Cant push files");
               
            }
        }

        private void pushCalib(string[] results)
        {
            Utilities util = new Utilities();
            Constants con = new Constants();

            PushPullFiles pushPull = new PushPullFiles();


            try
            {
                util.Check_devices();

                pushPull.PushCalib(results, true);
            }
            catch (Exception ex)
            {
                // throw new Exception("Cant push files");

            }
        }

        private void pullEditPush()
        {
        
            Utilities util = new Utilities();
            PushPullFiles pushPull = new PushPullFiles();
            try
            {
                util.Check_devices();
                string root = "adb root";
                util.proc(root, true);

                Thread.Sleep(1000);
                string remount = "adb remount";
                util.proc(remount, true);
                pushPull.pullEditPush(util);
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


        private void Check_Release_and_Engine()
        {
            try
            {
                util.Get_Release_and_Engine_Version();
                Thread.Sleep(1000);
                l_release_val.Content = Utilities.Release;

            }
            catch (Exception ex)
            {

                //throw ex;
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

        #region worker update the gui's values
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            var bc = new BrushConverter();
            while (true)
            {

                //(sender as BackgroundWorker).ReportProgress(Configuration.Instance.ProgressValue);

                this.Dispatcher.Invoke(() =>
                {
                    Progress_bar.Value = Utilities.Progress;

                });

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
                    // this.Dispatcher.Invoke(() =>
                    //{
                    //    l_serial_val.Content = Utilities.SerialNum;

                    //});

                    this.Dispatcher.Invoke(() =>
                    {
                        float a = Utilities.BatteryLevel / 1000000f;
                        l_battery.Content = string.Format("{0}", a);

                    });

                    this.Dispatcher.Invoke(() =>
                    {
                        l_battery.Background = (Brush)bc.ConvertFromString(Utilities.BackColor);

                    });

                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    l_release_val.Content = Utilities.Release;

                    //});

                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    l_engine_val.Content = Utilities.Engine;

                    //});

                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                   {
                       l_battery.Content = "N/A";
                   });

                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    l_release_val.Content = "N/A";

                    //});

                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    l_engine_val.Content = "N/A";

                    //});
                }

                Thread.Sleep(1000);
            }



        } 
        #endregion

        #region install APK
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

            if (dirs.Length < 1)
            {
                util.dc.ConsoleInput = "Path wasn't specified, to complete APK install, give an exist path";
                return;
            }

            //show all apks in the folder
            string[] filePaths = Directory.GetFiles(dirs[0], "*.apk",
                                         SearchOption.TopDirectoryOnly);


            string[] results = { };
            var items = new installedItems(filePaths.ToList());
            if ((bool)items.ShowDialog() == true)
            {
                results = System.IO.File.ReadAllLines(con.Get_ToInstall());

            }


            //wait till the list of items to be installed return


            var uiContext = SynchronizationContext.Current;
            if (results.Length > 0)
            {
                this.Dispatcher.Invoke((Action)delegate
                {

                    Task.Run(() => install.Install_APKs(results)).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

                });


                //var run = Task.Run(() => install.Install_APKs(results)).ContinueWith(failedTask => Console.WriteLine("APK is already installed"),
                //               TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
               
            }


        }
        
        #endregion

        #region Refresh button
        private void b_refresh_Click(object sender, RoutedEventArgs e)
        {
            if (!l_connected.Content.Equals("Connected"))
                return;

            Utilities util = new Utilities();
            Utilities util1 = new Utilities();
            Utilities util2 = new Utilities();
            Utilities util3 = new Utilities();
            var uiContext = SynchronizationContext.Current;

            this.Dispatcher.Invoke((Action)delegate
            {
                
                Task.Run(() => Check_Release_and_Engine()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
                
                Task.Run(() => util1.Check_devices()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
               
                Task.Run(() => util2.Check_Calib_Files()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
              
                Task.Run(() => util3.Check_Config_Files_Etc()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
               

            });

            Thread.Sleep(4000);
            l_calib_files_val.Content = String.Join(", ", Utilities.CalibrationFiles.ToArray());
            l_config_files_val.Content = string.Join(", ", Utilities.ConfigFiles.ToArray());
            l_serial_val.Content = Utilities.SerialNum;
            l_release_val.Content = Utilities.Release;
            l_engine_val.Content = Utilities.Engine;


        } 
        #endregion

        private void cb_enable_debug_Checked(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_enable_debug.IsChecked))           
                Utilities.EnableDebug = false;
           
            else
                Utilities.EnableDebug = true;

            var uiContext = SynchronizationContext.Current;
            this.Dispatcher.Invoke((Action)delegate
            {
               
                Task.Run(() => pullEditPush()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

            });
               
        }

        private void b_connect_Click(object sender, RoutedEventArgs e)
        {
            var uiContext = SynchronizationContext.Current;
            Utilities util = new Utilities();
            if (b_connect.Content.ToString().Equals("Connect"))
            {
                Utilities.Connect = true;
                b_connect.Content = "Disconnect";
            }
            else if (b_connect.Content.ToString().Equals("Disconnect"))
            {
                Utilities.Connect = false;
                b_connect.Content = "Connect";
            }

            this.Dispatcher.Invoke((Action)delegate
            {
                
                Task.Run(() => util.Adb_Connect_Disconnect()).ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));

            });
        }

        private void One_Shot_Install_Click(object sender, RoutedEventArgs e)
        {

            PushPullFiles pushPull = new PushPullFiles();
            var uiContext = SynchronizationContext.Current;
            
            List<string> apkFilePathsFiltered = new List<string>();
            List<string> imgFilePathsFiltered = new List<string>();
            List<string> calFilePathsFiltered = new List<string>();
            List<string> hexFilePathsFiltered = new List<string>();
            List<string> iniFilePathsFiltered = new List<string>();
                             

            Install install = new Install();
            install.One_shot_Install();
            

            var items = new installedItemsOneShot();
            items.ShowDialog();
            var files = File.ReadAllLines(con.Get_ToInstall());
            foreach (var item in files)
            {
                Console.WriteLine(System.IO.Path.GetExtension(item));
                if (System.IO.Path.GetExtension(item).Equals(".img"))
                    imgFilePathsFiltered.Add(item);

                if (System.IO.Path.GetExtension(item).Equals(".apk"))
                {
                    
                    apkFilePathsFiltered.Add(item);
                }
                if (System.IO.Path.GetExtension(item).Equals(".cal"))
                    calFilePathsFiltered.Add(item);
                if (System.IO.Path.GetExtension(item).Equals(".so"))
                    hexFilePathsFiltered.Add(item);
                if (System.IO.Path.GetExtension(item).Equals(".ini"))
                    iniFilePathsFiltered.Add(item);
            }

            //install images
            Utilities.Progress = 10;
            //if (imgFilePathsFiltered.Count > 0)
            //{
            //    var t2 = new Task(() => install.Install_APKs(imgFilePathsFiltered.ToArray()));
            //    var t1 = Task.Run(() => install.Enter_Boot_Loader(imgFilePathsFiltered.ToArray()));
            //    Utilities.Progress += 10 ;
            //    await t1.ContinueWith(task => uiContext.Send(x => logItems.Add(Utilities.TextToLog), null));
            //    //await t1.ContinueWith(t2.Start());
            
            //}
            this.Dispatcher.Invoke((Action)delegate
            {
                string pushHex= "";
                if (hexFilePathsFiltered.Count > 0)
                     pushHex = string.Format("adb push {0} /system/lib/rfsa/adsp", hexFilePathsFiltered[0]);
                //install images
                Utilities.Progress = 10;
                var backgroundScheduler = TaskScheduler.Default;
                var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();  
                //install img
                Task.Factory.StartNew(delegate { install.Enter_Boot_Loader(imgFilePathsFiltered.ToArray()); },
                         backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).

                //install apk
                ContinueWith(delegate { install.Install_APKs(apkFilePathsFiltered.ToArray()); },
                             backgroundScheduler).

                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).



                //copy Calibrations files
                ContinueWith(delegate { pushPull.PushCalib(calFilePathsFiltered.ToArray(), true); },
                             backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).

                ContinueWith(delegate { util.proc("adb root", true); },
                             backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).

                ContinueWith(delegate { util.proc("adb remount", true); },
                             backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).

                //wait after remount
                ContinueWith(delegate { Thread.Sleep(5000); },
                             backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler).

                //copy hexagon
                ContinueWith(delegate { util.proc(pushHex, true); },
                             backgroundScheduler).
                ContinueWith(delegate { uiContext.Send(x => logItems.Add(Utilities.TextToLog), null); }, uiScheduler);
                
                
                
         

                Utilities.Progress = 0;


               

            });

            
            //install apk
            
            
                    

        }                                          
                                                   
           

       
    }


//class for logging object to add to logItems collection
public class Item
{
    public string Text { get; set; }
}

}
