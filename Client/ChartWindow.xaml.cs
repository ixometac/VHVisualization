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

namespace VHVisualisation
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : NavigationWindow
    {
        public GeneralChartPage chart1;
        public ChartWindow()
        {
            InitializeComponent();

            chart1 = new GeneralChartPage();
            this.Navigate(chart1);
        }

        public void Generate(List<List<double>> values, List<string> labels)
        {
            chart1.Generate(values, labels);

            /*chart1.LayoutUpdated += (o, e) =>
            {
                if (!chart1.Exported && (chart1.ActualHeight > 0 || chart1.ActualWidth > 0) && chart1.IsInitialized)
                {
                    chart1.Exported = true;
                    Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(1000);

                        chart1.Export("C:\\Users\\jan_000\\exportedScreens\\" + DateTime.Now.ToLongTimeString().Replace(" ", "_").Replace(":", "-") + ".bmp");
                    });
                }
            };*/
        }
        public void SaveChart(string fileName)
        {
            chart1.LayoutUpdated += (o, e) =>
            {
                if (!chart1.Exported && (chart1.ActualHeight > 0 || chart1.ActualWidth > 0) && chart1.IsInitialized)
                {
                    chart1.Exported = true;
                    Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(1000);

                        chart1.Export(fileName + "_chart.bmp");
                    });
                }
            };

            //chart1.Export(fileName + "_chart.bmp");
        }
    }
}