using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    public static class Distances
    {
        public enum MetricType { FeatureCosine, FeatureL2, FeatureL2Squared, ColorSimpleL2, SignaturePMHD, SignatureL2SQFD }
        public static double ComputeFeatureSize(float[] feature)
        {
            double distRes = 0;
            for (int i = 0; i < feature.Length; i++)
            {
                distRes += Math.Pow(feature[i], 2);
            }
            return Math.Sqrt(distRes);
        }
        public static double FeatureCosine(Frame f1, Frame f2)
        {
            return (1 - (FeatureScalarMult(f1.feature, f2.feature))); // / (f1.featureSize * f2.featureSize)));
        }
        public static double FeatureScalarMult(float[] feature1, float[] feature2)
        {
            double result = 0;

            for (int i = 0; i < feature1.Length; i++)
            {
                result += feature1[i] * feature2[i];
            }

            return result;
        }
        public static double FeatureL2(Frame f1, Frame f2)
        {
            return FeatureL2(f1.feature, f2.feature);
        }
        public static double FeatureL2(float[] f1, float[] f2)
        {
            double distRes = 0;
            for (int i = 0; i < f1.Length; i++)
            {
                distRes += Math.Pow(f1[i] - f2[i], 2);
            }
            return Math.Sqrt(distRes);
        }
        public static double SimpleColorNonLockSecond(Bitmap Thumb1, Bitmap Thumb2, BitmapData img2Data)
        {
            double res = 0;

            //Bitmap small1 = ResizeImage(Thumb1, 4, 3);
            //Bitmap small2 = ResizeImage(Thumb2, 4, 3);

            Bitmap small1 = Thumb1;
            Bitmap small2 = Thumb2;

            Rectangle minRect = new Rectangle(0, 0, Math.Min(small1.Width, small2.Width), Math.Min(small1.Height, small2.Height));
            BitmapData img1Data;
            //BitmapData img2Data;
            try
            {
                img1Data = small1.LockBits(minRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                //img2Data = small2.LockBits(minRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(img1Data.PixelFormat) / 8;
                int strideOffset = img1Data.Stride;

                unsafe
                {
                    byte* img2Ptr = (byte*)img2Data.Scan0;
                    byte* img1Ptr = (byte*)img1Data.Scan0;

                    for (int i = 0; i < minRect.Height; i++)
                    {
                        for (int j = 0; j < minRect.Width; j++)
                        {
                            res += Math.Pow(img2Ptr[0] - img1Ptr[0], 2); // Blue
                            res += Math.Pow(img2Ptr[1] - img1Ptr[1], 2); // Green
                            res += Math.Pow(img2Ptr[2] - img1Ptr[2], 2); // Red
                                                                         //img2Ptr[3] = img1Ptr[3]; // Alpha

                            img1Ptr += bytesPerPixel;
                            img2Ptr += bytesPerPixel;
                        }
                        img1Ptr += strideOffset;
                        img2Ptr += strideOffset;
                    }
                }

                small1.UnlockBits(img1Data);
                //small2.UnlockBits(img2Data);
            }
            catch (InvalidOperationException e)
            {

                Trace.WriteLine("Problem Colordist");
            }
            finally
            {

            }

            /*
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    res += Math.Pow(small1.GetPixel(i, j).R - small2.GetPixel(i, j).R, 2) +
                        Math.Pow(small1.GetPixel(i, j).G - small2.GetPixel(i, j).G, 2) +
                        Math.Pow(small1.GetPixel(i, j).B - small2.GetPixel(i, j).B, 2);
                }
            }*/

            return Math.Sqrt(res);
        }
        public static double SimpleColor(Frame f1, Frame f2)
        {
            return SimpleColor(f1.ThumbSample, f2.ThumbSample);
        }
        public static double SimpleColor(Bitmap Thumb1, Bitmap Thumb2)
        {
            double res = 0;

            //Bitmap small1 = ResizeImage(Thumb1, 4, 3);
            //Bitmap small2 = ResizeImage(Thumb2, 4, 3);

            Bitmap small1 = Thumb1;
            Bitmap small2 = Thumb2;

            Rectangle minRect = new Rectangle(0, 0, Math.Min(small1.Width, small2.Width), Math.Min(small1.Height, small2.Height));
            BitmapData img1Data;
            BitmapData img2Data;
            try
            {
                img1Data = small1.LockBits(minRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                img2Data = small2.LockBits(minRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(img1Data.PixelFormat) / 8;
                int strideOffset = img1Data.Stride;

                unsafe
                {
                    byte* img2Ptr = (byte*)img2Data.Scan0;
                    byte* img1Ptr = (byte*)img1Data.Scan0;

                    for (int i = 0; i < minRect.Height; i++)
                    {
                        for (int j = 0; j < minRect.Width; j++)
                        {
                            res += Math.Pow(img2Ptr[0] - img1Ptr[0], 2); // Blue
                            res += Math.Pow(img2Ptr[1] - img1Ptr[1], 2); // Green
                            res += Math.Pow(img2Ptr[2] - img1Ptr[2], 2); // Red
                                                            //img2Ptr[3] = img1Ptr[3]; // Alpha

                            img1Ptr += bytesPerPixel;
                            img2Ptr += bytesPerPixel;
                        }
                        img1Ptr += strideOffset;
                        img2Ptr += strideOffset;
                    }
                }

                small1.UnlockBits(img1Data);
                small2.UnlockBits(img2Data);
            }
            catch (InvalidOperationException e)
            {
                
                Trace.WriteLine("Problem Colordist");
            }
            finally
            {

            }

            /*
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    res += Math.Pow(small1.GetPixel(i, j).R - small2.GetPixel(i, j).R, 2) +
                        Math.Pow(small1.GetPixel(i, j).G - small2.GetPixel(i, j).G, 2) +
                        Math.Pow(small1.GetPixel(i, j).B - small2.GetPixel(i, j).B, 2);
                }
            }*/

            return Math.Sqrt(res);
        }

        public static double PMHD(Frame Frame1, Frame Frame2)
        {
            return PMHD(Frame1.Signature(), Frame2.Signature());
        }

        public static double PMHD(ISPoint[] FS1, ISPoint[] FS2)
        {
            int sizeFS1 = FS1.Length;
            int sizeFS2 = FS2.Length;

            double[] d1 = new double[sizeFS1];
            double[] d2 = new double[sizeFS2];

            double sumW1 = 0.0, sumW2 = 0.0, d = 0.0;

            for (int i = 0; i < sizeFS1; i++) { sumW1 += FS1[i].R; d1[i] = double.MaxValue; }
            for (int j = 0; j < sizeFS2; j++) { sumW2 += FS2[j].R; d2[j] = double.MaxValue; }

            for (int i = 0; i < sizeFS1; i++)
                for (int j = 0; j < sizeFS2; j++)
                {
                    d = Math.Sqrt(FS1[i].LegacySearchDistanceSquare(FS2[j]));

                    d = d / Math.Min(FS1[i].R / sumW1, FS2[j].R / sumW2);

                    if (d1[i] > d) d1[i] = d;
                    if (d2[j] > d) d2[j] = d;
                }

            double result1 = 0.0, result2 = 0.0;

            for (int i = 0; i < sizeFS1; i++) result1 += d1[i] * FS1[i].R;
            for (int j = 0; j < sizeFS2; j++) result2 += d2[j] * FS2[j].R;

            return Math.Max(result1 / sumW1, result2 / sumW2);
        }

        public static double L2SQFD(Frame f1, Frame f2)
        {
            return f1.Mean.SearchDistance(f2.Mean);
        }

        public static double SQFD(Frame f1, Frame f2)
        {
            return SQFD(f1.Signature(), f2.Signature());
        }

        public static double SQFD(ISPoint[] signature1, ISPoint[] signature2)
        {
            double result = 0;



            return result;
        }

        public static Bitmap ResizeImage(Image imgToResize, int width, int heigth)
        {
            return new Bitmap(imgToResize, new Size(width, heigth));
        }

        public static Bitmap ResizeImageHighQ(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
