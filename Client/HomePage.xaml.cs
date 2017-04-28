using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        TextBox sizeBox, isizeBox, vidFrameCount;
        int historyRow;
        int UIrowCount, UIcolumnCount;
        ComboBox distanceBox;
        Distances.MetricType usedMetric;

        public ImageMapGen imageMapGen;
        BackgroundWorker imWorker;
        Result[] results;
        public ImageExtractor imExtractor;

        public TextBox[] simText;
        public Label[] simLabs;
        public Label progressLabel;
        public Label IMgenLabel;
        Label videoIDLabel;
        Label frameIDLabel;

        Label searchTime;
        Label imTime;
        Label switchTime;

        public double lastQueryTime;
        public double lastIMGenTime;

        HashSet<Border> selectedBorders;
        List<Frame> selectedImages;

        List<List<Frame>> history;

        private VideoDB videoDB;

        ChartGenerator queryCharts;

        public void SetVideoDB(VideoDB vdb)
        {
            videoDB = vdb;
        }
        public HomePage(VideoDB vdb)
        {
            InitializeComponent();

            //Temporary
            //TODO
            //SemanticServer.Init();

            CreateUI();

            this.videoDB = vdb;
        }

        public HomePage()
        {
            InitializeComponent();

            imageMap.MouseOverImageHandler += MouseOverImage;
            imageMap.MouseClickImage += OnImageClick;
            //imageMap.EnterPressedinIM += SearchSimilar;
            //Temporary
            //TODO
            //SemanticServer.Init();
            historyRow = 0;
            historyGrid.SetNumberOfRows(20);
            history = new List<List<Frame>>();

            selectedImages = new List<Frame>();
            selectedBorders = new HashSet<Border>();

            UIrowCount = 14;
            UIcolumnCount = 2;
            usedMetric = Distances.MetricType.ColorSimpleL2;

            //CreateUI();
            //this.Loaded += (sender, args) => { SetupGUI(); };

            videoDB = new VideoDB();
            videoDB.videosLoaded += (sender, args) => { this.WindowTitle += " - Video DB Loaded"; };
           
            //videoDB.ReadTrecVidVideos();
        }

        public void SetupGUI()
        {
            videoSlider.SetupGrid();
            videoViewer.SetupGrid();

            CreateUI();
            vidFrameCount.Text = videoDB.maxFramesFromVideo.ToString();
        }

        public void CreateUI()
        {
            UIGrid.Width = this.ActualWidth * 0.2;
            UIGrid.Height = this.ActualHeight * 0.5;

            double UIwidth = (UIGrid.Width / 2) - 4;

            UIGrid.Children.Clear();

            UIGrid.SetNumberOfRows(UIrowCount);
            UIGrid.SetNumberOfColumns(UIcolumnCount);


            List<string> distOpts = new List<string> { "FeatureCosine", "FeatureL2", "FeatureL2Squared", "ColorSimpleL2", "SignaturePMHD", "SignatureL2SQFD" };
            distanceBox = new ComboBox();
            distanceBox.ItemsSource = distOpts;
            distanceBox.SelectedIndex = 0;
            Grid.SetColumn(distanceBox, 1);
            Grid.SetRow(distanceBox, 11);
            UIGrid.Children.Add(distanceBox);

            videoIDLabel = new Label()
            {
                Content = "Video ID: 0000",
                Width = UIwidth,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Grid.SetColumn(videoIDLabel, 0);
            Grid.SetRow(videoIDLabel, 0);
            UIGrid.Children.Add(videoIDLabel);

            frameIDLabel = new Label()
            {
                Content = "Frame ID: 1111",
                Width = UIwidth,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(frameIDLabel, 1);
            Grid.SetRow(frameIDLabel, 0);
            UIGrid.Children.Add(frameIDLabel);

            Button btn = new Button()
            {
                Content = "Recalculate signatues"
            };
            btn.Click += new RoutedEventHandler(imageMap.Recalculate);
            Grid.SetColumn(btn, 0);
            Grid.SetRow(btn, 1);
            UIGrid.Children.Add(btn);

            Button btnFeat = new Button()
            {
                Content = "Recalculate features"
            };
            btnFeat.Click += new RoutedEventHandler(imageMap.RecalculateFeatures);
            Grid.SetColumn(btnFeat, 0);
            Grid.SetRow(btnFeat, 2);
            UIGrid.Children.Add(btnFeat);

            Label sigSimL = new Label()
            {
                Content = "signature similarity sum:"
            };
            Grid.SetColumn(sigSimL, 0);
            Grid.SetRow(sigSimL, 3);
            UIGrid.Children.Add(sigSimL);

            Label featSimL = new Label()
            {
                Content = "feature similarity sum:"
            };
            Grid.SetColumn(featSimL, 0);
            Grid.SetRow(featSimL, 5);
            UIGrid.Children.Add(featSimL);

            TextBox tb = new TextBox()
            {
                Text = "0"
            };
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 4);
            UIGrid.Children.Add(tb);

            TextBox tb2 = new TextBox()
            {
                Text = "0"
            };
            Grid.SetColumn(tb2, 0);
            Grid.SetRow(tb2, 6);
            UIGrid.Children.Add(tb2);


            Button btnChart = new Button()
            {
                Content = "Chart"
            };
            btnChart.Click += new RoutedEventHandler(ShowChart);
            Grid.SetColumn(btnChart, 0);
            Grid.SetRow(btnChart, 8);
            UIGrid.Children.Add(btnChart);

            sizeBox = new TextBox()
            {
                Text = "0"
            };
            Grid.SetColumn(sizeBox, 0);
            Grid.SetRow(sizeBox, 10);
            UIGrid.Children.Add(sizeBox);

            Label sizeLab = new Label()
            {
                Content = "ImageMap final size:"
            };
            Grid.SetColumn(sizeLab, 0);
            Grid.SetRow(sizeLab, 9);
            UIGrid.Children.Add(sizeLab);

            isizeBox = new TextBox()
            {
                Text = "0"
            };
            Grid.SetColumn(isizeBox, 0);
            Grid.SetRow(isizeBox, 12);
            UIGrid.Children.Add(isizeBox);

            Label isizeLab = new Label()
            {
                Content = "IM initial size:"
            };
            Grid.SetColumn(isizeLab, 0);
            Grid.SetRow(isizeLab, 11);
            UIGrid.Children.Add(isizeLab);

            Button randomBtn = new Button()
            {
                Content = "Generate Random Frames"
            };
            randomBtn.Click += new RoutedEventHandler(GenerateRandomFrames);
            Grid.SetColumn(randomBtn, 1);
            Grid.SetRow(randomBtn, 1);
            UIGrid.Children.Add(randomBtn);

            Button searchBtn = new Button()
            {
                Content = "Search"
            };
            searchBtn.Click += new RoutedEventHandler(SearchSimilar);
            Grid.SetColumn(searchBtn, 1);
            Grid.SetRow(searchBtn, 2);
            UIGrid.Children.Add(searchBtn);

            progressLabel = new Label()
            {
                Content = "In Progress...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255))
            };
            Grid.SetColumn(progressLabel, 1);
            Grid.SetRow(progressLabel, 3);
            progressLabel.Visibility = Visibility.Collapsed;
            UIGrid.Children.Add(progressLabel);

            IMgenLabel = new Label()
            {
                Content = "Generating ImageMap...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)),
                Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(IMgenLabel, 1);
            Grid.SetRow(IMgenLabel, 4);
            UIGrid.Children.Add(IMgenLabel);

            vidFrameCount = new TextBox()
            {
                Text = "0"
            };
            Grid.SetColumn(vidFrameCount, 1);
            Grid.SetRow(vidFrameCount, 5);
            UIGrid.Children.Add(vidFrameCount);

            searchTime = new Label()
            {
                Content = "Search time: ",
                FontSize = 12,
                //Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)),
                //Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(searchTime, 1);
            Grid.SetRow(searchTime, 7);
            UIGrid.Children.Add(searchTime);

            imTime = new Label()
            {
                Content = "IM gen time: ",
                FontSize = 12,
                //Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)),
                //Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(imTime, 1);
            Grid.SetRow(imTime, 8);
            UIGrid.Children.Add(imTime);

            switchTime = new Label()
            {
                Content = "Switching time: ",
                FontSize = 12,
                //Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)),
                //Visibility = Visibility.Collapsed
            };
            Grid.SetColumn(switchTime, 1);
            Grid.SetRow(switchTime, 9);
            UIGrid.Children.Add(switchTime);

            foreach (var control in UIGrid.Children)
            {
                var c = control as Control;
                c.Margin = new Thickness(5, 1, 5, 1);
            }
        }

        public void ShowChart(object sender, RoutedEventArgs args)
        {
            queryCharts.DisplayGraph();

            queryCharts.SaveResults(lastQueryTime, lastIMGenTime);

            //chartWindow.chart1.Export(DateTime.Now.ToLongTimeString());
        }

        private void MouseOverImage (object sender, MouseEventArgs e)
        {
            Frame f = sender as Frame;
            if (f != null)
            {
                videoIDLabel.Content = "Video ID: " + f.VideoID.ToString();
                frameIDLabel.Content = "Frame ID: " + f.ID.ToString();

                videoViewer.DisplayVideo(videoDB.Videos[f.VideoID], f.ArrayID);
                videoSlider.DisplayVideo(videoDB.Videos[f.VideoID], f.ArrayID);
            }
        }
        private void OnImageClick(object sender, EventArgs e)
        {
            Frame f = sender as Frame;
            if (f != null)
            {
                videoIDLabel.Content = "Video ID: " + f.VideoID.ToString();
                frameIDLabel.Content = "Frame ID: " + f.ID.ToString();

                bool fix = true;

                if (e == null)
                {
                    videoViewer.Release();
                    videoSlider.Release();
                    fix = false;
                }
                videoViewer.DisplayVideo(videoDB.Videos[f.VideoID], f.ArrayID, fix);
                videoSlider.DisplayVideo(videoDB.Videos[f.VideoID], f.ArrayID, fix);
            }
        }

        public void GenerateRandomFrames(object sender, RoutedEventArgs e)
        {
            progressLabel.Visibility = Visibility.Visible;

            Button btn = sender as Button;
            btn.Background = new SolidColorBrush(Color.FromRgb(59, 99, 159));

            int resultsCount = 256;

            List<Frame> frameList = videoDB.GetRandomFrames(resultsCount, distanceBox.SelectedIndex);

            var searchResults = new List<Result>(resultsCount);
            foreach(var frame in frameList)
            {
                searchResults.Add(new Result(frame));
            }
            imageMap.selectedImages = new List<Frame>();
            imageMap.selectedBorders = new HashSet<Border>();

            RunimageMapGen(searchResults.ToArray());
        }

        public void AddHistory(List<Frame> selected)
        {
            Image[] images = new Image[selected.Count];
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = new Image()
                {
                    Source = ImageMap.LoadBitmap(imageMap.selectedImages[i].Thumb)
                };
            }

            history.Add(selected);

            for (int i = 0; i < images.Length; i++)
            {
                Border border = new Border()
                {
                    BorderThickness = new Thickness(0),
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0)),

                    Width = historyGrid.Width / images.Length
                };
                

                if (i==0)
                border.HorizontalAlignment = HorizontalAlignment.Left;
                if (images.Length > 2 && i == 1)
                    border.HorizontalAlignment = HorizontalAlignment.Center;
                if (images.Length <= 2 && i==1 || i == 2)
                    border.HorizontalAlignment = HorizontalAlignment.Right;

                border.Child = images[i];

                border.MouseEnter += new MouseEventHandler(HighLight);
                border.MouseLeave += new MouseEventHandler(DisLight);
                border.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(SelectImage);

                Grid.SetColumn(border, 0);
                Grid.SetRow(border, historyRow);
                
                historyGrid.Children.Add(border);
            }
            historyRow++;
        }

        public void SearchSimilar(object sender, RoutedEventArgs e)
        {
            progressLabel.Visibility = Visibility.Visible;

            bool succ = Int32.TryParse(vidFrameCount.Text, out int mffv);

            if (succ)
                videoDB.maxFramesFromVideo = mffv;

            Stopwatch searchWatch = new Stopwatch();

            searchWatch.Start();

            List<Result> results = videoDB.SearchSimilar(imageMap.selectedImages, (Distances.MetricType)distanceBox.SelectedIndex);

            searchWatch.Stop();
            lastQueryTime = searchWatch.ElapsedMilliseconds;
            searchTime.Content = String.Format("Search time: {0}", searchWatch.ElapsedMilliseconds);

            AddHistory(imageMap.selectedImages);
            videoViewer.Release();

            searchWatch.Restart();

            imageMap.SearchSimilar(results.ToArray(), distanceBox.SelectedIndex);

            searchWatch.Stop();

            lastIMGenTime = searchWatch.ElapsedMilliseconds;
            imTime.Content = String.Format("Search time: {0}", searchWatch.ElapsedMilliseconds);

            queryCharts = new ChartGenerator(imageMap.imageMapGen, imageMap.selectedImages);
        }

        public void HighLight(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(100, 255, 0, 0));
            b.BorderThickness = new Thickness(1);

            Point pos = e.GetPosition(historyGrid);
            int Y = (int)Math.Floor(pos.Y / (historyGrid.ActualHeight / historyGrid.RowDefinitions.Count()));
            int X = (int)Math.Floor(pos.X / Math.Floor(historyGrid.ActualWidth / history[Y].Count));
            if (history[Y].Count > X)
            {
                Frame selected = history[Y][X];
            }
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

            Point pos = e.GetPosition(historyGrid);
            int Y = (int)Math.Floor(pos.Y / (historyGrid.ActualHeight / historyGrid.RowDefinitions.Count()));
            int X = (int)Math.Floor(pos.X / (historyGrid.ActualWidth / history[Y].Count));
            Frame selected = history[Y][X];

            if (selectedBorders.Contains(b))
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0));
                b.BorderThickness = new Thickness(0);
                selectedImages.Remove(selected);
                selectedBorders.Remove(b);

            }
            else
            {
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(50, 50, 200, 0));
                b.BorderThickness = new Thickness(2);
                selected.XPos = X;
                selected.YPos = Y;
                selectedImages.Add(selected);
                selectedBorders.Add(b);
            }
        }

        private void RunimageMapGen(Result[] results)
        {
            IMgenLabel.Visibility = Visibility.Visible;

            if (imWorker != null)
                imWorker.CancelAsync();

            imageMapGen = new ImageMapGen(results, false);


            imWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            imWorker.DoWork +=
                new DoWorkEventHandler(ImWorker_DoWork);
            imWorker.ProgressChanged +=
                                        new ProgressChangedEventHandler(ImWorker_ProgressChanged);
            imWorker.RunWorkerCompleted +=
                                        new RunWorkerCompletedEventHandler(ImWorker_RunWorkerCompleted);

            imWorker.RunWorkerAsync();
        }

        private void ImWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            imageMapGen.GenImageMap(usedMetric);
        }
        public void SetText(TextBox tbo, string st)
        {
            tbo.Text = st;
        }
        private void ImWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string prog = (e.ProgressPercentage.ToString() + "%");
            System.Console.WriteLine(prog);
        }
        private void ImWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IMgenLabel.Visibility = Visibility.Collapsed;

            //Stall execution in case different view level is initializing
            //while (!imageMap.Initialized)
            //{ }

            imageMap.SetImages(imageMapGen);

            //ImageMap detailPage = new ImageMap(imageMapGen);
            //this.NavigationService.Navigate(detailPage);

            queryCharts = new ChartGenerator(imageMapGen, selectedImages);

            progressLabel.Visibility = Visibility.Collapsed;
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            RunimageMapGen(imExtractor.GetResArr());
        }
    }
}
