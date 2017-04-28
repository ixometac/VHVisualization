using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VHVisualisation
{

    public class ImageMapGen
    {
        //VARIABLE CONSTANTS
        //Threshold for deleting images
        private double simThresh = 250;

        public int initSize = Properties.Settings.Default.InitGridSize;
        public int finalSize = Properties.Settings.Default.InitGridSize;
        private int maxRuns = Properties.Settings.Default.IMSwitchRuns;
        private int neighbourhoodSize = Properties.Settings.Default.NeighbouthoodSize;

        int K = 4;


        private bool gridReady = false;
        private bool withDeletion;

        private Frame[] sortedFrames;

        private Result[] Results;

        private Result[,] frameGrid;

        private double simSum;
        private double simSumF;

        private double displayEnergy;
        private double displayColorEnergy;
        Distances.MetricType usedMetric;

        public double[] simSumArr;
        public double[] featureSimSumArr;

        bool distsCalculated;
        public bool useFeatures;

        double[,] dists;
        double[,] featureDists;

        int mdi1 = 0;
        int mdi2 = 0;
        int li1 = 0;
        int li2 = 1;

        public double CurrentDisplayEnergy { get { return displayEnergy; } }
        public bool GridReady
        {
            get { return gridReady; }
            set { gridReady = value; }
        }

        public Frame[,] GetFrameGrid()
        {
            Frame[,] res = new Frame[initSize, initSize];
            
            for (int i = 0; i < initSize; i++)
            {
                for (int j = 0; j < initSize; j++)
                {
                    res[i, j] = frameGrid[i, j]?.F1;
                }
            }
            return res;
        }

        public Result[,] SetFrameGrid
        {
            set { frameGrid = value; }
        }

        public Frame GetFrameAtPos(int row, int col)
        {
            if (row < finalSize && col < finalSize)
            {
                return frameGrid[row, col].F1;
            }
            else
            {
                return null;
            }
        }

        public void SetFeatures()
        {
            double[,] temp = dists;
            dists = featureDists;
            featureDists = temp;

            useFeatures = !useFeatures;
        }

        public void SetFinalSize(int s)
        {
            finalSize = s;
        }
        public Result GetResultAtPos(int row, int col)
        {
            if (row < finalSize && col < finalSize)
            {
                return frameGrid[row, col];
            }
            else
            {
                return null;
            }
        }

        public ImageMapGen(Result[] results, bool initDel)
        {
            withDeletion = initDel;
            Results = results;


            Initialize();
        }

        private void Initialize()
        {
            useFeatures = true;
            gridReady = false;
            gridReady = false;
            simSum = 0;
            simSumF = 0;
            distsCalculated = false;
            frameGrid = new Result[initSize, initSize];
            int gridCount = Math.Min(Results.Length, initSize * initSize);

            //List<Clip> got = new List<Clip>();

            //BIGMAT
            int num = gridCount;
            dists = new double[num, num];
            featureDists = new double[num, num];

            mdi1 = 0;
            mdi2 = 0;

            sortedFrames = new Frame[num];
        }

        //Generating the result 16x16 grid by switching and deleting images based on their similarity
        //TIME CONSUMING...done in background
        //TODO workout in parallel
        public void GenImageMap(Distances.MetricType metric = 0, List<Frame> selectedFrames = null)
        {
            if (Results.Any())
            {

                int num = initSize * initSize;
                mdi1 = mdi2 = li1 = li2 = 0;

                //calculate the distance Matrix
                if (!distsCalculated)
                {
                    //if (selectedFrames == null || selectedFrames.Count == 0)
                        CalculateDistances(metric);
                    //else
                    //    CalculateDistances(Distances.MetricType.FeatureCosine);
                }

                simSumArr = new double[maxRuns/1000];
                featureSimSumArr = new double[maxRuns/1000];

                Random rand = new Random();

                if (selectedFrames != null && selectedFrames.Any())
                {
                    SortAccordingTo(selectedFrames);
                }
                else
                {
                    //kMeans
                    InitialClusterPositioning();
                }
                //DONE in calculating distances currently
                //calculateSimSums();

                //CalculateDists("SimpleColor");

                ///Image Pairs SWITCHING
                //Random rand = new Random();
                //if (selectedFrames == null || selectedFrames.Count == 0)
                    PerformPairsSwitching(rand, "sc");
                /*else
                {
                    CalculateDists("SimpleColor");
                    PerformPairsSwitching(rand, "f");
                }*/

                //Deletion of 3/4 images and squeezing the rest into 16x16 grid
                if (withDeletion)
                {
                    DeleteExcessFrames();
                }

                gridReady = true;
            }
        }

        Tuple<int, int, int>[] GetNeighbours(int row, int col)
        {
            Tuple<int, int, int>[] neighbours = new Tuple<int, int, int>[(int)Math.Pow(1 + neighbourhoodSize * 2, 2) - 1];
            int index = 0;

            int nr, nc;

            for (int i = -neighbourhoodSize; i <= neighbourhoodSize; i++)
            {
                nr = row + i;
                for (int j = -neighbourhoodSize; j <= neighbourhoodSize; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        nc = col + j;
                        if (nr >= 0 && nc >= 0 && nr < frameGrid.GetLength(0) && nc < frameGrid.GetLength(1))
                        {
                            if (frameGrid[nr, nc] != null)
                                neighbours[index] = new Tuple<int, int, int>(nr, nc, Math.Max(Math.Abs(nr - row), Math.Abs(nc - col))); //frameGrid[nr, nc];
                            else
                                neighbours[index] = null;
                        }
                        else
                        {
                            neighbours[index] = null;
                        }
                        index++;
                    }
                }
            }

            return neighbours;
        }

        Result[] GetDirectNeighbours(int row, int col)
        {
            Result[] neighbours = new Result[4];

            if (row + 1 < initSize)
                neighbours[0] = frameGrid[row + 1, col];
            if (row - 1 >= 0)
                neighbours[1] = frameGrid[row - 1, col];
            if (col + 1 < initSize)
                neighbours[2] = frameGrid[row, col + 1];
            if (col - 1 >= 0)
                neighbours[3] = frameGrid[row, col - 1];

            return neighbours;
        }

        public Tuple<double, double> GetNextSim(int row, int col)
        {
            double res = 0;
            double resF = 0;

            if (row + 1 < initSize)
            {
                res += dists[frameGrid[row + 1, col].IndHelp, frameGrid[row, col].IndHelp];
                resF += featureDists[frameGrid[row + 1, col].IndHelp, frameGrid[row, col].IndHelp];

                if (col - 1 >= 0)
                {
                    res += dists[frameGrid[row + 1, col - 1].IndHelp, frameGrid[row, col].IndHelp];
                    resF += featureDists[frameGrid[row + 1, col - 1].IndHelp, frameGrid[row, col].IndHelp];
                }
                if (col + 1 < initSize)
                {
                    res += dists[frameGrid[row + 1, col + 1].IndHelp, frameGrid[row, col].IndHelp];
                    resF += featureDists[frameGrid[row + 1, col + 1].IndHelp, frameGrid[row, col].IndHelp];
                }
            }
            if (col + 1 < initSize)
            {
                res += dists[frameGrid[row, col + 1].IndHelp, frameGrid[row, col].IndHelp];
                resF += featureDists[frameGrid[row, col + 1].IndHelp, frameGrid[row, col].IndHelp];
            }

            return new Tuple<double, double>(res, resF);
        }

        internal void SetInitSize(int initS)
        {
            initSize = initS;

            Initialize();
        }

        private void CalculateSimSums()
        {
            simSum = 0;
            simSumF = 0;
            for (int row = 0; row < initSize; row++)
            {
                for (int col = 0; col < initSize; col++)
                {
                    Tuple<double, double> sims = GetNextSim(row, col);
                    simSum += sims.Item1;
                    simSumF += sims.Item2;
                }
            }
        }

        private void CalculateEnergy()
        {
            displayEnergy = 0;
            displayColorEnergy = 0;

            for(int row = 0; row < initSize; row++)
            {
                for (int col = 0; col < initSize; col++)
                {
                    for (int row2 = row+1; row2 < initSize; row2++)
                    {
                        for (int col2 = col+1; col2 < initSize; col2++)
                        {
                            double normManhattanDist = ((row2-row) + (col2-col)) / (double)(2*initSize);
                            double shortDist = Math.Max((row2 - row), (col2 - col));

                            double pairE = dists[frameGrid[row, col].IndHelp, frameGrid[row2, col2].IndHelp];
                            double pairEF = featureDists[frameGrid[row, col].IndHelp, frameGrid[row2, col2].IndHelp];

                            //displayEnergy += Math.Abs((pairE / normManhattanDist));
                            displayEnergy += Math.Abs((pairE - normManhattanDist)) * Math.Log(pairE / normManhattanDist);
                            //displayEnergy += Math.Abs((pairE - normManhattanDist) / normManhattanDist);

                            //displayEnergy += Math.Abs((pairE - normManhattanDist)) * Math.Log(pairE / normManhattanDist);
                            displayColorEnergy += Math.Abs((pairEF - normManhattanDist)) * Math.Log(pairEF / normManhattanDist);
                        }
                    }
                }
            }
        }

        double NeighboursSimilarity(int r, int c, int fr, int fc, bool usef = false)
        {
            double totalSim = 0;

            //Frame[] neighbours = getDirectNeighbours(r, c);
            Tuple<int, int, int>[] neighbours = GetNeighbours(r, c);
            int count = 0;

            for (int i = 0; i < neighbours.Length; i++)
            {
                double sim = 0;
                if (neighbours[i] != null)
                {
                    count++;
                    if (usef)
                    {
                        sim = featureDists[frameGrid[neighbours[i].Item1, neighbours[i].Item2].IndHelp, frameGrid[fr, fc].IndHelp];
                    }
                    else
                    {
                        sim = dists[frameGrid[neighbours[i].Item1, neighbours[i].Item2].IndHelp, frameGrid[fr, fc].IndHelp];
                    }

                    sim /= neighbours[i].Item3;
                }
                totalSim += sim;
            }

            return totalSim;
        }

        //Calculate the distance matrix

        public void CalculateDistances(Distances.MetricType metricType)
        {
            usedMetric = metricType;
            int num = initSize * initSize;
            double maxDist = 0;
            simSum = 0;
            simSumF = 0;
            //int[] mDists = new int[16 * 2];

            for (int i = 0; i < Results.Count(); i++)
            {
                //Setting Index Helper value as a index pointer to the original Result Array
                Results[i].IndHelp = i;

                for (int j = i + 1; j < num && j < Results.Count(); j++)
                {
                    double fd = 0;
                    double d = 0;

                    switch (metricType)
                    {
                        case (Distances.MetricType.SignaturePMHD):
                            fd = Distances.PMHD(Results[i].F1, Results[j].F1);
                            break;
                        case (Distances.MetricType.SignatureL2SQFD):
                            fd = Distances.L2SQFD(Results[i].F1, Results[j].F1);
                            break;
                        case (Distances.MetricType.FeatureCosine):
                            d = Distances.FeatureCosine(Results[i].F1, Results[j].F1);
                            fd = Distances.SimpleColor(Results[i].F1, Results[j].F1);
                            break;
                        case (Distances.MetricType.ColorSimpleL2):
                            d = Distances.SimpleColor(Results[i].F1, Results[j].F1);
                            fd = Distances.FeatureCosine(Results[i].F1, Results[j].F1);
                            break;
                    }

                    //Debug.Assert(fd >= 0 && fd <= 1);
                    //if(Double.IsNaN(fd))
                    //Debug.Assert(!Double.IsNaN(fd));

                    simSumF += fd;
                    simSum += d;

                    dists[i, j] = d;
                    dists[j, i] = d;

                    featureDists[i, j] = fd;
                    featureDists[j, i] = fd;

                    if (d > maxDist)
                    {
                        maxDist = d;
                        li1 = mdi1;
                        li2 = mdi2;
                        mdi1 = i;
                        mdi2 = j;
                    }
                }
            }

            distsCalculated = true;
        }

        private Tuple<double, double> GetPairDiff(int row1, int col1, int row2, int col2)
        {
            //double frameDist = PMHD(fp1, fp2);
            double frameDist = dists[frameGrid[row1, col1].IndHelp, frameGrid[row2, col2].IndHelp];

            double totSim1 = NeighboursSimilarity(row1, col1, row1, col1);
            double totSim2 = NeighboursSimilarity(row2, col2, row2, col2);

            double chSim1 = NeighboursSimilarity(row1, col1, row2, col2);
            double chSim2 = NeighboursSimilarity(row2, col2, row1, col1);

            double totSim1f = NeighboursSimilarity(row1, col1, row1, col1, true);
            double totSim2f = NeighboursSimilarity(row2, col2, row2, col2, true);

            double chSim1f = NeighboursSimilarity(row1, col1, row2, col2, true);
            double chSim2f = NeighboursSimilarity(row2, col2, row1, col1, true);

            double diff = (totSim1 + totSim2) - (chSim1 + chSim2);
            double featureDiff = (totSim1f + totSim2f) - (chSim1f + chSim2f);

            return new Tuple<double, double>(diff, featureDiff);
        }

        public void SetDeletion(bool newState)
        {
            if (withDeletion && newState == false)
            {
                withDeletion = false;
            }
            if (!withDeletion && newState == true)
            {
                withDeletion = true;
            }
        }

        private void PerformPairsSwitching(Random r, string typeStr)
        {
            ///Image Pairs SWITCHING
            Random rand = r;
            int runs = 0;
            int changes = 0;

            while (runs < maxRuns)
            {
                //Random seed...each run different
                //TODO better seed (or generator)

                int row1 = rand.Next() % initSize;
                int row2 = rand.Next() % initSize;
                int col1 = rand.Next() % initSize;
                int col2 = rand.Next() % initSize;

                var diffsTuple = GetPairDiff(row1, col1, row2, col2);

                if (diffsTuple.Item1 > 0 && !frameGrid[row1, col1].Fixed && !frameGrid[row2, col2].Fixed)
                {
                    Result temp = frameGrid[row1, col1];
                    frameGrid[row1, col1] = frameGrid[row2, col2];
                    frameGrid[row2, col2] = temp;

                    simSum -= diffsTuple.Item1;
                    simSumF -= diffsTuple.Item2;

                    changes++;
                }

                if (runs % 1000 == 0)
                {
                    CalculateEnergy();

                    if (usedMetric == Distances.MetricType.ColorSimpleL2)
                    {
                        simSumArr[runs / 1000] = displayEnergy /1000;
                        featureSimSumArr[runs / 1000] = displayColorEnergy * 10;
                    } else
                    {
                        simSumArr[runs / 1000] = displayColorEnergy * 10;
                        featureSimSumArr[runs / 1000] = displayEnergy /1000;
                    }
                }

                runs++;
            }
        }
        private void SortAccordingTo(List<Frame> images)
        {
            List<Result> setReses = new List<Result>();

            for (int i = 0; i < Results.Count(); i++)
            {
                for (int j = i + 1; j < Results.Count(); j++)
                {
                    //Debug.Assert((!Results[i].name.SequenceEqual(Results[j].name)));
                }
            }

            foreach (var res in Results)
            {
                if (images.Any(im => im.VideoID == res.F1.VideoID && im.ID == res.F1.ID))
                {
                    res.Fixed = true;
                    res.Placed = true;
                    setReses.Add(res);
                    frameGrid[res.F1.XPos, res.F1.YPos] = res;
                }
                else
                {
                    res.Fixed = false;
                    res.Placed = false;
                }
            }
            Dictionary<Result, SortedList<double, Result>> sortedSets = new Dictionary<Result, SortedList<double, Result>>();
            foreach (var fram in setReses)
            {
                SortedList<double, Result> distList = new SortedList<double, Result>();
                sortedSets.Add(fram, distList);
                fram.ResArrIndex = 0;
            }
            foreach (var res in Results)
            {
                foreach (var fram in setReses)
                {
                    //TODO improve
                    //Conveniency helper so that I dont Add samve value twice
                    double dToAdd = dists[fram.IndHelp, res.IndHelp];
                    while (sortedSets[fram].ContainsKey(dToAdd))
                        dToAdd += 0.000001;
                    sortedSets[fram].Add(dToAdd, res);
                }
            }

            int totalSize = initSize * initSize;
            for (int i = 0; i < initSize; i++)
            {
                for (int j = 0; j < initSize; j++)
                {
                    if (frameGrid[i, j] != null && !frameGrid[i, j].Placed)
                        frameGrid[i, j] = null;
                }
            }
            int toPlace = 256 - setReses.Count;
            for (int range = 0; toPlace > 0 && range < initSize; range++)
            {
                for (int col = -range; col < range; col++)
                {
                    for (int row = -range; row < range; row++)
                    {
                        if (!(col == 0 && row == 0))
                        {
                            foreach (var res in setReses)
                            {
                                int realCol = col + res.F1.XPos;
                                int realRow = row + res.F1.YPos;
                                if (realCol >= 0 && realCol < initSize && realRow >= 0 && realRow < initSize &&
                                    frameGrid[realCol, realRow] == null)
                                {
                                    SortedList<double, Result> ff = sortedSets[res];
                                    while (res.ResArrIndex < ff.Count && ff.ElementAt(res.ResArrIndex).Value.Placed)
                                    {
                                        res.ResArrIndex++;
                                    }
                                    if (res.ResArrIndex < ff.Count)
                                    {
                                        Result rr = ff.ElementAt(res.ResArrIndex).Value;
                                        rr.Placed = true;
                                        frameGrid[realCol, realRow] = rr;
                                        toPlace--;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //TODO DELETE
            for (int i = 0; i < initSize; i++)
            {
                for (int j = 0; j < initSize; j++)
                {
                    if (frameGrid[i, j] == null)
                        frameGrid[i, j] = Results[0];
                    //Debug.Assert(frameGrid[i, j] != null);
                }
            }
        }
        public double GetSimSum()
        {
            return simSum;
        }
        public double GetSimSumF()
        {
            return simSumF;
        }

        private void InitialClusterPositioning()
        {
            //kMeans
            var rand = new Random();
            int num = initSize * initSize;

            bool running = true;
            int modal = (int)Math.Sqrt(K);
            int perSize = initSize / modal;
            int maxCl = num / K;

            Result[] means = new Result[K];
            means[0] = Results[mdi1];
            means[1] = Results[mdi2];
            means[2] = Results[li1];
            means[3] = Results[li2];

            int[] meanInds = new int[K];
            meanInds[0] = mdi1;
            meanInds[1] = mdi2;
            meanInds[2] = li1;
            meanInds[3] = li2;

            for (int i = 4; i < K; i++)
            {
                int ra = (rand.Next() % num);

                meanInds[i] = ra;
                means[i] = Results[ra];
            }

            SortedList[] clusters = new SortedList[K];
            int[] order = new int[K];
            for (int i = 0; i < K; i++)
            {
                clusters[i] = new SortedList();
                order[i] = 0;
            }


            while (running)
            {
                //Assigning to nearest cluster
                for (int i = 0; i < num && i < Results.Count(); i++)
                {
                    double minDist = double.MaxValue;
                    int cluster = 0;
                    double d = 0;
                    for (int j = 0; j < K; j++)
                    {
                        d = dists[i, meanInds[j]];
                        if (d < minDist && clusters[j].Count < maxCl)
                        {
                            minDist = d;
                            cluster = j;
                        }
                    }

                    while (clusters[cluster].Contains(d))
                    {
                        d += 0.001;
                    }

                    clusters[cluster].Add(d, Results[i]);
                }

                running = false;
            }

            int[] clusterPose = new int[K * 2];
            clusterPose[0] = clusterPose[1] = 4;
            clusterPose[2] = clusterPose[3] = 12;
            clusterPose[4] = 4; clusterPose[5] = 12;
            clusterPose[6] = 12; clusterPose[7] = 4;

            for (int i = 0; i < K; i++)
            {
                int cSize = clusters[i].Count;
                int cSide = (int)Math.Ceiling(Math.Sqrt(cSize));
                int empty = (cSide * cSide) - cSize;

                int cx = 0;
                int cy = 0;
                if (i > 0)
                {
                    cx = (i % modal) * perSize;
                    cy = (i / modal) * perSize;
                }

                //placing
                if (clusters[i].Count > 0)
                    frameGrid[cx, cy] = (Result)clusters[i].GetByIndex(0);

                int curx = 1;
                int cury = 1;
                int xoff = curx;
                int yoff = 0;
                bool yup = true;
                for (int j = 1; j < Math.Min(cSize, clusters[i].Count); j++)
                {
                    if (cx + xoff >= 0 && cx + xoff < initSize && cy + yoff >= 0 && cy + yoff < initSize)
                        frameGrid[cx + xoff, cy + yoff] = (Result)clusters[i].GetByIndex(j);

                    if (yup)
                    {
                        yoff++;
                        if (yoff == cury)
                        {
                            yoff = cury;
                            yup = false;
                        }
                    }
                    else
                    {
                        xoff--;
                        if (xoff < 0)
                        {
                            cury++;
                            curx++;
                            xoff = curx;
                            yoff = 0;
                            yup = true;
                        }
                    }
                }
            }
        }

        private void DeleteExcessFrames()
        {
            Result[,] res = new Result[finalSize, finalSize];

            int dels = 0;
            int diff = (initSize * initSize) - (finalSize * finalSize);
            double step = 0.1;
            Random rand = new Random();

            while (dels < diff)
            {
                int row = rand.Next() % initSize;
                int col = rand.Next() % initSize;
                while (frameGrid[row, col] == null)
                {
                    row = rand.Next() % initSize;
                    col = rand.Next() % initSize;
                }

                double sim = NeighboursSimilarity(row, col, row, col);
                if (sim < simThresh)
                {
                    frameGrid[row, col] = null;
                    dels++;
                }
                else
                {
                    simThresh += step;
                }
            }

            bool inRange = true;

            Queue<Tuple<int, int>> freeCoords = new Queue<Tuple<int, int>>();
            Queue<Result> rest = new Queue<Result>();

            for (int i = 0; i < initSize && inRange; i++)
            {
                for (int j = 0; j < initSize && inRange; j++)
                {
                    if (frameGrid[j, i] == null && i < finalSize && j < finalSize)
                    {
                        if (rest.Any())
                            res[j, i] = rest.Dequeue();
                        else
                            freeCoords.Enqueue(new Tuple<int, int>(j, i));
                    }
                    else if (frameGrid[j, i] != null)
                    {
                        Tuple<int, int> toSet;
                        if (freeCoords.Any())
                        {
                            toSet = freeCoords.Dequeue();
                            res[toSet.Item1, toSet.Item2] = frameGrid[j, i];
                            if (i < finalSize && j < finalSize)
                                freeCoords.Enqueue(new Tuple<int, int>(j, i));
                        }
                        else
                        {
                            rest.Enqueue(frameGrid[j, i]);
                        }
                    }
                }
            }
            frameGrid = res;
        }
    }
}
