using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHVisualisation
{
    class Tsne
    {
        int objCount;
        double[,] distances;
        int dimensionality;

        public Tsne(int count, int dim)
        {
            dimensionality = dim;
            objCount = count;
            distances = new double[count, count];
        }

        private double calculateSymmProb(double[] feature1, double[] feature2)
        {
            double result = 0;

            int len = Math.Min(feature1.Length, feature2.Length);

            double gamma = 1d / Math.Sqrt(2);

            for(int i=0; i<len; i++)
            {

            }
            return result;
        }
    }
}
