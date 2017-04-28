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
    /// Interaction logic for videoSlider.xaml
    /// </summary>
    public partial class VideoSlider : UserControl
    {
        int rowCount, columnCount, radius;
        bool videoFixed, sliderFixed, move;
        Video renderedVideo;
        int mainFrame;
        double imageHeight, imageWidth;

        Point lastPos;
        double slideDistance;
        int firstFrame, lastFrame;

        public VideoSlider()
        {
            InitializeComponent();

            rowCount = 1;
            columnCount = 16;
            radius = columnCount / 2;

            videoFixed = sliderFixed = move = false;
            slideDistance = 0;

            sliderGrid.MouseLeftButtonDown += (sender, e) => { move = true; lastPos = e.GetPosition(this); };
            sliderGrid.MouseLeftButtonUp += (sender, e) => { move = false; };
            sliderGrid.MouseLeave += (sender, e) => { move = false; };
            sliderGrid.MouseMove += (sender, e) =>
            {
                if (move)
                    moveGrid(sender, e);
            };
        }

        public void SetupGrid()
        {
            imageHeight = this.ActualHeight;
            imageWidth = imageHeight * (4d / 3d);
            Thickness margin = this.Margin;
            margin.Left = -imageWidth;
            margin.Right = -imageWidth;
            this.Margin = margin;
                
            columnCount = (int)Math.Ceiling(this.ActualWidth / imageWidth) + 2;
            radius = columnCount / 2;

            this.Width += 2*imageWidth;

            sliderGrid.SetNumberOfRows(rowCount);
            sliderGrid.SetNumberOfColumns(columnCount);
        }

        public void moveGrid(object sender, MouseEventArgs e)
        {
            Point actualMousePos = e.GetPosition(this);
            double xToMove = lastPos.X - actualMousePos.X;
            slideDistance -= xToMove;

            if (slideDistance < -imageWidth)
            {
                firstFrame++;
                if (firstFrame + columnCount > renderedVideo.FrameCount)
                {
                    firstFrame = renderedVideo.FrameCount - columnCount;
                    slideDistance += xToMove;
                }
                else
                {
                    slideDistance += imageWidth;

                    renderImages(renderedVideo, mainFrame, firstFrame);
                }
            }
            if (slideDistance > 0)
            {
                firstFrame--;
                if (firstFrame < 0)
                {
                    firstFrame = 0;
                    slideDistance += xToMove;
                }
                else
                {
                    slideDistance -= imageWidth;

                    renderImages(renderedVideo, mainFrame, firstFrame);
                }
            }

            TranslateTransform myTranslate = new TranslateTransform();
            myTranslate.X = slideDistance;

            sliderGrid.RenderTransform = myTranslate;

            lastPos = actualMousePos;
        }
        public void renderImages(Video video, int frameNum, int firstFrame = 0)
        {
            int frameCount = Math.Min(rowCount * columnCount, video.FrameCount);

            sliderGrid.Children.Clear();

            for (int j = 0; j < frameCount; j++)
            {
                Border border;
                Image image = new Image()
                {
                    Source = ImageMap.LoadBitmap(video.Frames[j + firstFrame].Thumb)
                };
                if (frameNum == j + firstFrame)
                {
                    border = new Border()
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(255, 255, 0, 128)),

                        Child = image
                    };
                }
                else
                {
                    border = new Border()
                    {
                        BorderThickness = new Thickness(0),
                        BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromScRgb(255, 255, 255, 255)),

                        Child = image
                    };
                }

                //border.MouseEnter += new MouseEventHandler(highLight);
                //border.MouseLeave += new MouseEventHandler(disLight);
                //border.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(selectImage);

                Grid.SetColumn(border, j);

                sliderGrid.Children.Add(border);
            }
        }
        public void DisplayVideo(Video video, int frameNum, bool fix = false)
        {
            if (!videoFixed || (videoFixed && fix))
            {
                renderedVideo = video;
                mainFrame = frameNum;
                //columnCount = video.FrameCount;
                sliderGrid.SetNumberOfColumns(columnCount);

                sliderGrid.Children.Clear();

                firstFrame = frameNum - radius;
                if (firstFrame < 0)
                    firstFrame = 0;
                if (firstFrame + columnCount > renderedVideo.FrameCount)
                    firstFrame = renderedVideo.FrameCount - columnCount;

                renderImages(video, frameNum, firstFrame);

                videoFixed = fix;
            }
        }

        public void Release()
        {
            videoFixed = false;
            sliderFixed = false;
        }
    }
}
