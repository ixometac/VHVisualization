using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    class ChartGenerator
    {
        ImageMapGen imageMapGen;

        Bitmap resultGrid;
        Bitmap correctImage;

        ChartWindow chartWindow;
        List<Frame> selectedFrames;

        string baseFileName;
        int trecVideBaseID;

        public ChartGenerator(ImageMapGen img, List<Frame> selected)
        {
            this.imageMapGen = img;
            baseFileName = "C:\\Users\\jan_000\\exportedScreens\\" + DateTime.Now.ToLongTimeString().Replace(" ", "_").Replace(":", "-");

            selectedFrames = selected;
            if(selected.Any())
            {
                correctImage = selected[0].Thumb;
            } else
            {
                correctImage = img.GetFrameAtPos(0, 0).Thumb;
            }

            CreateImageGrid(img.GetFrameGrid());
        }

        public void DisplayGraph()
        {
            List<double> sigs = new List<double>(imageMapGen.simSumArr);
            List<double> feats = new List<double>(imageMapGen.featureSimSumArr);
            List<List<double>> vals = new List<List<double>> { sigs, feats };
            List<string> labs = new List<string> { "Signatures", "Features" };

            chartWindow = new ChartWindow();
            chartWindow.Show();
            chartWindow.Generate(vals, labs);
        }

        public void SaveResults(double queryT, double IMT)
        {
            resultGrid.Save(baseFileName + "_grid.bmp");
            correctImage.Save(baseFileName + "_correct.bmp");

            //chartPage appends the sufix on its own
            chartWindow.SaveChart(baseFileName);

            string textPath = baseFileName + "_info.txt";
            if (true) //!File.Exists(textPath))
            {
                //File.Create(textPath);
                using (var tw = new StreamWriter(textPath))
                {
                    tw.WriteLine("ImageMap Video Hunter visualization query:\n");

                    tw.WriteLine("IM runs:" + Properties.Settings.Default.IMSwitchRuns + ", Neighbourhood: " + Properties.Settings.Default.NeighbouthoodSize + ", GridSize: " + Properties.Settings.Default.InitGridSize);
                    tw.WriteLine("Query time: " + queryT + ", IM gen time: " + IMT);

                    if(selectedFrames.Any())
                    {
                        tw.WriteLine("Selected Frames for the query:");
                        foreach(var f in selectedFrames)
                        {
                            tw.WriteLine("Vid: " + (f.VideoID + trecVideBaseID) + ", F: " + f.ID);
                        }
                    }
                    tw.WriteLine();
                    tw.WriteLine("Grid frames:");
                    for(int row=0; row < imageMapGen.finalSize; row++)
                    {
                        for(int col=0; col < imageMapGen.finalSize; col++)
                        {
                            tw.Write("V:" + imageMapGen.GetFrameAtPos(row, col).VideoID + "|F:" + imageMapGen.GetFrameAtPos(row, col).ID + " ;");
                        }
                        tw.WriteLine();
                    }

                    tw.Close();
                }
            }
        }
    

        public void CreateImageGrid(Frame[,] grid)
        {
            int imWidth = grid[0, 0].Thumb.Width;
            int imHeight = grid[0, 0].Thumb.Height;

            int totalWidth = grid.GetLength(0) * imWidth;
            int totalHeight = grid.GetLength(1) * imHeight;

            resultGrid = new Bitmap(totalWidth, totalHeight);

            using (Graphics graph = Graphics.FromImage(resultGrid))
            {
                for (int row = 0; row < grid.GetLength(0); row++)
                {
                    for (int col = 0; col < grid.GetLength(1); col++)
                    {
                        Point position = new Point(col * imWidth, row * imHeight);

                        graph.DrawImage(grid[col, row].Thumb, position);
                    }
                }
            }
        }

        public Bitmap GetImage()
        {
            return correctImage;
        }
        public Bitmap GetGridImage()
        {
            return resultGrid;
        }
        private Bitmap DrawFilledRectangle(int x, int y)
        {
            Bitmap bmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(Brushes.White, ImageSize);
            }
            return bmp;
        }
    }
}
