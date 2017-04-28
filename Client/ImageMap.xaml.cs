using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
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
    /// Interaction logic for ImageMap.xaml
    /// </summary>
    public partial class ImageMap : UserControl
    {
        int imWidth, imHeight, initS;
        double realWidth, realHeight;
        public ImageMapGen imageMapGen;

        public List<Frame> selectedImages;
        public HashSet<Border> selectedBorders;

        public event MouseEventHandler MouseOverImageHandler;
        public event EventHandler MouseClickImage;
        //public event KeyEventHandler EnterPressedinIM;

        Distances.MetricType usedMetric;

        public bool IMInitialized;
        public ImageMap()
        {
            InitializeComponent();
            imHeight = imWidth = 16;
            selectedImages = new List<Frame>();
            selectedBorders = new HashSet<Border>();

            usedMetric = 0;
        }
        public void SetImages(ImageMapGen img)
        {
            this.imageMapGen = img;

            SetupGrid();
        }

        public void SetupGrid()
        {
            imGrid.Children.Clear();

            //realWidth = Math.Min(img.GetFrameAtPos(0, 0).Thumb.Width, imGrid.ActualWidth / imWidth);

            realWidth = imGrid.ActualWidth / imWidth;
            realHeight = imGrid.ActualHeight / imHeight;

            //Accomodate to the smaller of width / heigth
            realWidth = realWidth * (0.75) < realHeight ? realWidth : realHeight * (4d / 3d);
            realHeight = realWidth * 0.75;

            imGrid.SetNumberOfColumns(imWidth, realWidth);
            imGrid.SetNumberOfRows(imHeight, realHeight);

            Frame[,] results = imageMapGen.GetFrameGrid();

            List<Tuple<int, int>> l = new List<Tuple<int, int>>();

            foreach (var f in selectedImages)
            {
                l.Add(new Tuple<int, int>(f.XPos, f.YPos));
            }
            selectedImages = new List<Frame>();
            selectedBorders = new HashSet<Border>();

            for (int i = 0; i < imHeight; i++)
            {
                for (int j = 0; j < imWidth; j++)
                {
                    //System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    //image.Source = loadBitmap(new Bitmap(results[i, j].image));

                    Image image = new Image()
                    {
                        Source = LoadBitmap(results[i, j].Thumb)
                    };
                    Border border = new Border();

                    Grid.SetZIndex(border, 4);
                    Grid.SetZIndex(image, 1);

                    bool contains = false;
                    foreach (var t in l)
                    {
                        if (t.Item1 == i && t.Item2 == j)
                            contains = true;
                    }
                    if (contains)
                    {
                        border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(50, 50, 200, 0));
                        border.BorderThickness = new Thickness(2);
                        results[i, j].XPos = i;
                        results[i, j].YPos = j;
                        selectedImages.Add(results[i, j]);
                        selectedBorders.Add(border);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(0);
                        border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0));
                    }

                    border.Child = image;

                    border.MouseEnter += new MouseEventHandler(HighLight);
                    border.MouseLeave += new MouseEventHandler(DisLight);
                    border.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(SelectImage);

                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                    imGrid.Children.Add(border);
                }
            }

            if (this.Parent is HomePage hp)
                hp.progressLabel.Visibility = Visibility.Collapsed;

            //imGrid.ShowGridLines = true;
        }

        public void HighLight(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(100, 255, 0, 0));
            b.BorderThickness = new Thickness(1);

            Point pos = e.GetPosition(imGrid);
            int X = (int)Math.Floor(pos.X / b.ActualWidth);
            int Y = (int)Math.Floor(pos.Y / b.ActualHeight);
            Frame selected = imageMapGen.GetFrameAtPos(X, Y);
            MouseOverImageHandler.Invoke(selected, e);
        }
        public void DisLight(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            if (!selectedBorders.Contains(b))
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0));
                b.BorderThickness = new Thickness(0);
            }
            else
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(50, 50, 200, 0));
                b.BorderThickness = new Thickness(2);
            }
        }

        public void SelectImage(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;

            Point pos = e.GetPosition(imGrid);
            int X = (int)Math.Floor(pos.X / b.ActualWidth);
            int Y = (int)Math.Floor(pos.Y / b.ActualHeight);
            Frame selected = imageMapGen.GetFrameAtPos(X, Y);

            if (selectedBorders.Contains(b))
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0));
                b.BorderThickness = new Thickness(0);
                selectedImages.Remove(selected);
                selectedBorders.Remove(b);

                MouseClickImage.Invoke(selected, null);
            }
            else
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(50, 50, 200, 0));
                b.BorderThickness = new Thickness(2);
                selected.XPos = X;
                selected.YPos = Y;
                selectedImages.Add(selected);
                selectedBorders.Add(b);

                MouseClickImage.Invoke(selected, e);
            }
        }

        public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
        {
            BitmapSource bs = null;
            try
            {
                IntPtr ip = source.GetHbitmap();

                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch ( Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                //MessageBox.Show(e.Message);
                return null;
            }
            finally
            {

            }

            return bs;
        }
       
        public void Recalculate(object sender, RoutedEventArgs args)
        {
            //TODO redo
            //var hp = this.Parent as HomePage;
            //hp.progressLabel.Visibility = Visibility.Visible;

            bool succ = false;// Int32.TryParse(sizeBox.Text, out imWidth);
            if (succ)
            {
                imHeight = imWidth;
            }
            else
            {
                imWidth = imHeight;
            }
            int a = 0;
            //succ = Int32.TryParse(sizeBox.Text, out a);
            if (succ)
            {
                initS = a;
            }

            //img.setInitSize(initS);
            //img.setFinalSize(imWidth);

            if (imageMapGen.useFeatures)
                imageMapGen.SetFeatures();

            imageMapGen.GenImageMap(usedMetric);

            SetupGrid();
        }
        public void RecalculateFeatures(object sender, RoutedEventArgs args)
        {
            //TODO redo
            //var hp = this.Parent as HomePage;
            //hp.progressLabel.Visibility = Visibility.Visible;

            bool succ = false; //Int32.TryParse(sizeBox.Text, out imWidth);
            if (succ)
            {
                imHeight = imWidth;
            }
            else
            {
                imWidth = imHeight;
            }
            int a = 0;
            //succ = Int32.TryParse(sizeBox.Text, out a);
            if (succ)
            {
                initS = a;
            }

            //img.setFinalSize(imWidth);
            //img.setInitSize(initS);

            if (!imageMapGen.useFeatures)
                imageMapGen.SetFeatures();

            imageMapGen.GenImageMap(usedMetric, selectedImages);

            SetupGrid();
        }
        public void SearchSimilar(Result[] results, int metricIndex)
        {
            if (this.Parent is HomePage hp)
                hp.progressLabel.Visibility = Visibility.Visible;

            usedMetric = (Distances.MetricType)metricIndex;

            bool succ = false; //Int32.TryParse(sizeBox.Text, out imWidth);
            if (succ)
            {
                imHeight = imWidth;
            }
            else
            {
                imWidth = imHeight;
            }
            int a = 0;
            //succ = Int32.TryParse(sizeBox.Text, out a);
            if (succ)
            {
                initS = a;
            }

            //img.setFinalSize(imWidth);
            //img.setInitSize(initS);

            if (!imageMapGen.useFeatures)
                imageMapGen.SetFeatures();

            imageMapGen = new ImageMapGen(results, false);

            imageMapGen.GenImageMap(usedMetric, selectedImages);

            SetupGrid();
        }
    }
}