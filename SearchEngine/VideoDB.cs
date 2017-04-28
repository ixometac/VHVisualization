using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VHVisualisation
{
    public class VideoDB
    {
        Video[] videos;
        int videoCount, frameCount;
        public int maxFramesFromVideo;

        string thumbsFileName, featuresFileName;

        BitmapReader thumbsReader;
        FeatureReader featureReader;

        BackgroundWorker loadWorker;

        public event EventHandler videosLoaded;

        public Video[] Videos { get { return videos; } }
        public int VideoCount { get { return videos.Length; } }
        public int FrameCount { get { return videos.AsParallel().Select(v => v.FrameCount).Sum(); } }


        public VideoDB()
        {
            thumbsFileName = Properties.Settings.Default.ThumbnailsFileName;
            featuresFileName = Properties.Settings.Default.FeaturesFileName;
            maxFramesFromVideo = Properties.Settings.Default.MaxFramesPerVideo;

            LoadVideos();
        }

        public List<Frame> GetRandomFrames(int number, int metricIndex)
        {
            //ReadTrecVidVideos();

            HashSet<Frame> frameList = new HashSet<Frame>();

            Random randGen = new Random();

            for (int i = 0; i < number; i++)
            {
                int videoInd = randGen.Next(videoCount);
                int frameInd = randGen.Next(videos[videoInd].FrameCount);
                if (!frameList.Contains(videos[videoInd].Frames[frameInd]))
                {
                    frameList.Add(videos[videoInd].Frames[frameInd]);
                }
                else
                {
                    i--;
                }
            }

            return frameList.ToList<Frame>();
        }
        public void LoadVideos()
        {
            if (loadWorker != null)
                loadWorker.CancelAsync();

            loadWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            loadWorker.DoWork +=
                new DoWorkEventHandler(loadWorker_DoWork);
            loadWorker.ProgressChanged +=
                                        new ProgressChangedEventHandler(loadWorker_ProgressChanged);
            loadWorker.RunWorkerCompleted +=
                                        new RunWorkerCompletedEventHandler(loadWorker_RunWorkerCompleted);

            loadWorker.RunWorkerAsync();
        }

        private void loadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("Loaded");
            videosLoaded.Invoke(this, null);
        }

        private void loadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Trace.WriteLine(e.ProgressPercentage.ToString());
        }

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadTrecVidVideos();
        }

        public void ReadTrecVidVideos()
        {
            thumbsReader = new BitmapReader(thumbsFileName);
            featureReader = new FeatureReader(featuresFileName);

            videoCount = Math.Min(Properties.Settings.Default.videoCountToRead, featureReader.videoCount);
            //videoCount = featureReader.videoCount;

            videos = new Video[videoCount];

            for (int videoIndex = 0; videoIndex < videoCount; videoIndex++)
            {
                List<float[]> features = featureReader.ReadVideo(videoIndex);
                System.Drawing.Bitmap[] thumbnails = thumbsReader.ReadVideo(videoIndex);

                Video newVideo = new Video(videoIndex, features, thumbnails);

                videos[videoIndex] = newVideo;
            }

        }

        public List<Result> SearchSimilar(List<Frame> frames, Distances.MetricType metricIndex)
        {
            Frame toSearch;
            if (frames.Count == 1)
            {
                toSearch = new Frame(frames[0].feature, frames[0].ThumbSample);
            }
            else
            {
                float[] searchAverage = new float[4096];
                for (int j = 0; j < 4096; j++)
                {
                    searchAverage[j] = 0;
                    for (int i = 0; i < frames.Count; i++)
                    {
                        searchAverage[j] += frames[i].feature[j];
                    }
                    searchAverage[j] /= frames.Count;
                }

                toSearch = new Frame(searchAverage, frames[0].ThumbSample);
            }
            //TEST purpose only
            //toSearch = new Frame(frames[0].ThumbSample);

            SortedDictionary<double, Frame> queryResults = new SortedDictionary<double, Frame>();

            Frame last = videos[0].Frames[0];
            var minRect = new System.Drawing.Rectangle(0, 0, toSearch.ThumbSample.Width, toSearch.ThumbSample.Height);

            //Lock the bitman data of the example image only once,, to improve speed and ensure multi-threaded safety
            BitmapData toSearchData = toSearch.ThumbSample.LockBits(minRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            foreach (var video in videos)
            {
                //SortedDictionary<double, Frame> videoResults = new SortedDictionary<double, Frame>();
                foreach (var frame in video.Frames)
                {
                    //double dist = Distances.FeatureCosine(frame, toSearch);
                    //double dist = Distances.SimpleColor(frame, toSearch);
                    double dist = 0;

                    switch (metricIndex)
                    {
                        case (Distances.MetricType.SignaturePMHD):
                            dist = Distances.PMHD(frame, toSearch);
                            break;
                        case (Distances.MetricType.SignatureL2SQFD):
                            dist = Distances.L2SQFD(frame, toSearch);
                            break;
                        case (Distances.MetricType.FeatureCosine):
                            dist = Distances.FeatureCosine(frame, toSearch);
                            break;
                        case (Distances.MetricType.ColorSimpleL2):
                            dist = Distances.SimpleColorNonLockSecond(frame.ThumbSample, toSearch.ThumbSample, toSearchData);
                            //dist = Distances.SimpleColor(frame, new Frame(toSearch.ThumbSample));
                            break;
                    }


                    //videoResults.Add(dist, frame);
                    //while (queryResults.ContainsKey(dist))
                        //dist += 0.000001;
                    if(!queryResults.ContainsKey(dist))
                    queryResults.Add(dist, frame);

                    //double lastd = Distances.FeatureCosine(frame, last);

                    //TODO better solution
                    //Only to assure that we dont add 2 times same value
                    //if (!queryResults.ContainsKey(dist) && lastd > 0.33)
                    //{

                    //last = frame;
                    //}
                }
                //queryResults.Add(videoResults);
            }
            List<Result> results = new List<Result>();
            toSearch.ThumbSample.UnlockBits(toSearchData);

            int resultVideoCount = queryResults.Count;

            foreach (var f in frames)
            {
                results.Add(new Result(f));
            }

            int[] fromVideo = new int[VideoCount];

            for (int i = frames.Count; i < 256 && queryResults.Any(); i++)
            {
                /*int currentVideoId = i % resultVideoCount;
                while (!queryResults[currentVideoId].Any())
                {
                    currentVideoId = currentVideoId < 31 ? currentVideoId+1 : 0;
                }*/
                if (fromVideo[queryResults.First().Value.VideoID] < maxFramesFromVideo && !frames.Contains(queryResults.First().Value))
                {
                    fromVideo[queryResults.First().Value.VideoID]++;
                    var newRes = new Result(queryResults[queryResults.First().Key]);
                    results.Add(newRes);
                    queryResults.Remove(queryResults.First().Key);
                }
                else
                {
                    queryResults.Remove(queryResults.First().Key);
                    i--;
                }
            }

            return results;
        }
    }
}
