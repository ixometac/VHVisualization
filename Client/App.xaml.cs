using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VHVisualisation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        ImageMapGen imageMapGen;
        BackgroundWorker imWorker;
        Result[] pictures;
        public ImageExtractor imExtractor;

        public VideoDB videoDB;

        public VideoDB VideoDatabase { get { return videoDB; } }

        MainWindow wnd;
        protected override void OnStartup(StartupEventArgs e)
        {
            //WPF initialization
            base.OnStartup(e);
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewKeyDownEvent, new KeyEventHandler(OnWindowKeyUp));


            //Video database initialization
            //videoDB = new VideoDB();
            //videoDB.ReadTrecVidVideos();

            //Console.ReadLine();
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the startup window
            wnd = new MainWindow();

            // Show the window
            wnd.Show();

            
        }
        public void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.O) && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {

                List<string> fList = new List<string>();

                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Multiselect = true
                };
                if (openFileDialog.ShowDialog() == true)
                    OpenFiles(openFileDialog.FileNames);

            }
        }
        private void OpenFiles(IEnumerable<string> files)
        {
            imExtractor = new ImageExtractor(files);

            Result[] reses = imExtractor.GetResArr();

            //Image<Bgr, byte> cvIm = new Image<Bgr, byte>(new System.Drawing.Bitmap(reses[0].F1.image));

            imageMapGen = new ImageMapGen(reses, false);

            RunIMGen(imExtractor.GetResArr());
        }
        private void RunIMGen(Result[] results)
        {
            if (imWorker != null)
                imWorker.CancelAsync();

            imageMapGen = new ImageMapGen(results, false);
            //simText[0].Text = "0";


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

            imageMapGen.GenImageMap(0);
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
            //Stall execution in case different view level is initializing
            /*while (!imageMap.Initialized)
            { }

            imageMap.ClearGrid();
            imageMap.SetGrid(imageMapGen.GetFrameGrid(), 0);
            simText[1].Text = imageMapGen.cSum.ToString();
            simText[2].Text = imageMapGen.getSimSum().ToString();

            imageMap.Update();
            Update();*/

            //ImageMap detailPage = new ImageMap(imageMapGen);
            //wnd.NavigationService.Navigate(detailPage);
        }
    }
}
