using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{

    public class ImageExtractor
    {
        string path;
        List<Result> loaded;

        Size imgSize = new Size(64, 48);
        
        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public ImageExtractor(string path)
        {
            this.path = path;
            loaded = new List<Result>();

            ReadAll(Directory.GetFiles(path, "*.JPG"));
        }
        public ImageExtractor(IEnumerable<string> files)
        {
            //this.path = path;
            loaded = new List<Result>();

            ReadAll(files);
        }

        private void ReadAll(IEnumerable<string> files)
        {
            Parallel.ForEach(files, (imgPath) =>
            {
                string tempPath = imgPath + ".temp";
                Result res = null;
                if (File.Exists(tempPath))
                {
                    res = ReadFromBinaryFile<Result>(tempPath);
                }
                else if (File.Exists(imgPath) && imgPath.EndsWith(".temp"))
                {
                    res = ReadFromBinaryFile<Result>(imgPath);
                }
                else if (imgPath.EndsWith("jpg") || imgPath.EndsWith("JPG"))
                {
                    using (Image image = Image.FromFile(imgPath))
                    {
                        //Frame Frame = new Frame(image);
                        res = new Result(ResizeImage(image, imgSize), imgPath);
                        //ci++;
                    }
                    WriteToBinaryFile<Result>(tempPath, res, false);
                }
                else
                {
                    //MessageBoxResult mbr = MessageBox.Show("File format not supported.");
                }
                loaded.Add(res);
            });
        }

        public Result[] GetResArr()
        {
            return loaded.ToArray();
        }
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
