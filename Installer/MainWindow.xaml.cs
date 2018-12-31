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

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ConsoleContent dc = new ConsoleContent();
        public string Connection_Val { get; set; }
        public string Connection_Color { get; set; }
        public string Release_Val { get; set; }
        public string Serial_Val { get; set; }
        Utilities util = new Utilities();
        Constants con = new Constants();


        // public DataContext MyProperty { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Utilities.BackColor = con.Fail_Color;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();

            BackgroundWorker worker_check_connection = new BackgroundWorker();
            worker_check_connection.DoWork += worker_CheckConnection;
            worker_check_connection.RunWorkerAsync();

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



        void worker_CheckConnection(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    util.Check_devices();
                    Utilities.ConnectionVal = con.Connected();
                    Utilities.BackColor = con.Pass_Color;
                }
                catch (Exception ex)
                {
                    Utilities.ConnectionVal = con.Disconnected();
                    Utilities.BackColor = con.Fail_Color;
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
                  l_connected.Background = (Brush)bc.ConvertFromString(Utilities.BackColor);

               });
                this.Dispatcher.Invoke(() =>
               {
                   l_serial_val.Content = Utilities.SerialNum;

               });



                Thread.Sleep(1000);
            }



        }
    }
}
