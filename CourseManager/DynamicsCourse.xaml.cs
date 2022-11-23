using ClosedXML.Excel;
using ScottPlot;
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

namespace CourseManager
{
    /// <summary>
    /// Логика взаимодействия для DynamicsCourse.xaml
    /// </summary>
    public partial class DynamicsCourse : Page
    {
        private readonly MainWindow window;
        public List<Currency> rates = new();

        public DynamicsCourse(MainWindow _window, List<Currency> _rates)
        {
            InitializeComponent();
            window = _window;
            rates = _rates;

            listRates.ItemsSource = rates;
        }

        private void BackToList(object sender, RoutedEventArgs e)
        {
            window.myFrame.Content = new ListCurrenciesPage(window);
        }


        private void ShowCourseDynamics(object sender, RoutedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            Currency nameCurrency = (Currency)item.Content;

            List<Currency> currenciesCurrency = rates.Where(t => t.Cur_Abbreviation == nameCurrency.Cur_Abbreviation).ToList();
            double[] dataX = currenciesCurrency.Select(x => x.Date.ToOADate()).ToArray();
            double[] dataY = currenciesCurrency.Select(x => Convert.ToDouble(x.Cur_OfficialRate)).ToArray();

            WpfPlot wpfPlot = new();
            wpfPlot.Plot.AddScatter(dataX, dataY);
            wpfPlot.Plot.XAxis.DateTimeFormat(true);
            wpfPlot.Plot.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Day);
            wpfPlot.Plot.XAxis.TickLabelStyle(rotation: 45);
            wpfPlot.Refresh();

            ExchangeRateChart.Children.Add(wpfPlot);
        }


        private void SaveButton_Rates(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new();
            dialog.DefaultExt = ".xlsx";
            dialog.Filter = "XML documents (.xlsx)|*.xlsx";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;

                XLWorkbook wb = new();
                IXLWorksheet sh = wb.Worksheets.Add("Курсы валют");

                sh.Cell(1, 1).SetValue("Дата");
                sh.Cell(1, 2).SetValue("Буквенный код");
                sh.Cell(1, 3).SetValue("Наименование валюты");
                sh.Cell(1, 4).SetValue("Курс");

                for (int i = 0; i < rates.Count; i++)
                {
                    sh.Cell(i + 2, 1).SetValue(rates[i].Date);
                    sh.Cell(i + 2, 2).SetValue(rates[i].Cur_Abbreviation);
                    sh.Cell(i + 2, 3).SetValue(rates[i].Cur_Name);
                    sh.Cell(i + 2, 4).SetValue(rates[i].Cur_OfficialRate);
                }

                wb.SaveAs(filename);
            }
        }
    }
}
