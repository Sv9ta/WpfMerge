using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Collections;

namespace WpfMerge
{


    public class Item
    {
        private string str;
        private string strWithoutSpace;
        private int status;     // 1- Match, 2- Change, 3- Add, 4- Dell

        public string Str { get { return str; } set { str = value; } }
        public string StrWithoutSpace { get { return strWithoutSpace; } set { strWithoutSpace = value; } }
        public int Status { get { return status; } set { status = value; } }

        public override bool Equals(object obj)
        {
            Item itm = obj as Item;

            if (itm == null || GetType() != obj.GetType())
                return false;

            if (this.Status != itm.Status)
                return false;

            if (!this.StrWithoutSpace.GetHashCode().Equals(itm.StrWithoutSpace.GetHashCode()))
                return false;

            return true;
        }
    }


    //-- Unit - блок строк кода, ограниченный изменением состояния строк.
    //-- состояния: 1- Match, 2- Change, 3- Add, 4- Dell
    public class Unit
    {
        private int num;
        private ArrayList list;
        private int status;     // 1- Match, 2- Change, 3- Add, 4- Dell

        public int Num { get { return num; } set { num = value; } }
        public ArrayList List { get { return list; } set { list = value; } }
        public int Status { get { return status; } set { status = value; } }

        //-----------------------------------------------
        public override bool Equals(object obj)
        {
            Unit unt = obj as Unit;

            if (unt == null || GetType() != obj.GetType())
                return false;

            if (this.Status != unt.Status)
                return false;

            if (this.List.Count != unt.List.Count)
                return false;

            for (int h = 0; h < unt.List.Count; h++)
            {
                if (!this.List[h].GetHashCode().Equals(unt.List[h].GetHashCode()))
                    return false;
            }
            return true;
        }

        //public override int GetHashCode()
        //{
        //   // return this.Status.GetHashCode() ^ this.Num.GetHashCode();
        //    return (Status != null) ? Status.GetHashCode() : 0;
        //}
    }


    static class Merge
    {
        //-- Считывает текст из файла и возвращает SortedList, в котором элементы = строки файла
        public static SortedList ReadFile(string TestFile)
        {
            SortedList slistF = new SortedList();
            bool isExists = File.Exists(TestFile) ? true : false;

            if (isExists)
            {
                try
                {
                    using (FileStream fs = new FileStream(TestFile, FileMode.Open, FileAccess.Read))
                    {
                        if (fs.Length != 0)
                        {
                            string s = "";
                            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                            {
                                for (int i = 0; !sr.EndOfStream; i++)
                                {
                                    s = sr.ReadLine();
                                    if (s.Trim(new Char[] { ' ', '\t' }).Length > 0)
                                        slistF.Add(i, s);
                                    else
                                        i--;
                                }
                            }
                        }
                    }
                }
                catch (FileNotFoundException ioEx)
                {
                    string ex = ioEx.Message;
                }
            }
            return slistF;
        }


        public static void UpdateFile(string updateText, string fileName)
        {
            try
            {
                StreamWriter textFile = new StreamWriter(fileName, false, System.Text.Encoding.Default);
                textFile.WriteLine(updateText);
                textFile.Close();
            }
            catch (Exception e)
            {
            }
        }


        //-- переводит структуру из блочной в построчную
        public static void StructuredFile(ref ArrayList listUnt)
        {
            ArrayList resList = new ArrayList();
            Item itm = null;
            int i = 0;
            int status = 0;
            foreach (Unit unt in listUnt)
            {
                status = unt.Status;
                foreach (string str in unt.List)
                {
                    itm = new Item();
                    itm.Status = status;
                    itm.Str = str;
                    itm.StrWithoutSpace = str.Trim(new Char[] { ' ', '\t' });
                    resList.Add(itm);
                }
            }
            listUnt = resList;
        }



