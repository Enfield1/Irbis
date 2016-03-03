using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace IrbisXML
{
    class Program
    {
        static void Main(string[] args)
        {

            //закидываем в массив данные из csv-файла с инвентарниками, штрих-кодами и местоположением
            string[,] arrExcel = arrayCsv("1.csv");

            //лист для сохранения всех строк для output-файла
            List<string> listInput = new List<string>();

            //добавляем в лист все строки из исходного файла Irbis
            using (StreamReader sr = new StreamReader("1.txt", System.Text.Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    listInput.Add(line);
                }
            }

            //стримрайтер для лог-файла открываем
            StreamWriter swLog = new StreamWriter("log.txt", false, System.Text.Encoding.Default);

            //счетчик для консоли, количество строк в ирбис-файле
            int countChange = 0;
            //бежим по листу со строками ирбис-файла
            for (int i = 0; i < listInput.Count-1; i++)
            {
                //здесь делаем индикатор выполнения
                Console.SetCursorPosition(0,1);
                int LICount = listInput.Count - 1;
                Console.Write("Обработано строк {0} из {1}", i, LICount);

                //при нахождении необходимой строки
                if (listInput[i].Substring(1, 3) == "910")
                {
                    bool flagConsole = false;
                    //записываем штрих-код в переменную
                    string tempSC = searchShtrikhCod(arrExcel, searchInventarnikRegexp(listInput[i]).Trim());
                    //проверяем на отсутствие в строке тега с штрих-кодом и наличие штрих-кода в файле csv 
                    if (!listInput[i].Contains("^H") & tempSC != "")
                    {
                        flagConsole = true;
                        //дописываем штрих-код
                        listInput[i] += "^H" + tempSC; ;
                    }
                    //аналогично для местоположения
                    string tempMestopol = searchMestopolozhenie(arrExcel, searchInventarnikRegexp(listInput[i].Trim()));
                    if (!listInput[i].Contains("^!") & tempMestopol != "")
                    {
                        flagConsole = true;
                        listInput[i] += "^!" + tempMestopol;
                    }
                    //записываем измененную строку в лог-файл, если одно из двух предыдущих условний было выполнено
                    if (flagConsole) { swLog.WriteLine(listInput[i]); countChange++; }
                    
                }
            }
            //закрываем лог
            swLog.Close();

            //стримридер для output-файла, записываем в него лист измененных строк
            using (StreamWriter sw = new StreamWriter("output.txt", false, System.Text.Encoding.Default))
            {
                foreach (var item in listInput)
                {
                    sw.WriteLine(item);
                }
                sw.Close();
            }

            Console.WriteLine("\nГотово");
            Console.WriteLine("Изменено {0} строк", countChange);
            Console.Read();
        }
        //парсим данные из csv в массив
        public static string [,] arrayCsv(string path)
        {
            List<string> listInput = new List<string>();
            //создаем лист и записываем в него все строки из csv
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    listInput.Add(line);
                }
            }

            //создаем массив и записываем в него рас-split-ченные данные
            string[,] result = new string[3,listInput.Count-1];

            for (int i = 0; i < listInput.Count-1; i++)
            {
                string[] temp = listInput[i].Split(';');
                result[0, i] = temp[0];
                result[1, i] = temp[1];
                result[2, i] = temp[2];
            }

            return result;
            
        }

        public static string searchMestopolozhenie(string[,] arrayExcel, string inventarnik) 
        {
            string result = "";

            for (int i = 0; i < arrayExcel.Length / 3; i++)
            {
                if (arrayExcel[0, i] == inventarnik)
                {
                    result = arrayExcel[2, i];
                    break;
                }
            }

            return result;
        }


        public static string searchShtrikhCod(string[,] arrayExcel, string inventarnik)
        {
            string result = "";

            for (int i = 0; i < arrayExcel.Length / 3; i++)
            {
                if (arrayExcel[0, i] == inventarnik)
                {
                    result = arrayExcel[1, i];
                    break;
                }
            }

            return result;
        }

        public static string searchInventarnikRegexp(string input)
        {
            string result = "";
            Regex reg = new Regex(@"(?<=\^B).*?((?=\^)|$)", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(input);
            foreach (Match mat in mc)
            {
                result = mat.ToString();
            }


            return result;
        }

        public static string searchInventarnikStrF(string input)
        {
            string result = "";
            string temp;

            temp = input.Substring(input.IndexOf("^B")+2);

            //result = temp.Substring(0, temp.IndexOf("^"));
            result = temp.Substring(0, temp.IndexOfAny(new char[]{'^','\n'}));

            return result;
        }
        /*
        public static string[,] arrayExcel(string path, string nameOfBook)
        {
            FileStream tableXLSX = File.Open(path, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(tableXLSX);
            DataSet result = excelReader.AsDataSet();

            List<string> resultList = new List<string>();
            DataTable dt = result.Tables[nameOfBook];

            string[,] arrayNames = new string[2, dt.Rows.Count];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                arrayNames[0, i] = dt.Rows[i][0].ToString();
                arrayNames[1, i] = dt.Rows[i][1].ToString();
            }

            return arrayNames;
        }
         */
    }
}