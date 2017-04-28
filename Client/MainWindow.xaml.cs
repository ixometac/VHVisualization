using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VHVisualisation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public VideoDB videoDB;
        public HomePage homePage;
        public MainWindow()
        {
            InitializeComponent();
            //this.Loaded += (sender, args) => { Dispatcher.BeginInvoke(new Action(() => Trace.WriteLine("DONE!", "Rendering")), DispatcherPriority.ContextIdle, null); };
            this.Loaded += (sender, args) => { Trace.WriteLine("DONE!", "Rendering"); homePage.SetupGUI(); };
            //this.ContentRendered += (sender, args) => { Trace.WriteLine("DONE!", "Rendering"); homePage.SetupGUI(); };
            this.SizeChanged += (sender, args) => {
                Trace.WriteLine("Resizing!");
                homePage.SetupGUI();
                if(homePage.imageMapGen != null && homePage.imageMapGen.GridReady)
                    homePage.imageMap.SetupGrid();
            };
            homePage = new HomePage();

            Content = homePage;
        }
        public MainWindow(VideoDB vdb)
        {
            this.videoDB = vdb;
            InitializeComponent();
            homePage = new HomePage();

            this.AddChild(homePage);
            //this.Navigate(homePage);
        }
    }
}
