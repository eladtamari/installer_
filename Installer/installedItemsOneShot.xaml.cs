using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Installer
{
    /// <summary>
    /// Interaction logic for installedItems.xaml
    /// </summary>
    public partial class installedItemsOneShot : Window
    {
        Constants con = new Constants();

        List<string> resultCal = new List<string>();
        List<string> resultImg = new List<string>();
        List<string> resultApk = new List<string>();
        List<string> resultIni = new List<string>();
        List<string> resultHex = new List<string>();
        List<CheckBoxListItem> imgItems = new List<CheckBoxListItem>();
        List<CheckBoxListItem> calItems = new List<CheckBoxListItem>();
        List<CheckBoxListItem> apkItems = new List<CheckBoxListItem>();
        List<CheckBoxListItem> iniItems = new List<CheckBoxListItem>();
        List<CheckBoxListItem> hexItems = new List<CheckBoxListItem>();

        #region properties results
        public List<string> ResultImg
        {
            set
            {
                foreach (var i in imgItems)
                {
                    if (i.Checked == true)
                        resultImg.Add(i.Text);
                }
            }
            get
            {
                return resultImg;
            }
        }

        public List<string> ResultHex
        {
            set
            {
                foreach (var i in hexItems)
                {
                    if (i.Checked == true)
                        resultHex.Add(i.Text);
                }
            }
            get
            {
                return resultHex;
            }
        }

        public List<string> ResultApk
        {
            set
            {
                foreach (var i in apkItems)
                {
                    if (i.Checked == true)
                        resultApk.Add(i.Text);
                }
            }
            get
            {
                return resultApk;
            }
        }

        public List<string> ResultIni
        {
            set
            {
                foreach (var i in iniItems)
                {
                    if (i.Checked == true)
                        resultIni.Add(i.Text);
                }
            }
            get
            {
                return resultIni;
            }
        }

        public List<string> ResultCal
        {
            set
            {
                foreach (var i in calItems)
                {
                    if (i.Checked == true)
                        resultCal.Add(i.Text);
                }
            }
            get
            {
                return resultCal;
            }
        }

        #endregion

        public installedItemsOneShot()
        {
            InitializeComponent();

            File.Delete(con.Get_ToInstall());

            foreach (var item in Install.imgFilePaths)

                
            {

                imgItems.Add(new CheckBoxListItem(true, item));
            }

            foreach (var item in Install.apkFilePaths)
            {

                apkItems.Add(new CheckBoxListItem(true, item));
            }

            foreach (var item in Install.iniFilePaths)
            {

                iniItems.Add(new CheckBoxListItem(false, item));
            }

            foreach (var item in Install.calFilePaths)
            {

                calItems.Add(new CheckBoxListItem(true, item));
            }

            foreach (var item in Install.hexFilePaths)
            {

                hexItems.Add(new CheckBoxListItem(true, item));
            }
         
            
            img_files_selector.ItemsSource = imgItems;
            ini_files_selector.ItemsSource = iniItems;
            cal_files_selector.ItemsSource = calItems;
            apk_files_selector.ItemsSource = apkItems;
            hex_files_selector.ItemsSource = hexItems;

        }
        private void CheckBoxImg_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            img_files_selector.SelectedItem = item;
        }

        private void CheckBoxApk_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            apk_files_selector.SelectedItem = item;
        }

        private void CheckBoxIni_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            ini_files_selector.SelectedItem = item;
        }

        private void CheckBoxCal_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            ini_files_selector.SelectedItem = item;
        }

        private void CheckBoxHex_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            hex_files_selector.SelectedItem = item;
        }

        

        private void b_Install_Click(object sender, RoutedEventArgs e)
        {
            Constants con = new Constants();
            if (File.Exists(con.Get_ToInstall()))
                File.Delete(con.Get_ToInstall());

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(con.Get_ToInstall()))
            {
                foreach (var line in apkItems)
                {
                    
                    if (line.Checked == true)                    
                        file.WriteLine(line.Text);                  
                }

                foreach (var line in imgItems)
                {
                    
                    if (line.Checked == true)
                        file.WriteLine(line.Text);
                }

                foreach (var line in calItems)
                {
                   
                    if (line.Checked == true)
                        file.WriteLine(line.Text);
                }
                foreach (var line in hexItems)
                {
                   
                    if (line.Checked == true)
                        file.WriteLine(line.Text);
                }

            }
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        
        private void cb_all_img_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all_image.IsChecked))
            {

                foreach (var i in imgItems)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all_image.IsChecked))
            {
                img_files_selector.SelectAll();
                foreach (var i in imgItems)
                {
                    i.Checked = true;
                }

            }

            img_files_selector.Items.Refresh();


        }


        private void cb_all_apk_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all_apk.IsChecked))
            {

                foreach (var i in apkItems)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all_apk.IsChecked))
            {
                apk_files_selector.SelectAll();
                foreach (var i in apkItems)
                {
                    i.Checked = true;
                }

            }

            apk_files_selector.Items.Refresh();


        }


        private void cb_all_ini_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all_ini.IsChecked))
            {

                foreach (var i in iniItems)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all_ini.IsChecked))
            {
                ini_files_selector.SelectAll();
                foreach (var i in iniItems)
                {
                    i.Checked = true;
                }

            }

            ini_files_selector.Items.Refresh();


        }

        private void cb_all_cal_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all_cal.IsChecked))
            {

                foreach (var i in calItems)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all_cal.IsChecked))
            {
                cal_files_selector.SelectAll();
                foreach (var i in calItems)
                {
                    i.Checked = true;
                }

            }

            cal_files_selector.Items.Refresh();


        }

        private void cb_all_hex_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all_hex.IsChecked))
            {

                foreach (var i in hexItems)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all_hex.IsChecked))
            {
                hex_files_selector.SelectAll();
                foreach (var i in hexItems)
                {
                    i.Checked = true;
                }

            }

            hex_files_selector.Items.Refresh();


        }
       
    }


    public class CheckBoxListItem
    {
        public bool Checked { get; set; }
        public string Text { get; set; }

        public CheckBoxListItem(bool ch, string text)
        {
            Checked = ch;
            Text = text;
        }
    }
}
