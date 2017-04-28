using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VHVisualisation
{
    public static class UIhelpers
    {
        public static void SetNumberOfRows(this Grid grid, int value, double height = 0)
        {
            if (height == 0)
            {
                height = Double.IsNaN(grid.Height) ? grid.ActualHeight / value : grid.Height / value;
            }

            grid.RowDefinitions.Clear();
            for (int i = 0; i < value; i++)
            {
                RowDefinition rd = new RowDefinition()
                {
                    Height = new GridLength(height)
                };
                grid.RowDefinitions.Add(rd);
            }
        }
        public static void SetNumberOfColumns(this Grid grid, int value, double width = 0)
        {
            if (width == 0)
            {
                width = Double.IsNaN(grid.Width) ? grid.ActualWidth / value : grid.Width / value;
            }

            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < value; i++)
            {
                ColumnDefinition cd = new ColumnDefinition()
                {
                    Width = new GridLength(width)
                };
                grid.ColumnDefinitions.Add(cd);
            }
        }
    }
}
