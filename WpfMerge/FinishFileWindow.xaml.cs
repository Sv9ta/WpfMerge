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
using System.IO;
using Microsoft.Win32;

namespace WpfMerge
{
    /// <summary>
    /// Interaction logic for FinishFileWindow.xaml
    /// </summary>
    public partial class FinishFileWindow : Window
    {
        public FinishFileWindow()
        {
            InitializeComponent();
        }

        public event EventHandler buttonSaveClicked;

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            //---- сохранение в отдельный файл. (Думаю лишнее)
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "Text Files (*.txt)|*.txt";
            //if (sfd.ShowDialog() == true)
            //{
            //    TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            //    using (FileStream fs = File.Create(sfd.FileName))
            //    {
            //        if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".txt")
            //        {
            //            doc.Save(fs, DataFormats.Text);
            //        }
            //        else
            //        {
            //            doc.Save(fs, DataFormats.Xaml);
            //        }
            //    }
            //}
            //---
            FinishFileWindow1.Close();
            if (buttonSaveClicked != null)
            {
                buttonSaveClicked(this, EventArgs.Empty);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            FinishFileWindow1.Close();
        }
    }
}
