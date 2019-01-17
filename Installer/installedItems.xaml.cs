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
    public partial class installedItems : Window
    {
        Constants con = new Constants();
        List<string> output = new List<string>();
        List<CheckBoxListItem> items1 = new List<CheckBoxListItem>();
        public installedItems(List<string> appList)
        {
            InitializeComponent();

            File.Delete(con.Get_ToInstall());

            foreach (var item in appList)
            {

                items1.Add(new CheckBoxListItem(true, item));
            }
         
            
            lstExclude.ItemsSource = items1;

        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var item = cb.DataContext;
            lstExclude.SelectedItem = item;
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

        private void b_done_list_Click(object sender, RoutedEventArgs e)
        {
            Constants con = new Constants();

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(con.Get_ToInstall()))
            {
                foreach (var line in items1)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file.
                    if (line.Checked == true)
                    {
                        file.WriteLine(line.Text);
                    }
                }
            }
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        public List<string> result
        {
            set
            {
                foreach (var i in items1)
                {
                    if (i.Checked == true)
                        output.Add(i.Text);

                }
            }
            get {

                
                return output; }
        }

        private void cb_all_Change(object sender, RoutedEventArgs e)
        {
            if (!Convert.ToBoolean(cb_all.IsChecked))
            {

                foreach (var i in items1)
                {
                    i.Checked = false;
                }
            }
            else if (Convert.ToBoolean(cb_all.IsChecked))
            {
                lstExclude.SelectAll();
                foreach (var i in items1)
                {
                    i.Checked = true;
                }

            }

            lstExclude.Items.Refresh();


        }
       
    }
}
