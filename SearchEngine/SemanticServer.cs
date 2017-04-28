using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace VHVisualisation
{
    /// <summary>
    /// Class SemanticServer.
    /// Static class that manages extraction of semantic information from video key-frames.
    /// It runs a server-like c++ program in a separate process. The c++ program runs a DCNN model.
    /// </summary>
    static class SemanticServer
    {
        private static Process[] procs;
        private static StreamReader[] procOuts;
        private static StreamWriter[] procIns;
        private static bool ready;
        private static bool inited = false;
        public static bool Ready{get{ return ready; }}
        private static Random rand = new Random();
        private static int THREAD_COUNT = 1;

        private static Process CreateProcess()
        {
            //startup settings
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            proc.StartInfo.WorkingDirectory = dir;
            proc.StartInfo.FileName = dir + '\\' + "features.exe";

            //start & io
            proc.Start();

            return proc;
        } 

        public static void Init()
        {
            
            procs = new Process[THREAD_COUNT];
            procOuts = new StreamReader[THREAD_COUNT];
            procIns = new StreamWriter[THREAD_COUNT];
            for(int i=0;i< THREAD_COUNT;i++)
            {
                Process proc = CreateProcess();
                procs[i] = proc;
                procOuts[i] = proc.StandardOutput;
                procIns[i] = proc.StandardInput;
            }
            inited = true;
            ready = false;          
        }

        public static void WaitForReady()
        {
            if (!ready && procOuts != null)
            {
                ready = true;
                for (int i = 0; i < THREAD_COUNT; i++)
                {
                    StreamReader procOut = procOuts[i];
                    StreamWriter procIn = procIns[i];
                    String line = procOut.ReadLine(); // 1st row is human-readable info
                    line = procOut.ReadLine();
                    ready &= line == "READY";
                    procIn.WriteLine("B"); // receive both labels and features
                    procIn.Flush();
                }
            }
        }

        
        public static Tuple<float[],List<Tuple<double,String>>> GetFeatures(String filename, int thread)
        {
            if (!inited)
                Init();
            StreamWriter procIn = procIns[thread];
            StreamReader procOut = procOuts[thread];
            if (!ready) WaitForReady();
            procIn.WriteLine(filename);
            List<Tuple<double, string>> labels = new List<Tuple<double, string>>();
            for(int i=0;i<5;i++)
            {
                string labelLine = procOut.ReadLine();
                float prob = float.Parse(labelLine.Substring(0, 6), CultureInfo.InvariantCulture);
                string number = labelLine.Substring(labelLine.IndexOf("n") + 1, 8);
                labels.Add(new Tuple<double, string>(prob, number));
            }
            string line = procOut.ReadLine();
            float[] feature = line.Trim().Split(' ').Select(numStr => float.Parse(numStr, CultureInfo.InvariantCulture)).ToArray();
            return new Tuple<float[],List<Tuple<double,string>>>(feature, labels);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Tuple<float[], List<Tuple<double, String>>> GetFeatures(Image<Bgr,byte> image)
        {
            return GetFeatures(image, 0);
        }

        public static Tuple<float[], List<Tuple<double, String>>> GetFeatures(Image<Bgr, byte> image, int thread)
        {
            if (!ready) WaitForReady();
            string tmpName = thread+ "" + Process.GetCurrentProcess().Id + "tmp.jpg";
            image.Save(tmpName);
            var features = GetFeatures(tmpName, thread);
            if(File.Exists(tmpName))
                File.Delete(tmpName);
            return features;
        }

        public static void Terminate()
        {
            if(procs != null)
            {
                for(int i=0;i<THREAD_COUNT;i++)
                {
                    StreamWriter procIn = procIns[i];
                    StreamReader procOut = procOuts[i];
                    Process proc = procs[i];
                    procIn.WriteLine("EXIT");
                    proc.Close();
                    procIn.Close();
                    procOut.Close();
                }

            }
            procs = null;
            procOuts = null;
            procIns = null;
        }
    }
}