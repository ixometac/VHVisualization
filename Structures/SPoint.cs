using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.Drawing;

//using System.Windows.Input;

namespace VHVisualisation
{
    /// <summary>
    /// A 6-dimensional point (x,y, L,a,b and w) used in extraction algorithm.
    /// </summary>
    public class SPoint
    {
        public static int pos2l = 7;
        public static int col2l = 3;

        /// <summary>
        /// Spatial coordinates
        /// </summary>
        protected double x, y;

        /// <summary>
        /// The color in the CIE LAB color space (suitable for euclidean distance).
        /// </summary>
        protected double l, a, b;

        /// <summary>
        /// In the k-means clustering, this denotes the number of points in a particular mean.
        /// </summary>
        protected double w;

        public double X
        {
            get
            {
                return x;
            }
        }
        public double Y
        {
            get
            {
                return y;
            }
        }
        public double L
        {
            get
            {
                return l;
            }
        }
        public double A
        {
            get
            {
                return a;
            }
        }
        public double B
        {
            get
            {
                return b;
            }
        }
        public double W
        {
            get
            {
                return w;
            }
        }

        /// <summary>
        /// Creates a SPoint from a pixel at given position in the given image.
        /// </summary>
        /// <param name="p">The spatial coordinates.</param>
        /// <param name="labImage">The LAB image.</param>
        public SPoint(Point p, Image<Lab, Byte> labImage)
        {
            x = p.X;
            y = p.Y;
            l = labImage.Data[(int)y, (int)x, 0];
            a = labImage.Data[(int)y, (int)x, 1];
            b = labImage.Data[(int)y, (int)x, 2];
        }

        /// <summary>
        /// Creates a mean from the given point and weight.
        /// </summary>
        /// <param name="sp">The mean.</param>
        /// <param name="w_arg">The weight.</param>
        public SPoint(SPoint sp, double w_arg)
        {
            x = sp.x;
            y = sp.y;
            l = sp.l;
            a = sp.a;
            b = sp.b;
            w = w_arg;
        }

        public SPoint(double x_arg, double y_arg, double l_arg, double a_arg, double b_arg, double w_arg)
        {
            x = x_arg;
            y = y_arg;
            l = l_arg;
            a = a_arg;
            b = b_arg;
            w = w_arg;
        }

        public double Distance(SPoint p)
        {
            return Math.Sqrt(DistanceSquare(p));
        }

        public double DistanceSquare(SPoint p)
        {
            double xd = (p.x - x);
            double yd = (p.y - y);
            double ad = (p.a - a);
            double bd = (p.b - b);
            double ld = (p.l - l);
            return 
                (xd * xd + yd * yd) * pos2l +
                ld * ld +
                (ad * ad + bd * bd) * col2l;
        }

        public double ColourDistance(SPoint p)
        {
            return Math.Sqrt(ColourDistanceSquare(p));
        }

        public double ColourDistanceSquare(SPoint p)
        {
            return 
                (p.l - l) * (p.l - l) +
                ((p.a - a) * (p.a - a) +
                (p.b - b) * (p.b - b) )* 3;
        }

        public override String ToString()
        {
            StringBuilder toReturn = new StringBuilder();
            toReturn.Append("[");
            toReturn.Append(x);
            toReturn.Append("|");
            toReturn.Append(y);
            toReturn.Append("|");
            toReturn.Append(l);
            toReturn.Append("|");
            toReturn.Append(a);
            toReturn.Append("|");
            toReturn.Append(b);
            toReturn.Append("|");
            toReturn.Append(w);
            return toReturn.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is SPoint)
            {
                SPoint p = (SPoint)obj;
                return
                    x == p.x && y == p.y && l == p.l && a == p.a && b == p.b;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
