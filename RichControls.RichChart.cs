using System;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace RichControls
{
    public static partial class Extensions
    {
        public static void AddPoints(this Chart self, Tuple<DateTime, double>[] points, string legend)
        {
            self.Legends.Remove(self.Legends.FindByName(legend));
            self.Legends.Add(legend);
            self.Series.Remove(self.Series.FindByName(legend));
            self.Series.Add(legend);
            self.Series[legend].ChartType = SeriesChartType.Line;
            foreach (var p in points)
            {
                self.AddPoint(p, legend);
            }
        }

        public static void AddPoint(this Chart self, Tuple<DateTime, double> point, string legend)
        {
            try
            {
                self.Series[legend].Points.AddXY(point.Item1, point.Item2);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Series[" + legend + "] is empty.");
            }
        }

        public static void AddPoint(this Chart self, Tuple<DateTime, double> point, int index)
        {
            try
            {
                self.Series[0].Points.AddXY(point.Item1, point.Item2);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Series[" + index + "] is empty.");
            }
        }
    }
}
