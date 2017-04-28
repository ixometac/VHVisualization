using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;


namespace VHVisualisation
{
    /// <summary>
    /// A mean/cluster in the k-means algorithm.
    /// </summary>
    public class SMean
    {
        SPoint centroid;
        public SPoint Centroid
        {
            get
            {
                return centroid;
            }
        }

        public bool OK
        {
            get
            {
                return centroid.W > 0;
            }
        }

        /// <summary>
        /// I.e., the number of points in this cluster.
        /// </summary>
        public int Weigth
        {
            get
            {
                return (int)centroid.W;
            }
        }

        private List<SPoint> points;

        public SMean(SPoint seed)
        {
            centroid = new SPoint(seed, 1);
            points = new List<SPoint>();
        }

        public double Distance(SPoint p)
        {
            return centroid.Distance(p);
        }

        public double Distance(SMean c)
        {
            return c.centroid.Distance(centroid);
        }

        public double ColourDistance(SMean p)
        {
            return centroid.ColourDistance(p.centroid);
        }

        public double DistanceSquare(SPoint p)
        {
            return centroid.DistanceSquare(p);
        }
        public double DistanceSquare(SMean p)
        {
            return centroid.DistanceSquare(p.Centroid);
        }


        public double ColourDistanceSquare(SMean p)
        {
            return centroid.ColourDistanceSquare(p.centroid);
        }

        public void Update()
        {
            double xSum = 0, ySum = 0, LSum = 0, aSum = 0, bSum = 0;
            foreach (SPoint p in points)
            {
                xSum += p.X;
                ySum += p.Y;
                LSum += p.L;
                aSum += p.A;
                bSum += p.B;
            }
            centroid = new SPoint((int)(xSum / points.Count), (int)(ySum / points.Count), (int)(LSum / points.Count), (int)(aSum / points.Count), (int)(bSum / points.Count), points.Count);
        }

        public void Clear()
        {
            points.Clear();
        }

        public void AddPoint(SPoint p)
        {
            points.Add(p);
        }

        public void AddMean(SMean c)
        {
            points.AddRange(c.points);
            c.Drop();
        }

        /// <summary>
        /// Drops all the points in the mean and marks it for deletion.
        /// </summary>
        public void Drop()
        {
            Clear();
            centroid = new SPoint(centroid, 0);
        }


    }
}
