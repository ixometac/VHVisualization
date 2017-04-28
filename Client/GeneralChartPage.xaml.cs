using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace VHVisualisation
{
    /// <summary>
    /// Interaction logic for GeneralChartPage.xaml
    /// </summary>
    public partial class GeneralChartPage : Page
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> XFormatter { get; set; }

        public bool Exported { get; set; }
        public GeneralChartPage()
        {
            InitializeComponent();

            Exported = false;
        }
        public void Export(string fileName)
        {
            //var positionTransform = mainChart.TransformToAncestor(this);
            //var root = this.PointToScreen(new System.Windows.Point(0, 0))   ;


            //var root = PointToScreen(new System.Windows.Point(0, 0));

            Bitmap bmp = TakeCroppedScreenShot(0, 28, mainChart.ActualWidth, mainChart.ActualHeight);

            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
        }
        public Bitmap TakeCroppedScreenShot(double x, double y, double width, double height)
        {
            System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)x, (int)y, (int)width, (int)height);
            Bitmap bmp = new Bitmap(r.Width, r.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(r.Left, r.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp;
        }
        public void Generate(List<List<double>> values, List<string> labels)
        {
            SeriesCollection = new SeriesCollection();
            mainChart.Zoom = ZoomingOptions.Xy;
            

            Debug.Assert(values.Count == labels.Count);

            System.Windows.Media.Brush[] prepares = { System.Windows.Media.Brushes.AliceBlue, System.Windows.Media.Brushes.IndianRed, System.Windows.Media.Brushes.DarkOliveGreen, System.Windows.Media.Brushes.DarkSalmon };

            //for (int i = 0; i < values.Count; i++)
            //{
            int i = 0;
                SeriesCollection.Add(
                    new LineSeries
                    {
                        Title = labels[i],
                        Values = new ChartValues<double>(values[i]),
                        PointGeometry = null,
                        PointForeround = prepares[i % prepares.Length],
                        Fill = System.Windows.Media.Brushes.Transparent
                    });

            SeriesCollection.Add(
                    new LineSeries
                    {
                        Title = labels[1],
                        Values = new ChartValues<double>(values[1]),
                        PointGeometry = null,
                        PointForeround = prepares[1 % prepares.Length],
                        Fill = System.Windows.Media.Brushes.Transparent
                    });

            //}
            Labels = labels.ToArray();
            XFormatter = value => (value * 1000).ToString();

            DataContext = this;
        }
    }
}
