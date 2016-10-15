using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz
{
    public partial class Form1
    {
        private void InitializeChart()
        {
            chart1.ChartAreas.Add(new ChartArea(UpperChartArea));
            chart1.ChartAreas.Add(new ChartArea(LowerChartArea));
            chart1.ChartAreas[UpperChartArea].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            chart1.ChartAreas[LowerChartArea].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            InitializeInnerPlotPosition(UpperChartArea);
            InitializeInnerPlotPosition(LowerChartArea);

            // Ideally only one `Series` instance be created and displayed in both charts
            // though it's impossible as far as I tried.
            AddSeriesToChart(UpperChartArea);
            AddSeriesToChart(LowerChartArea);
            foreach (var detectorName in Detection.Detectors)
            {
                AddDetectionSeriesToChart(UpperChartArea, detectorName);
                AddDetectionSeriesToChart(LowerChartArea, detectorName);
            }
        }

        private void InitializeInnerPlotPosition(string name)
        {
            var inner = chart1.ChartAreas[name].InnerPlotPosition;
            inner.Width = 92;
            inner.Height = 90;
            inner.X = 8;
            inner.Y = 0;
        }

        private void AddSeriesToChart(string name)
        {
            chart1.Series.Add(new Series
            {
                Name = name,
                ChartArea = name,
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                Color = Color.CornflowerBlue
            });
        }

        private void AddDetectionSeriesToChart(string chartArea, string detector)
        {
            chart1.Series.Add(new Series
            {
                Name = chartArea + detector,
                ChartArea = chartArea,
                ChartType = SeriesChartType.Point,
                XValueType = ChartValueType.DateTime,
                MarkerSize = 10,
                MarkerBorderWidth = 1,
                MarkerBorderColor = Color.Red,
                Color = Color.Transparent,
                Enabled = false
            });
        }

        private void LoadRawDataToChart()
        {
            chart1.Series[UpperChartArea].Points.Clear();
            chart1.Series[LowerChartArea].Points.Clear();

            var path = Path.Combine("..", "data", treeView1.SelectedNode.FullPath);
            if (!File.Exists(path)) throw new FileNotFoundException("\"" + path + "\"" + " does not exist.");
            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine().Split(',').ToList();
                var dateColumnIndex = head.IndexOf("timestamp");
                var valueColumnIndex = head.IndexOf("value");
                if (dateColumnIndex == -1) throw new FormatException("\"timestamp\" column does not exist.");
                if (valueColumnIndex == -1) throw new FormatException("\"value\" column does not exist.");
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var date = DateTime.ParseExact(line[dateColumnIndex], "yyyy-MM-dd HH:mm:ss", null);
                    var value = double.Parse(line[valueColumnIndex]);
                    chart1.Series[UpperChartArea].Points.AddXY(date, value);
                    chart1.Series[LowerChartArea].Points.AddXY(date, value);
                }
            }
        }

        private void LoadDetectionDataToChart()
        {
            Detection.LoadResults(treeView1.SelectedNode.FullPath);
            if (!Detection.Detectors.Any()) return;

            var anomalyPoints = Detection.Detectors.ToDictionary(s => s, s => new List<DataPoint>());

            _dataReader.Rewind();
            while (!_dataReader.EndOfStream)
            {
                var point = _dataReader.Next;
                foreach (var detector in Detection.Detectors)
                {
                    var score = Detection.Results[detector][treeView1.SelectedNode.FullPath][DateTime.FromOADate(point.XValue)];
                    if (score >= Detection.Thresholds[detector]) anomalyPoints[detector].Add(point);
                }
            }

            foreach (var detectorName in Detection.Detectors)
            {
                chart1.Invoke((Action)(() =>
                {
                    chart1.Series[UpperChartArea + detectorName].Points.Clear();
                    chart1.Series[LowerChartArea + detectorName].Points.Clear();
                    foreach (var point in anomalyPoints[detectorName])
                    {
                        chart1.Series[UpperChartArea + detectorName].Points.Add(point.Clone());
                        chart1.Series[LowerChartArea + detectorName].Points.Add(point.Clone());
                    }
                }));
            }
        }
    }
}
