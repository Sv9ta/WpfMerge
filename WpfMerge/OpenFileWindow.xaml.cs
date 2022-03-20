using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using System.IO;

namespace WpfMerge
{
    /// <summary>
    /// Interaction logic for OpenFileWindow.xaml
    /// </summary>
    public partial class OpenFileWindow : Window
    {
        public string File0name = "";

        public OpenFileWindow()
        {
            InitializeComponent();
        }


        public event EventHandler ButtonClicked;
        public event EventHandler ActivatedOfw;
        public event EventHandler ClosingOfw;

        private void buttonOpenFile0_Click(object sender, RoutedEventArgs e)
        {
            WriteFileName(NameFile0tbx);
        }

        private void buttonOpenFile1_Click(object sender, RoutedEventArgs e)
        {
            WriteFileName(NameFile1tbx);
        }

        private void buttonOpenFile2_Click(object sender, RoutedEventArgs e)
        {
            WriteFileName(NameFile2tbx);
        }


        private void WriteFileName(TextBox tb)
        {
            tb.Clear();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt";

            if (ofd.ShowDialog() == true)
            {
                tb.AppendText(ofd.FileName);
            }
        }


        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            //--Проверка
            bool isOk = true;
            if (NameFile0tbx.Visibility == Visibility.Visible && NameFile0tbx.Text == "")
            {
                isOk = false;
                lblError0.Visibility = Visibility.Visible;
            }
            else
                lblError0.Visibility = Visibility.Collapsed;
            if (NameFile1tbx.Visibility == Visibility.Visible && NameFile1tbx.Text == "")
            {
                isOk = false;
                lblError1.Visibility = Visibility.Visible;
            }
            else
                lblError1.Visibility = Visibility.Collapsed;
            if (NameFile2tbx.Visibility == Visibility.Visible && NameFile2tbx.Text == "")
            {
                isOk = false;
                lblError2.Visibility = Visibility.Visible;
            }
            else
                lblError2.Visibility = Visibility.Collapsed;
            //--
            if (isOk)
            {
                OpenFileWindow1.Close();

                if (ButtonClicked != null)
                {
                    ButtonClicked(this, EventArgs.Empty);
                }
            }
        }

        private void OpenFileWindow1_Activated(object sender, EventArgs e)
        {
            if (ActivatedOfw != null)
            {
                ActivatedOfw(this, EventArgs.Empty);
            }
        }

        private void OpenFileWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ClosingOfw != null)
            {
                ClosingOfw(this, EventArgs.Empty);
            }
        }

       
    }
}