        //-- Объединить измененные файлы
        public static ArrayList MergeALLFiles(SortedList slistF, ArrayList listUnt1, ArrayList listUnt2, ref bool isConflict)
        {
            //-- переводим структуру из блочной в построчную
            StructuredFile(ref listUnt1);
            StructuredFile(ref listUnt2);

            ArrayList resultList = new ArrayList();

            bool flag = true;
            int i = 0; // счетчик первого списка
            int j = 0; // счетчик второго списка

            Item itm0 = null;
            Item itm1 = null;
            Item itm2 = null;

            bool ok = false;
            bool statusSame = false;
            bool strSame = false;
            int status = 0; // 1- match, 2- Change, 3- Add, 4- Dell, ==== 5- Conflict

            while (flag)
            {
                itm0 = new Item();
                itm1 = null;
                itm2 = null;
                if (listUnt1.Count > i)
                    itm1 = (Item)listUnt1[i];
                if (listUnt2.Count > j)
                    itm2 = (Item)listUnt2[j];

                if (itm1 == null || itm2 == null)
                {
                    itm0.Status = 1;
                    itm0.Str = (itm1 == null) ? itm2.Str : itm1.Str;
                    i++;
                    j++;
                }
                else if (itm1.Equals(itm2))
                {
                    if (itm1.Status == 1 || itm1.Status == 2 || itm1.Status == 3)
                    {
                        itm0.Status = 1;
                        itm0.Str = itm1.Str;
                        itm0.StrWithoutSpace = itm1.StrWithoutSpace;
                    }
                    i++;
                    j++;
                }
                else
                {
                    statusSame = (itm1.Status == itm2.Status) ? true : false;
                    strSame = (itm1.StrWithoutSpace.GetHashCode().Equals(itm2.StrWithoutSpace.GetHashCode())) ? true : false;

                    if (itm1.Status == 2 && itm2.Status == 1) //-- Изменение в одном из файлов
                    {
                        itm0.Status = 1;
                        itm0.Str = itm1.Str;
                        itm0.StrWithoutSpace = itm1.StrWithoutSpace;
                    }
                    else if (itm1.Status == 1 && itm2.Status == 2) //-- Изменение в одном из файлов
                    {
                        itm0.Status = 1;
                        itm0.Str = itm2.Str;
                        itm0.StrWithoutSpace = itm2.StrWithoutSpace;
                    }
                    else if (itm1.Status == itm2.Status && itm1.Status == 2) //-- конфликт
                    {
                        itm0.Status = 5;
                        itm0.Str = itm1.Str + "\r\n<------->\r\n" + itm2.Str;
                        isConflict = true;
                    }
                    else if (itm1.Status == 3 && itm2.Status != 3) //-- добавление в одном из файлов
                    {
                        itm0.Status = 1;
                        itm0.Str = itm1.Str;
                        itm0.StrWithoutSpace = itm1.StrWithoutSpace;
                        j--;
                    }
                    else if (itm1.Status != 3 && itm2.Status == 3) //-- добавление в одном из файлов
                    {
                        itm0.Status = 1;
                        itm0.Str = itm2.Str;
                        itm0.StrWithoutSpace = itm2.StrWithoutSpace;
                        i--;
                    }
                    else if (itm1.Status != 3 && itm2.Status == 3) //-- добавление в обоих файлах
                    {
                        itm0.Status = 1;
                        itm0.Str = itm1.Str;
                        itm0.StrWithoutSpace = itm1.StrWithoutSpace;
                        resultList.Add(itm0);
                        itm0 = new Item();
                        itm0.Status = 1;
                        itm0.Str = itm2.Str;
                        itm0.StrWithoutSpace = itm2.StrWithoutSpace;
                    }
                    //else if (itm1.Status == 4 && itm2.Status == 4) //-- удаление в обоих файлах - ничего не делаем
                    //else if (itm1.Status == 4 && itm2.Status != 4 && itm2.Status != 2) //-- удаление в одном из файлов
                    //{
                    //    j--;
                    //}
                    //else if (itm2.Status == 4 && itm1.Status != 4 && itm1.Status != 2) //-- удаление в одном из файлов
                    //{
                    //    i--;
                    //}
                    else if (itm1.Status == 4 && itm2.Status == 2) //-- удаление в одном из файлов и изменение в другом - конфликт
                    {
                        itm0.Status = 5;
                        itm0.Str = itm2.Str;
                        isConflict = true;
                    }
                    else if (itm2.Status == 4 && itm1.Status == 2) //-- удаление в одном из файлов и изменение в другом - конфликт
                    {
                        itm0.Status = 5;
                        itm0.Str = itm1.Str;
                        isConflict = true;
                    }

                    i++;
                    j++;
                }
                //-- Проверка на конец
                if (listUnt1.Count <= i && listUnt2.Count <= j)
                    flag = false;

                if (itm0 != null && itm0.Str != null)
                    resultList.Add(itm0);
            }
            return resultList;
        }


