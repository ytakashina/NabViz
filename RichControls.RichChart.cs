using System;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace RichControls
{
    public static class RichChart
    {
        public static void Initialize(this Chart self)
        {
            self.Series.Clear();
            self.Legends.Clear();
            self.ChartAreas.Clear();
            self.ChartAreas.Add(new ChartArea());
        }

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
                Debug.WriteLine(legend + " is empty series.");
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
                Debug.WriteLine("Series[" + index + "] is empty series.");
            }
        }
    }
}
