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
    public class BitmapReader : IDisposable
    {
        private BinaryReader reader;
        private char[] header = "VidSee merged bitmaps           ".ToCharArray();
        private int bitmapWidth;
        private int bitmapHeight;
        private const int bitmapStride = 3;
        public int[] videoOffsets;
        public int frameCount;

        private long bitmapDataStartOffset;
        private int bitmapSize;

        public BitmapReader(string filename)
        {
            try
            {
                reader = new BinaryReader(File.OpenRead(filename));
            }
            catch(IOException ex)
            {
                MessageBox.Show("File: " + filename + " could not be found. Rather shutting down.");
                //Not possible from different thread
                //Application.Current.Shutdown();
            }
            ReadHeader();
        }
        
        private void ReadHeader()
        {
            char[] headerCheck = reader.ReadChars(header.Length);
            bitmapWidth = reader.ReadInt32();
            bitmapHeight = reader.ReadInt32();
            int videoCount = reader.ReadInt32();
            frameCount = reader.ReadInt32();
            videoOffsets = new int[videoCount];

            int metadataSize = header.Length + 4 * sizeof(int) + videoCount * sizeof(int);
            const int blockSize = 4096;
            bitmapDataStartOffset = ((metadataSize / blockSize) + 1) * blockSize;
            bitmapSize = bitmapWidth * bitmapHeight * bitmapStride;

            for (int i = 0; i < videoCount; i++)
            {
                videoOffsets[i] = reader.ReadInt32();
            }

        }

        public unsafe static bool OneColor(Bitmap b)
        {
            Rectangle rect = new Rectangle(0, 0, b.Width, b.Height);

            BitmapData bitmapData = b.LockBits(rect, ImageLockMode.ReadOnly, b.PixelFormat);

            byte* data1 = (byte*)bitmapData.Scan0;
            int count = b.Width * b.Height, diff; float x = 0;
            int size = Image.GetPixelFormatSize(b.PixelFormat) / 8;
            int offset = b.Width * size + 1;

            for (int i = 1; i < count; i++)
                for (int j = 0; j < size; j++)
                {
                    diff = Math.Abs(data1[offset + j] - data1[i * size + j]);
                    if (diff < 20) { x++; break; }
                }

            b.UnlockBits(bitmapData);

            if ((x / count) * 100 > 90) return true;

            return false;
        }

        public unsafe static double L2Distance(Bitmap bmp1, Bitmap bmp2, int width, int height)
        {
            double result = 0.0;
            Bitmap b1 = new Bitmap(bmp1, width, height);
            Bitmap b2 = new Bitmap(bmp2, width, height);

            Rectangle rect = new Rectangle(0, 0, b1.Width, b1.Height);
            
            BitmapData bitmapData1 = b1.LockBits(rect, ImageLockMode.ReadWrite, b1.PixelFormat);
            BitmapData bitmapData2 = b2.LockBits(rect, ImageLockMode.ReadWrite, b2.PixelFormat);
            
            byte* data1 = (byte*)bitmapData1.Scan0;
            byte* data2 = (byte*)bitmapData2.Scan0;
            int count = b1.Width * b1.Height * Image.GetPixelFormatSize(b1.PixelFormat) / 8;
            for (int i = 0; i < count; i++)
                result += Math.Abs(data1[i] - data2[i]);

            b1.UnlockBits(bitmapData1);
            b2.UnlockBits(bitmapData2);

            b1.Dispose(); b2.Dispose();

            return result;
        }

        public Bitmap ReadFrame(int frameID)
        {
            Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format24bppRgb);


            Rectangle rect = new Rectangle(0, 0, bitmapWidth, bitmapHeight);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bitmapData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = 3 * bitmapWidth * bitmapHeight;

            reader.BaseStream.Seek(bitmapDataStartOffset + (long)frameID * bitmapSize, SeekOrigin.Begin);
            byte[] rgbValues = reader.ReadBytes(bytes);

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            //writer.Write(rgbValues);

            // Copy the RGB values back to the bitmap
            //System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }


        public Bitmap[] ReadVideo(int videoID)
        {
            int videoFrameCount;
            if (videoID != videoOffsets.Length - 1)
            {
                videoFrameCount = videoOffsets[videoID + 1] - videoOffsets[videoID];
            }
            else
            {
                videoFrameCount = frameCount - videoOffsets[videoID];
            }

            Bitmap[] bitmaps = new Bitmap[videoFrameCount];
            for (int i = 0; i < videoFrameCount; i++)
            {
                bitmaps[i] = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format24bppRgb);
            }

            reader.BaseStream.Seek(bitmapDataStartOffset + (long)videoOffsets[videoID] * bitmapSize, SeekOrigin.Begin);
            for (int i = 0; i < videoFrameCount; i++)
            {


                Rectangle rect = new Rectangle(0, 0, bitmapWidth, bitmapHeight);
                BitmapData bitmapData = bitmaps[i].LockBits(rect, ImageLockMode.ReadWrite, bitmaps[i].PixelFormat);
                // Get the address of the first line.
                IntPtr ptr = bitmapData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = 3 * bitmapWidth * bitmapHeight;

                byte[] rgbValues = reader.ReadBytes(bytes);

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

                //writer.Write(rgbValues);

                // Copy the RGB values back to the bitmap
                //System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

                // Unlock the bits.
                bitmaps[i].UnlockBits(bitmapData);
            }
            return bitmaps;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
