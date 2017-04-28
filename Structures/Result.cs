using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    [Serializable]
    public class Result
    {
        public string name;

        Frame frame;
        string filePath;
        
        public int IndHelp { get; set; }
        public bool Fixed { get; set; }
        public Frame F1 { get { return frame; } set { frame = value; } }
        public bool Placed { get; set; }
        public int ResArrIndex { get; set; }

        public Result(Image im, string fp)
        {
            frame = new Frame(im);
            filePath = fp;
            Fixed = false;
            Placed = false;
        }
        public Result(Frame frame)
        {
            Fixed = false;
            Placed = false;
            this.frame = frame;

            name = frame.VideoID.ToString() + "-" + frame.ID.ToString();
        }
    }
}