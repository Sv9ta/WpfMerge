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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using Microsoft.Win32;

namespace WpfMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string fileName0 = "";
        public string fileName1 = "";
        public string fileName2 = "";
        public SortedList slistF; 
        public ArrayList listUnt1;
        public ArrayList listUnt2;
        public bool isSame = false;

        public MainWindow()
        {
            InitializeComponent();

            lblFileName0.Content = " ИСХОДНЫЙ ФАЙЛ: ";
        }



        private void ofw_ButtonClicked(object sender, EventArgs e)
        {
            if (((OpenFileWindow)sender).NameFile0tbx.Visibility == Visibility.Visible)
            {
                fileName0 = ((OpenFileWindow)sender).NameFile0tbx.Text;
                lblFileName0.Content += fileName0;
            }
            if (((OpenFileWindow)sender).NameFile1tbx.Visibility == Visibility.Visible && ((OpenFileWindow)sender).NameFile2tbx.Visibility == Visibility.Visible)
            {
                fileName1 = ((OpenFileWindow)sender).NameFile1tbx.Text;
                lblFileName1.Content += fileName1;
                //ReadText(fdsvFile1, fileName1);
            
                fileName2 = ((OpenFileWindow)sender).NameFile2tbx.Text;
                lblFileName2.Content += fileName2;
                //ReadText(fdsvFile2, fileName2);

                //-- сравнить файлы с исходным
                slistF = Merge.ReadFile(fileName0);
                SortedList slistF1 = Merge.ReadFile(fileName1);
                SortedList slistF2 = Merge.ReadFile(fileName2);

                listUnt1 = Merge.MergeFile(slistF, slistF1);
                listUnt2 = Merge.MergeFile(slistF, slistF2);

                //-- Отобразить файлы с изменениями на экран
                ShowChangeText(fdsvFile1, listUnt1);
                ShowChangeText(fdsvFile2, listUnt2);
            }
        }


        private void ofw_ActivatedOfw(object sender, EventArgs e)
        {
            this.IsEnabled = false;
        }


        private void ofw_ClosingOfw(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            this.Activate();
        }

        private void ffw_buttonSaveClicked(object sender, EventArgs e)
        {
            //-- обновить измененные 1 и 2 файлы
            TextRange doc = new TextRange(((FinishFileWindow)sender).rtb.Document.ContentStart, ((FinishFileWindow)sender).rtb.Document.ContentEnd);
            string updateStr = doc.Text;

            ShowChangeText(fdsvFile1, updateStr);
            ShowChangeText(fdsvFile2, updateStr);
            //-- Пересохранить
            Merge.UpdateFile(updateStr, fileName1);
            Merge.UpdateFile(updateStr, fileName2);

            isSame = true;
        }


        private void ReadText(FlowDocumentScrollViewer nametbx, string fileName)
        {
            TextRange doc = new TextRange(nametbx.Document.ContentStart, nametbx.Document.ContentEnd);
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                if (System.IO.Path.GetExtension(fileName).ToLower() == ".txt")
                {
                    //doc.Load(fs, DataFormats.Text);
                    //--для русской кодировки тоже
                    using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                    {
                        string text = sr.ReadToEnd();
                        var document = new FlowDocument();
                        var paragraph = new Paragraph();
                        paragraph.Inlines.Add(text);
                        document.Blocks.Add(paragraph);
                        nametbx.Document = document;
                    }
                }
            }
        }


        private void ShowChangeText(FlowDocumentScrollViewer nametbx, ArrayList listUnt)
        {
            TextRange doc = new TextRange(nametbx.Document.ContentStart, nametbx.Document.ContentEnd);
            var document = new FlowDocument();
            
            string test = "";
            foreach (Unit unt in listUnt)
            {
                var paragraph = new Paragraph();
                string pref = "";
                SolidColorBrush br = Brushes.Bisque;
                //SolidColorBrush br = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                //Color clr = Color.FromArgb(0, 0, 0, 0);
                switch (unt.Status)
                {
                    case 1:
                        pref = "";
                        br = Brushes.White;
                        //clr = Color.FromArgb(0, 221, 127, 127);
                        break;
                    case 2:
                        pref = "Изменено";
                        br = Brushes.Yellow;
                        //clr = Color.FromArgb(0, 0, 0, 0);
                        break;
                    case 3:
                        pref = "Добавлено";
                        br = Brushes.YellowGreen;
                        //clr = Color.FromArgb(0, 0, 0, 0);
                        break;
                    case 4:
                        pref = "Удалено";
                        br = Brushes.DarkOrange;
                        //clr = Color.FromArgb(0, 0, 0, 0);
                        break;
                }

                string listString = "";
                foreach (string str in unt.List)
                    listString += str + "\r\n";

                if(pref.Length > 0)
                    test += "=========" + pref + "=========\r\n" + listString + "\r\n";
                else
                    test += listString + "\r\n";

                //SolidColorBrush br1 = new SolidColorBrush(clr);

                paragraph.Background = br;
                paragraph.Inlines.Add(test);
                document.Blocks.Add(paragraph);
                document.Background = Brushes.White;
                test = "";
            }
            nametbx.Document = document;
        }

        private void ShowChangeText(RichTextBox rtb, ArrayList resultList)
        {
            string strToFile = "";
            string pref = "";
            int stat = 0;
            bool isChange = false;
            bool isFirst = true;

            TextRange doc = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            var document = new FlowDocument();
            var paragraph = new Paragraph();
            SolidColorBrush br = Brushes.White;

            foreach (Item itm in resultList)
            {
                if (stat != itm.Status)
                {
                    isChange = (stat == 0) ? false : true;
                    stat = itm.Status;
                }
                else
                    isChange = false;

                if (stat == 5 && (isChange || isFirst))
                {
                    if (strToFile.Length > 0)
                    {
                        paragraph.Background = br;
                        paragraph.Inlines.Add(strToFile);
                        document.Blocks.Add(paragraph);
                        paragraph = new Paragraph();
                        strToFile = "";
                    }
                    //---
                    pref = "//---- Конфликт -------------------------------------------------->>>>>>>>>>\r\n";
                    br = Brushes.YellowGreen;
                }
                else
                    if (stat != 5 && isChange)
                    {
                        if (strToFile.Length > 0)
                        {
                            paragraph.Background = br;
                            pref = "//---------------------------------------------------------------->>>>>>>>>>\r\n";
                            strToFile += pref;
                            paragraph.Inlines.Add(strToFile);
                            document.Blocks.Add(paragraph);
                            paragraph = new Paragraph();
                            strToFile = "";
                            pref = "";
                        }
                        //--
                        //pref = "//---------------------------------------------------------------->>>>>>>>>>\r\n";
                        br = Brushes.White;
                    }
                    

                strToFile += pref + itm.Str + "\r\n";

                pref = "";
                isFirst = false;
            }

            //--проверка на остаток
            if (strToFile.Length > 0)
            {
                paragraph.Background = br;
                paragraph.Inlines.Add(strToFile);
                document.Blocks.Add(paragraph);
            }
            document.Background = Brushes.White;
            rtb.Document = document;
        }


        private void ShowChangeText(FlowDocumentScrollViewer nametbx, string text)
        {
            TextRange doc = new TextRange(nametbx.Document.ContentStart, nametbx.Document.ContentEnd);
            var document = new FlowDocument();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(text);
            paragraph.Background = Brushes.White;
            document.Blocks.Add(paragraph);
            document.Background = Brushes.White;
            nametbx.Document = document;
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            OpenFileWindow ofw = new OpenFileWindow();
            ofw.ButtonClicked += ofw_ButtonClicked;
            ofw.ActivatedOfw += ofw_ActivatedOfw; 
            ofw.ClosingOfw += ofw_ClosingOfw; 
            ofw.lblOpenFile.Content = "Выберите исходный файл";
            ofw.Show();
        }

        private void buttonOpenFile1and2_Click(object sender, RoutedEventArgs e)
        {
            if (fileName0.Length > 0)
            {
                isSame = false;
                lblFileName1.Content = "";
                lblFileName2.Content = "";
                OpenFileWindow ofw = new OpenFileWindow();
                ofw.ButtonClicked += ofw_ButtonClicked;
                ofw.lblOpenFile.Content = "Выберите файлы для сравнения";
                ofw.NameFile0tbx.Visibility = Visibility.Collapsed;
                ofw.buttonOpenFile0.Visibility = Visibility.Collapsed;
                ofw.NameFile1tbx.Visibility = Visibility.Visible;
                ofw.buttonOpenFile1.Visibility = Visibility.Visible;
                ofw.NameFile2tbx.Visibility = Visibility.Visible;
                ofw.buttonOpenFile2.Visibility = Visibility.Visible;
                ofw.Show();
            }
            else
            { 
                //--подсказка
                popup1.IsOpen = true;
            }
        }

        private void buttonChangeFile0_Click(object sender, RoutedEventArgs e)
        {
            lblFileName0.Content = " ИСХОДНЫЙ ФАЙЛ: ";
            OpenFileWindow ofw = new OpenFileWindow();
            ofw.ButtonClicked += ofw_ButtonClicked;
            ofw.lblOpenFile.Content = "Выберите исходный файл";
            ofw.Show();
        }

        private void buttonMerge_Click(object sender, RoutedEventArgs e)
        {
            if (isSame)
            {
                MessageBox.Show("Файлы одинаковы");
            }
            else if (slistF != null && listUnt1 != null && listUnt2 != null)
            {

                FinishFileWindow ffw = new FinishFileWindow();
                ffw.buttonSaveClicked += ffw_buttonSaveClicked;

                //---Merge
                bool isConflict = false;
                ArrayList resultList = Merge.MergeALLFiles(slistF, listUnt1, listUnt2, ref isConflict);
                //-- Отобразить на экран
                ShowChangeText(ffw.rtb, resultList);
                if (isConflict)
                    ffw.lblFileStatus.Content = "В ходе слияния возникли конфликты";
                else
                    ffw.lblFileStatus.Content = "Слияние прошло успешно";

                ffw.Show();
            }
            else
            {
                //--подсказка
                popup2.IsOpen = true;
            }
            
        }


    }
}
