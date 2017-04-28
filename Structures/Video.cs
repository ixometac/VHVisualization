using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    public class Video
    {
        private Frame[] frames;
        private static double loadThreshold = Properties.Settings.Default.LoadThreshold;
        int id;
        int frameLoadSkip;

        public Frame[] Frames { get { return frames; } }
        public int FrameCount { get { return frames.Length; } }
        public int ID { get { return id; } }

        public Video(int id, Frame[] frames)
        {
            this.id = id;
            this.frames = frames;
        }


        public Video(int id, List<float[]> features, System.Drawing.Bitmap[] thumbs)
        {
            this.id = id;
            int frameCount = features.Count;
            this.frameLoadSkip = Properties.Settings.Default.skipframes;
            List<Frame> toAdd = new List<Frame>();

            //toAdd.Add(new Frame(features[0], thumbs[0]));

            for (int frameIndex=0; frameIndex < frameCount; frameIndex+=frameLoadSkip)
            {
                //double d = Distances.FeatureL2(features[last], features[frameIndex]);
                //if (d > loadThreshold)
                //{
                    toAdd.Add(new Frame(frameIndex, frameIndex / frameLoadSkip, ID, features[frameIndex], thumbs[frameIndex]));
                    //last = frameIndex;
                //}
            }

            frames = toAdd.ToArray();
        }
    }
}
