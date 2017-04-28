using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VHVisualisation
{
    /// <summary>
    /// The centroid for searching purposes.
    /// </summary>
    [Serializable]
    public class ISPoint
    {


        private Frame frame;
        private short l, a, b, x, y,r =0;

        public Frame Frame{
            get{ return frame; }
            set { frame = value; }
        }
        public short L
        {
            get { return l; }
            set { l = value; }
        }   
        public short A
        {
            get { return a; }
            set { a = value; }
        }
        public short B
        {
            get { return b; }
            set { b = value; }
        }
        public short X
        {
            get { return x; }
            set { x = value; }
        }
        public short Y
        {
            get { return y; }
            set { x = value; }
        }
        public short R
        {
            get { return r; }
            set { r = value; }
        }

        public ISPoint(short x_arg, short y_arg)
        {
            x = x_arg;
            y = y_arg;
            l = 0;
            a = 0;
            b = 0;
        }

        public ISPoint(short x_arg, short y_arg,short l_arg, short a_arg, short b_arg,short r_arg)
        {
            x = x_arg;
            y = y_arg;
            l = l_arg;
            a = a_arg;
            b = b_arg;
            r = r_arg;
        }


        public ISPoint(Frame frame_arg,SPoint pt){
            l = (short)pt.L;
            a = (short)pt.A;
            b = (short)pt.B;
            x = (short)pt.X;
            y = (short)pt.Y;
            r = (short)Math.Sqrt(pt.W / Math.PI);
            frame = frame_arg;
        }

        public double PositionDistance(System.Drawing.Point p)
        {
            return Math.Sqrt(
                (p.X - x) * (p.X - x) +
                (p.Y - y) * (p.Y - y));
        }
        public double PositionDistanceSquare(System.Drawing.Point p)
        {
            return 
                (p.X - x) * (p.X - x) +
                (p.Y - y) * (p.Y - y);
        }

        public double PositionDistance(ISPoint p)
        {
            return Math.Sqrt(
                (p.x - x) * (p.x - x) +
                (p.y - y) * (p.y - y));
        }

        public double LegacySearchDistance(ISPoint p)
        {
            return Math.Sqrt(
                (p.x - x) * (p.x - x) + 
                (p.y - y) * (p.y - y) +
                (p.l - l) * (p.l - l) +
                (p.a - a) * (p.a - a) +
                (p.b - b) * (p.b - b));
        }

        public double LegacySearchDistanceSquare(ISPoint p)
        {
            return 
                (p.x - x) * (p.x - x) +
                (p.y - y) * (p.y - y) +
                (p.l - l) * (p.l - l) +
                (p.a - a) * (p.a - a) +
                (p.b - b) * (p.b - b);
        }

        public double SearchDistance(ISPoint p)
        {
            return Math.Sqrt(
                Math.Max(0,(p.x - x) * (p.x - x) + (p.y - y) * (p.y - y) - r * r) +
                (p.l - l) * (p.l - l) +
                (p.a - a) * (p.a - a) +
                (p.b - b) * (p.b - b));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[").Append(x).Append("|").Append(y).Append("|").Append(l).Append("|").Append(a).Append("|").Append(b).Append("|").Append(Math.PI*r * r);
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            int hash = ToString().GetHashCode();
            return hash;
        }
    }
}
