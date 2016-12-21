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
            chart1.Series[UpperChartArea + "group1"].Points.Clear();
            chart1.Series[LowerChartArea + "group1"].Points.Clear();

            var means = new[] { 0.20300992437855453, 0.19668648155473095, 0.1903630387309074, 0.18403959590708382, 0.17771615308326028, 0.1713927102594367, 0.16506926743561312, 0.15874582461178957, 0.15242238178796602, 0.14609893896414244, 0.13977549614031887, 0.13345205331649532, 0.12712861049267174, 0.12080516766884818, 0.11448172484502461, 0.10815828202120105, 0.10183483919737749, 0.09551139637355392, 0.08918795354973036, 0.0828645107259068, 0.07654106790208323, 0.07021762507825965, 0.0638941822544361, 0.05757073943061253, 0.05124729660678895, 0.0449238537829654, 0.03860041095914182, 0.032276968135318274, 0.025953525311494696, 0.019630082487671147, 0.01330663966384757, 0.00698319684002402 };

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
                    var sampledValue = means[0];
                    var min = double.MaxValue;
                    foreach (var m in means)
                    {
                        var tmp = Math.Abs(m - value);
                        if (min > tmp)
                        {
                            min = tmp;
                            sampledValue = m;
                        } 
                    }
                    chart1.Series[UpperChartArea + "group1"].Points.AddXY(date, sampledValue);
                    chart1.Series[LowerChartArea + "group1"].Points.AddXY(date, sampledValue);
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