        //-- Возвращает список блоков строк сверяемого файла с их статусами (ок, изменение, добавление, удаление)
        public static ArrayList MergeFile(SortedList slistF, SortedList slistF1)
        {
            bool flag = true;
            int o = 0; // счетчик нулевого(эталонного) списка
            int i = 0; // счетчик первого(сверяемого) списка
            int o_z = -1; // замершая итерация o
            int i_z = -1; // замершая итерация i
            int countZ = 0; // кол-во замерших итераций o
            int countZ1 = 0; // кол-во замерших итераций i

            ArrayList sl = new ArrayList();
            ArrayList sl1 = new ArrayList();
            ArrayList sl2 = new ArrayList();

            ArrayList listUnt = new ArrayList();

            Unit unt = null;

            while (flag)
            {
                bool ok1 = true;

                //-- сравниваем совпадают ли строки
                if (slistF.Count > o && slistF1.Count > i)
                {
                    if (slistF[o].ToString().Trim(new Char[] { ' ', '\t' }) != slistF1[i].ToString().Trim(new Char[] { ' ', '\t' }))
                        ok1 = false;
                }
                else
                    ok1 = false;

                //-- изменений не было
                if (ok1 && o_z == -1)
                {
                    if (sl.Count == 0)
                        unt = new Unit();
                    sl.Add(slistF1[i]);
                }

                if (ok1 && o_z != -1) //-- значит было изменение
                {
                    if (o_z == 0)
                        unt = new Unit();
                    o_z = -1;
                    i_z = -1;
                    countZ = 0;
                    countZ1 = 0;
                    if (sl1.Count != 0)
                    {
                        ArrayList sll = new ArrayList();
                        sll.AddRange(sl1);
                        unt.Num = 0;
                        unt.List = sll;
                        unt.Status = 2; //-- change
                        listUnt.Add(unt);
                        sl1.Clear();
                        sl2.Clear();
                        unt = new Unit();
                    }
                    sl.Add(slistF1[i]);
                }

                //-- добавление или изменение
                if (!ok1)
                {
                    if (sl.Count != 0)
                    {
                        //-- сохраняем блок строк, который накопился до изменения статуса
                        ArrayList sll = new ArrayList();
                        sll.AddRange(sl);
                        unt.Num = 0;
                        unt.List = sll;
                        unt.Status = 1; //-- match
                        listUnt.Add(unt);
                        sl.Clear();
                        unt = new Unit();
                    }

                    if (o_z == -1 || i_z == -1)
                    {
                        if (slistF.Count > o)
                        {
                            o_z = o;
                            countZ++;
                            sl2.Add(slistF[o]); //-- строка, которая не совпала из эталонного списка
                        }
                        if (slistF1.Count > i)
                        {
                            i_z = i;
                            countZ1++;
                            sl1.Add(slistF1[i]); //-- строка, которая не совпала из сверяемого списка
                        }
                    }
                    else
                    {
                        //-- Формируем из блока строк список для сравнения

                        //-- список для сравнения o (из эталонного файла)
                        ArrayList listToCompare = new ArrayList();
                        for (int c = 0; c < countZ; c++)
                            listToCompare.Add(slistF[o_z + c].ToString().Trim(new Char[] { ' ', '\t' }));

                        //-- список для сравнения i (из сравниваемого файла)
                        ArrayList listToCompare1 = new ArrayList();
                        for (int w = 0; w < countZ1; w++)
                            listToCompare1.Add(slistF1[i_z + w].ToString().Trim(new Char[] { ' ', '\t' }));

                        //-- была вставка
                        //if (slistF1.Count > i && slistF[o_z].ToString() == slistF1[i].ToString() && sl1.Count > 0) 
                        if (slistF1.Count > i && listToCompare.Contains(slistF1[i].ToString().Trim(new Char[] { ' ', '\t' })) && sl1.Count > 0) 
                        {
                            if (o_z == 0)
                                unt = new Unit();

                            int num = listToCompare.IndexOf(slistF1[i].ToString().Trim(new Char[] { ' ', '\t' }));
                            o_z += num;

                            //--  Здесь вопрос, какая строка является изменением, а какие добавлением
                            //-- Добавить сюда анализатор (Надо уточнить)

                            ArrayList listCh = new ArrayList();
                            if (num != 0) //-- значит было еще и изменение
                            {
                                listCh.AddRange(sl1.GetRange(0, num));

                                //-- изменение
                                unt.Num = 1;
                                unt.List = listCh;
                                unt.Status = 2; //-- change
                                listUnt.Add(unt);
                                unt = new Unit();

                                sl1.RemoveRange(0, num);
                            }

                            //-- сохраняем блок строк "Add" (вставка)
                            ArrayList sll = new ArrayList();
                            sll.AddRange(sl1);
                            unt.Num = 1;
                            unt.List = sll;
                            unt.Status = 3; //-- add
                            listUnt.Add(unt);
                            sl1.Clear();
                            unt = new Unit();

                            //-- возврат итератора эталонного списка
                            o = o_z;
                            o_z = -1;
                            i_z = -1;
                            sl2.Clear();
                            sl.Add(slistF1[i]);
                            countZ = 0;
                            countZ1 = 0;
                        }
                        //-- было удаление
                        //else if (slistF[o].ToString() == slistF1[i_z].ToString() && sl1.Count > 0) //-- было удаление
                        else if (slistF.Count > o && listToCompare1.Contains(slistF[o].ToString().Trim(new Char[] { ' ', '\t' })) && sl1.Count > 0) 
                        {
                            if (i_z == 0)
                                unt = new Unit();

                            int num = listToCompare1.IndexOf(slistF[o].ToString().Trim(new Char[] { ' ', '\t' }));
                            i_z += num;

                            ArrayList listCh = new ArrayList();
                            if (num != 0) //-- значит было еще и изменение
                            {
                                listCh.AddRange(sl1.GetRange(0, num));

                                //-- изменение
                                unt.Num = 1;
                                unt.List = listCh;
                                unt.Status = 2; //-- change
                                listUnt.Add(unt);
                                unt = new Unit();

                                sl2.RemoveRange(0, num);
                            }

                            ArrayList sll = new ArrayList();
                            sll.AddRange(sl2);
                            unt.Num = 1;
                            unt.List = sll;
                            unt.Status = 4; //-- dell
                            listUnt.Add(unt);
                            sl2.Clear();
                            unt = new Unit();
                            //-- возврат
                            i = i_z;
                            i_z = -1;
                            o_z = -1;
                            sl1.Clear();
                            sl.Add(slistF1[i]);
                            countZ = 0;
                            countZ1 = 0;
                        }
                        else
                        {
                            //if (slistF1.Count == i + 1 && sl1.Count > 0 && slistF.Count < o) //  до этого было измененение
                            //{
                            //    ArrayList sll = new ArrayList();
                            //    sll.AddRange(sl1);
                            //    unt.Num = 0;
                            //    unt.List = sll;
                            //    unt.Status = 2;
                            //    listUnt.Add(unt);
                            //    sl1.Clear();
                            //    unt = new Unit();
                            //    o_z = -1;
                            //}
                            if (slistF1.Count > i)
                            {
                                sl1.Add(slistF1[i]);
                                countZ1++;
                            }
                            if (slistF.Count > o)
                            {
                                sl2.Add(slistF[o]);
                                countZ++;
                            }
                        }
                    }
                }

                o++;
                i++;

                //-- проверка на конец
                if (slistF.Count <= o && slistF1.Count <= i)
                {
                    flag = false;
                    //-- сохраняем последний блок строк
                    if (sl.Count > 0)
                    {
                        ArrayList sll = new ArrayList();
                        sll.AddRange(sl);
                        unt.Num = 2;
                        unt.List = sll;
                        unt.Status = 1; //-- match
                        listUnt.Add(unt);
                        sl.Clear();
                    }
                    if (sl1.Count > 0)
                    {
                        int ready = 0;
                        int countCh = 0;
                        for (int z = 0; z < countZ; z++)
                            if (o_z + z <= slistF.Count - 1)
                                countCh++;

                        if (unt == null)
                            unt = new Unit();

                        ArrayList listCh = new ArrayList();
                        if (countCh != 0 && countCh < sl1.Count) //-- изменение
                        {
                            listCh.AddRange(sl1.GetRange(0, countCh));

                            unt.Num = 1;
                            unt.List = listCh;
                            unt.Status = 2; //-- change
                            listUnt.Add(unt);
                            unt = new Unit();

                            ready = countCh;
                            o_z += countCh;
                            sl1.RemoveRange(0, countCh);
                        }

                        ArrayList sll = new ArrayList();
                        sll.AddRange(sl1);
                        unt.Num = 2;
                        unt.List = sll;
                        if (o_z != -1 && o_z <= slistF.Count - 1)
                            unt.Status = 2; //-- change
                        else
                            unt.Status = 3; //-- add
                        listUnt.Add(unt);
                        unt = new Unit();

                        ready += sl1.Count;

                        if (sl2.Count > ready)
                            sl2.RemoveRange(0, ready);
                        else
                            sl2.Clear();
                        sl1.Clear();
                    }
                    if (sl2.Count > 0)
                    {
                        ArrayList sll = new ArrayList();
                        sll.AddRange(sl2);
                        unt.Num = 2;
                        unt.List = sll;
                        unt.Status = 4; //-- dell
                        listUnt.Add(unt);
                        sl2.Clear();
                    }

                }
            }

            return listUnt;
        }

    }
}
