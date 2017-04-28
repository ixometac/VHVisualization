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
    /// Interaction logic for VideoViewer.xaml
    /// </summary>
    public partial class VideoViewer : UserControl
    {
        int rowCount = 1;
        int columnCount = 1;
        int startFrame;
        bool isFixed;

        int cellWidth;
        int cellHeigth;
        public VideoViewer()
        {
            InitializeComponent();

            isFixed = false;
        }

        public void SetupGrid()
        {
            videoGrid.SetNumberOfRows(rowCount);
            videoGrid.SetNumberOfColumns(columnCount);

            cellWidth = (int)videoGrid.ActualWidth;
            cellHeigth = (int)(videoGrid.ActualWidth * 0.75);
         }

        public void DisplayVideo(Video video, int frameNum, bool fix = false)
        {
            if (!isFixed || (isFixed && fix))
            {
                videoGrid.Children.Clear();

                startFrame = frameNum;

                int frameCount = Math.Min(rowCount * columnCount, video.FrameCount);

                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount && (i * rowCount) + j < frameCount; j++)
                    {
                        System.Drawing.Bitmap src = Distances.ResizeImageHighQ(video.Frames[(i * rowCount) + j + startFrame].Thumb, cellWidth, cellHeigth);

                        Image image = new Image()
                        {
                            Source = ImageMap.LoadBitmap(src)
                        };
                        Border border = new Border()
                        {
                            BorderThickness = new Thickness(0),
                            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(0, 0, 0, 0)),
                            VerticalAlignment = VerticalAlignment.Top,

                            Child = image
                        };

                        //border.MouseEnter += new MouseEventHandler(highLight);
                        //border.MouseLeave += new MouseEventHandler(disLight);
                        //border.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(selectImage);

                        Grid.SetColumn(border, j);
                        Grid.SetRow(border, i);
                        videoGrid.Children.Add(border);
                    }
                }
                isFixed = fix;
            }
        }
        public void Release()
        {
            isFixed = false;
        }
    }
}
