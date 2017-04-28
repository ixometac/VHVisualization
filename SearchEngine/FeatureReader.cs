using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VHVisualisation
{
    public struct Features
    {
        public float[] features;
        public long id;
        public double distance;

        public Features(float[] features, long id, double distance)
        {
            this.features = features;
            this.id = id;
            this.distance = distance;
        }
    }
 

    public class FeatureReader : IDisposable
    {
        private BinaryReader reader;
        // private int headerSize = 16 * 3;
        public int featureDimension;
        public int featureCount;
        public int videoCount;
        public int[] videoOffsets;
        
        private long featureDataStartOffset;
        
        public FeatureReader(string filename)
        {
            try {
            reader = new BinaryReader(File.OpenRead(filename));
            }
            catch (IOException ex)
            {
                MessageBox.Show("File: " + filename + " could not be found. Rather shutting down.");
                Application.Current.Shutdown();
            }
            ReadHeader();
        }

        private void ReadHeader()
        {
            char[] header0 = reader.ReadChars(16);
            char[] header1 = reader.ReadChars(16);
            char[] header2 = reader.ReadChars(16);
            featureDimension = reader.ReadInt32();
            featureCount = reader.ReadInt32();
            videoCount = reader.ReadInt32();
            videoOffsets = new int[videoCount];

            for (int i = 0; i < videoCount; i++)
            {
                videoOffsets[i] = reader.ReadInt32();
            }

            int metadataSize = 3 * 16 + 3 * sizeof(int) + videoCount * sizeof(int);
            const int blockSize = 4096;
            featureDataStartOffset = ((metadataSize / blockSize) + 1) * blockSize;
        }

        public float[] GetFeature(long featureID)
        {
            reader.BaseStream.Seek(featureDataStartOffset + featureID * featureDimension * sizeof(float), SeekOrigin.Begin);
            byte[] bytes = reader.ReadBytes(featureDimension * sizeof(float));
            float[] floats = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            
            return floats;
        }

        public List<float[]> ReadVideo(int videoID)
        {
            int videoFrameCount;
            if (videoID != videoOffsets.Length - 1)
            {
                videoFrameCount = videoOffsets[videoID + 1] - videoOffsets[videoID];
            }
            else
            {
                videoFrameCount = featureCount - videoOffsets[videoID];
            }

            int featureSizeInBytes = featureDimension * sizeof(float);
            reader.BaseStream.Seek(featureDataStartOffset + (long)videoOffsets[videoID] * featureSizeInBytes, SeekOrigin.Begin);
            byte[] bytes = reader.ReadBytes(videoFrameCount * featureSizeInBytes);

            List<float[]> features = new List<float[]>();

            for (int i = 0; i < videoFrameCount; i++)
            {
                float[] floats = new float[featureDimension];
                Buffer.BlockCopy(bytes, i * featureSizeInBytes, floats, 0, featureSizeInBytes);
                features.Add(floats);
            }

            return features;
        }


        public List<float[]> ReadAll()
        {
            int featureSizeInBytes = featureDimension * sizeof(float);
            
            reader.BaseStream.Seek(featureDataStartOffset, SeekOrigin.Begin);
            byte[] bytes = reader.ReadBytes(featureCount * featureSizeInBytes);

            List<float[]> result = new List<float[]>();

            for (int i = 0; i < featureCount; i++)
            {
                float[] floats = new float[featureDimension];
                Buffer.BlockCopy(bytes, i * featureSizeInBytes, floats, 0, featureSizeInBytes);
                result.Add(floats);
            }

            return result;
        }


        public LinkedList<Features> FindSimilar(long featureID, int kResults)
        {
            float[] queryFeature = GetFeature(featureID);
            LinkedList<Features> results = new LinkedList<Features>();

            reader.BaseStream.Seek(featureDataStartOffset, SeekOrigin.Begin);
            for (int i = 0; i < featureCount; i++)
            {
                if (i % 10000 == 0) Console.WriteLine("Checking feature ID: " + i);
                byte[] bytes = reader.ReadBytes(featureDimension * sizeof(float));
                float[] floats = new float[bytes.Length / sizeof(float)];
                Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

                double distance = Distances.FeatureL2(queryFeature, floats);

                if (results.Count == 0)
                {
                    results.AddFirst(new Features(floats, i, distance));
                }
                else if (results.Count < kResults || distance < results.Last.Value.distance)
                {
                    LinkedListNode<Features> node = results.First;
                    while (node != null && node.Value.distance < distance)
                    {
                        node = node.Next;
                    }
                    if (node != null)
                    {
                        results.AddBefore(node, new Features(floats, i, distance));
                    }
                    else
                    {
                        results.AddLast(new Features(floats, i, distance));
                    }

                    if (results.Count > kResults)
                    {
                        results.RemoveLast();
                    }
                }
            }

            return results;
        }


        public LinkedList<Features>[] FindSimilars(int[] featureIDs, int kResults)
        {
            float[][] queryFeatures = new float[featureIDs.Length][];
            for (int i = 0; i < queryFeatures.Length; i++)
            {
                queryFeatures[i] = GetFeature(featureIDs[i]);
            }
            
            LinkedList<Features>[] results = new LinkedList<Features>[featureIDs.Length];
            for (int i = 0; i < featureIDs.Length; i++)
            {
                results[i] = new LinkedList<Features>();
            }

            reader.BaseStream.Seek(featureDataStartOffset, SeekOrigin.Begin);
            for (int i = 0; i < featureCount; i++)
            {
                if (i % 10000 == 0)
                {
                    Console.WriteLine("Checking feature ID: " + i);
                }
                byte[] bytes = reader.ReadBytes(featureDimension * sizeof(float));
                float[] floats = new float[bytes.Length / sizeof(float)];
                Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

                double[] distances = new double[featureIDs.Length];
                Parallel.For(0, featureIDs.Length, j =>
                {
                    distances[j] = Distances.FeatureL2(queryFeatures[j], floats);

                    if (results[j].Count == 0)
                    {
                        results[j].AddFirst(new Features(floats, i, distances[j]));
                    }
                    else if (results[j].Count < kResults || distances[j] < results[j].Last.Value.distance)
                    {
                        LinkedListNode<Features> node = results[j].First;
                        while (node != null && node.Value.distance < distances[j])
                        {
                            node = node.Next;
                        }
                        if (node != null)
                        {
                            results[j].AddBefore(node, new Features(floats, i, distances[j]));
                        }
                        else
                        {
                            results[j].AddLast(new Features(floats, i, distances[j]));
                        }

                        if (results[j].Count > kResults)
                        {
                            results[j].RemoveLast();
                        }
                    }
                });
            }
            

            return results;
        }


        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
