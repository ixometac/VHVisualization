using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    public static class KMeans
    {

        /// <summary>
        /// Assigns given points to the nearest mean.
        /// </summary>
        public static void AddToNearest(SPoint[] points, List<SMean> means)
        {
            foreach (SPoint p in points)
                means[GetNearestIndex(p, means)].AddPoint(p);
        }

        /// <summary>
        /// Returns index of the nearest mean for given point and list of means.
        /// This is the actual work :).
        /// </summary>
        private static int GetNearestIndex(SPoint point, List<SMean> means)
        {
            double minDistance = 0;
            int minIndex = -1;
            for (int i = 0; i < means.Count; i++)
            {
                double distanceSquare = means[i].DistanceSquare(point);
                if (minIndex == -1 || distanceSquare < minDistance)
                {
                    minDistance = distanceSquare;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Drops the empty means and clears the non-empty ones.
        /// </summary>
        public static void CleanMeans(List<SMean> means)
        {
            List<SMean> toReturn = new List<SMean>();
            foreach (SMean mean in means)
                if (mean.OK)
                {
                    mean.Clear();
                    toReturn.Add(mean);
                }
            means.Clear();
            means.AddRange(toReturn);
        }

        /// <summary>
        /// Deletes the small means.
        /// </summary>
        public static void DeleteSmallMeans(List<SMean> means, int treshold)
        {
            foreach (SMean mean in means)
                if (mean.Centroid.W < treshold)
                    mean.Drop();
        }

        /// <summary>
        /// Updates the means according to the last assigment.
        /// </summary>
        /// <param name="means"></param>
        public static void UpdateMeans(List<SMean> means)
        {
            foreach (SMean mean in means)
                if (mean.OK)
                    mean.Update();
        }

        /// <summary>
        /// Merges the means that are too close.
        /// </summary>
        public static void MergeMeans(List<SMean> means, double tresholdSquare, double colorTresholdSquare)
        {
            for (int i = 0; i < means.Count; i++)
            {
                if (!means[i].OK) continue;
                for (int j = i + 1; j < means.Count; j++)
                {
                    if (!means[j].OK) continue;
                    double distanceSquare = means[i].DistanceSquare(means[j]);
                    if (distanceSquare < tresholdSquare)
                    {
                        double colorDistaneSquare = means[i].ColourDistanceSquare(means[j]);
                        if (colorDistaneSquare < colorTresholdSquare)
                            means[j].AddMean(means[j]);
                    }
                }
            }
        }

    }
    [Serializable]
    public class Frame
    {
        int id;
        int arrayId;
        int videoId;
        private Bitmap thumbnail;
        private Bitmap downSizedThumb;
        public Image image;
        ISPoint[] signature;
        ISPoint mean;

        public double featureSize;
        public float[] feature;

        public int ID { get { return id; } }
        public int ArrayID { get { return arrayId; } }
        public int VideoID { get { return videoId; } }
        public Bitmap ThumbSample { get { return downSizedThumb; } }

        //TEMPORARY!!!
        Image<Lab, Byte> ImageLab;
        public int XPos { get; set; }
        public int YPos { get; set; }

        public ISPoint Mean
        {
            get { return mean; }
        }
        public Frame(Image im)
        {
            this.image = im;

            var bmp = new Image<Bgr, Byte>(new Bitmap(image));

            signature = ParseSignature(this);
            CalculateMean();

            feature = SemanticServer.GetFeatures(bmp).Item1;
        }
        public Frame(int id, int arrID, int videoId, float[] feature, System.Drawing.Bitmap thumb)
        {
            this.id = id;
            this.arrayId = (arrID);
            this.videoId = videoId;
            this.thumbnail = thumb;
            this.downSizedThumb = Distances.ResizeImage(thumb, 4, 3);
            this.feature = feature;

            featureSize = Distances.ComputeFeatureSize(feature);
            NormalizeFeature();

            image = thumbnail;
        }
        public Frame(float[] feature)
        {
            this.feature = feature;

            featureSize = Distances.ComputeFeatureSize(feature);
        }
        public Frame(float[] feature, Bitmap Thumb)
        {
            this.feature = feature;

            featureSize = Distances.ComputeFeatureSize(feature);

            this.downSizedThumb = Thumb;
        }
        public Frame(Bitmap Thumb)
        {
            this.downSizedThumb = new Bitmap(Thumb.Width, Thumb.Height);
            using (Graphics graphics = Graphics.FromImage(downSizedThumb))
            {
                Rectangle imageRectangle = new Rectangle(0, 0, downSizedThumb.Width, downSizedThumb.Height);
                graphics.DrawImage(Thumb, imageRectangle, imageRectangle, GraphicsUnit.Pixel);
            }

            this.downSizedThumb = Thumb;
        }

        public Bitmap Thumb { get { return thumbnail; } }

        public Image<Lab, byte> ImageLab1 { get => ImageLab; set => ImageLab = value; }

        public ISPoint[] Signature()
        {
            return signature;
        }
        private void NormalizeFeature()
        {
            Parallel.For(0, feature.Length, (index) => { feature[index] = feature[index] / (float)featureSize; });
        }

        public static ISPoint[] ParseSignature(Frame frame)
        {
            return ParseSignature(frame, 7, 27, 9, 17, 3, 1.1, 7, 3);
        }

        public static ISPoint[] ParseSignature(Frame frame, int iter, int colorMergeTreshold, int deleteTreshold, int mergeTreshold, int seedSpan, double deleteIterInc, int pos2l, int col2l)
        {
            #region Init
            SPoint.pos2l = pos2l;
            SPoint.col2l = col2l;
            Image<Lab, Byte> im = frame.ImageLab1;
            List<SPoint> pointsList = new List<SPoint>();
            List<SMean> means = new List<SMean>();
            for (int x = 0; x < im.Width; x++)
                for (int y = 0; y < im.Height; y++)
                    pointsList.Add(new SPoint(new Point(x, y), im));

            int start = seedSpan / 2;
            int xCount = (im.Width - start) / seedSpan;
            int yCount = (im.Height - start) / seedSpan;

            for (int x = 0; x < xCount; x++)
                for (int y = 1; y < yCount; y++)
                    means.Add(new SMean(new SPoint(new Point(start + x * seedSpan, start + y * seedSpan), im)));

            SPoint[] points = pointsList.ToArray<SPoint>();
            int mergeTresholdSquare = mergeTreshold * mergeTreshold;
            int colorMergeTresholdSquare = colorMergeTreshold * colorMergeTreshold;
            #endregion
            #region KMeans
            for (int it = 0; it < iter; it++)
            {
                KMeans.AddToNearest(points, means);
                KMeans.UpdateMeans(means);
                KMeans.MergeMeans(means, mergeTresholdSquare, colorMergeTresholdSquare);
                KMeans.DeleteSmallMeans(means, (int)(deleteTreshold * (deleteIterInc * (it + 1))));
                KMeans.UpdateMeans(means);
                KMeans.CleanMeans(means);
            }
            #endregion
            return means.Select(m => new ISPoint(null, m.Centroid)).ToArray();
        }
        public void CalculateMean()
        {
            int x=0, y=0, l=0, a=0, b=0, w=0;
            for(int i=0; i<signature.Length; i++)
            {
                x += signature[i].X;
                y += signature[i].Y;
                l += signature[i].L * signature[i].R;
                a += signature[i].A * signature[i].R;
                b += signature[i].B * signature[i].R;

                w += signature[i].R;
            }

            x /= signature.Length;
            y /= signature.Length;
            l /= w;
            a /= w;
            b /= w;

            mean = new ISPoint((short)x, (short)y, (short)l, (short)a, (short)b, (short)(w / signature.Length));
        }
    }
}
