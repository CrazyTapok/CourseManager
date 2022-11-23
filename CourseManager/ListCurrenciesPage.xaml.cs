using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

namespace CourseManager
{
    /// <summary>
    /// Логика взаимодействия для ListCurrenciesPage.xaml
    /// </summary>
    public partial class ListCurrenciesPage : Page
    {
        private readonly MainWindow window;

        public List<Currency> currencies = new();
        public List<string> selectedCurrencies = new();
        public List<Currency> rates = new();

        public ListCurrenciesPage(MainWindow _window)
        {
            InitializeComponent();

            window = _window;

            Task getData = Task.Run(GetAllcurrencies);
            getData.Wait();


            currencyList.ItemsSource = currencies;
            startDate.SelectedDate = DateTime.Now;
            endDate.SelectedDate = DateTime.Now;
        }

        public async Task GetAllcurrencies()
        {
            using HttpClient client = new();
            currencies = await client.GetFromJsonAsync<List<Currency>>("https://www.nbrb.by/api/exrates/currencies");
        }


        private async void GetButton_Currencies(object sender, RoutedEventArgs e)
        {
            List<string> allDates = new();

            if (!DateTime.TryParse(startDate.Text, out var x) || !DateTime.TryParse(endDate.Text, out var y))
            {
                MessageBox.Show("Не указана дата!");
                return;
            }

            DateTime start = DateTime.Parse(startDate.Text);
            DateTime end = DateTime.Parse(endDate.Text);

            if (start > end)
            {
                MessageBox.Show("Убедитесь что указан корректный интервал!");
                return;
            }

            TimeSpan interval = end - start;
            IEnumerable<DateTime> dates = from shift in Enumerable.Range(0, interval.Days + 1)
                                          select start.AddDays(shift);

            foreach (DateTime item in dates)
            {
                allDates.Add(DateFormat(item.ToString()));
            }

            using HttpClient client = new();
            for (int i = 0; i < selectedCurrencies.Count; i++)
            {
                for (int j = 0; j < allDates.Count; j++)
                {

                    using var response = await client.GetAsync($"https://www.nbrb.by/api/exrates/rates/{selectedCurrencies[i]}?parammode=2&ondate={allDates[j]}");
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        rates.Add(await response.Content.ReadFromJsonAsync<Currency>());
                    }
                }
            }

            window.myFrame.Content = new DynamicsCourse(window, rates);
        }

        public static string DateFormat(string date)
        {
            string[] str = date.Split(new char[] { '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return $"{str[2]}-{str[0]}-{str[1]}";
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            CheckBox chBox = (CheckBox)sender;

            if (!selectedCurrencies.Contains((string)chBox.Tag))
                selectedCurrencies.Add((string)chBox.Tag);
            else
                selectedCurrencies.Remove((string)chBox.Tag);
        }


        private void SaveButton_Currencies(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new();
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "XML documents (.xlsx)|*.xlsx";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;

                XLWorkbook wb = new();
                IXLWorksheet sh = wb.Worksheets.Add("Валюты");

                sh.Cell(1, 1).SetValue("Переодичность установления");
                sh.Cell(1, 2).SetValue("Цифровой код");
                sh.Cell(1, 3).SetValue("Буквенный код");
                sh.Cell(1, 4).SetValue("Наименование на русском языке");
                sh.Cell(1, 5).SetValue("Наименование на английском языке");

                for (int i = 0; i < currencies.Count; i++)
                {
                    sh.Cell(i + 2, 1).SetValue(currencies[i].Cur_Periodicity);
                    sh.Cell(i + 2, 2).SetValue(currencies[i].Cur_Code);
                    sh.Cell(i + 2, 3).SetValue(currencies[i].Cur_Abbreviation);
                    sh.Cell(i + 2, 4).SetValue(currencies[i].Cur_Name);
                    sh.Cell(i + 2, 5).SetValue(currencies[i].Cur_Name_Eng);
                }

                wb.SaveAs(filename);
            }
        }
    }
}
