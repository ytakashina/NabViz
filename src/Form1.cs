using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz
{
    public partial class Form1 : Form
    {
        private const string UpperChartArea = "Global";
        private const string LowerChartArea = "Local";
        private Graphics _graphics;
        private readonly Brush _windowBrush;
        private RectangleF _selection;
        private bool _dataLoadCompleted;
        private DataReader _dataReader;
        private bool _selectionFixed;

        public Form1()
        {
            InitializeComponent();

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent(Color.White);
            _graphics = Graphics.FromImage(bmp);
            _windowBrush = new SolidBrush(Color.FromArgb(64, Color.Red));
            pictureBox1.BackgroundImage = bmp;
            pictureBox1.Parent = chart1;

            timer1.Start();

            chart1.ChartAreas.Add(new ChartArea(UpperChartArea));
            chart1.ChartAreas.Add(new ChartArea(LowerChartArea));
            chart1.ChartAreas[UpperChartArea].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            chart1.ChartAreas[LowerChartArea].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            InitializeInnerPlotPosition(UpperChartArea);
            InitializeInnerPlotPosition(LowerChartArea);

            // 本当だったら 1 つの Series を上下両方の ChartArea に描画したいのだが、
            // Chart の仕様でできないっぽい？
            // Detector の Series を 2 つ作っているのもそのため。
            chart1.Series.Add(new Series
            {
                Name = UpperChartArea,
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                ChartArea = UpperChartArea,
                Color = Color.CornflowerBlue
            });
            chart1.Series.Add(new Series
            {
                Name = LowerChartArea,
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                ChartArea = LowerChartArea,
                Color = Color.CornflowerBlue
            });

            treeView1.PathSeparator = Path.DirectorySeparatorChar.ToString();

            var rootDir = new DirectoryInfo(Path.Combine("..", "data"));
            foreach (var dir in rootDir.GetDirectories())
            {
                var node = new TreeNode(dir.ToString());
                var files = dir.GetFiles("*.csv");
                foreach (var f in files) node.Nodes.Add(f.ToString());
                treeView1.Nodes.Add(node);
            }
            foreach (var detectorName in Detectors.List)
            {
                tableLayoutPanel1.Controls.Add(new Label {Text = detectorName});
                tableLayoutPanel1.Controls.Add(CreateDetectorComboBox(detectorName));
                tableLayoutPanel1.Controls.Add(CreateThresholdTextBox(detectorName));

                chart1.Series.Add(new Series
                {
                    Name = UpperChartArea + detectorName,
                    XValueType = ChartValueType.DateTime,
                    ChartArea = UpperChartArea,
                    ChartType = SeriesChartType.Point,
                    MarkerSize = 10,
                    MarkerBorderWidth = 1,
                    MarkerBorderColor = Color.Red,
                    Color = Color.Transparent,
                    Enabled = false
                });
                chart1.Series.Add(new Series
                {
                    Name = LowerChartArea + detectorName,
                    XValueType = ChartValueType.DateTime,
                    ChartArea = LowerChartArea,
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.None,
                    MarkerSize = 20,
                    MarkerBorderWidth = 1,
                    MarkerBorderColor = Color.Red,
                    Color = Color.Transparent,
                    Enabled = false
                });
            }

            _selection.Width = 100;
        }

        private void InitializeInnerPlotPosition(string name)
        {
            var inner = chart1.ChartAreas[name].InnerPlotPosition;
            inner.Width = 92;
            inner.Height = 90;
            inner.X = 8;
            inner.Y = 0;
        }

        private void InitializeSelection()
        {
            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            _selection.X = (float) axisX.ValueToPixelPosition(axisX.Minimum);
        }

        /// <summary>
        /// Chart 更新後にこのメソッドするには chart1.Refresh() などで一度 Chart を再描画する必要がある。
        /// クソだけど Windows.Forms の Chart の仕様なので仕方ない。
        /// </summary>
        private void AdjustSelection()
        {
            if (!_dataLoadCompleted) return;
            var axisY = chart1.ChartAreas[UpperChartArea].AxisY;
            _selection.Y = (float) axisY.ValueToPixelPosition(axisY.Maximum);
            _selection.Height = (float) axisY.ValueToPixelPosition(axisY.Minimum) - _selection.Y;

            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            var minX = axisX.ValueToPixelPosition(axisX.Minimum);
            var maxX = axisX.ValueToPixelPosition(axisX.Maximum);

            if (_selection.Width < 1) _selection.Width = 1;
            if (_selection.Width > maxX - minX) _selection.Width = (float) (maxX - minX);

            if (_selection.Right > maxX) _selection.X = (float) (maxX - _selection.Width);
            if (_selection.X < minX) _selection.X = (float) minX;
        }

        private void DrawSelectedRange()
        {
            _graphics.DrawRectangles(Pens.Red, new[] {_selection});
        }

        private void DrawAnomaryWindow(string name)
        {
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;
            var axisX = chart1.ChartAreas[name].AxisX;
            var axisY = chart1.ChartAreas[name].AxisY;
            var minY = (int) axisY.ValueToPixelPosition(axisY.Minimum);
            var maxY = (int) axisY.ValueToPixelPosition(axisY.Maximum);
            foreach (var window in AnomalyLabels.Instance[treeView1.SelectedNode.FullPath])
            {
                var minX = (int) axisX.ValueToPixelPosition(window.Item1.ToOADate());
                var maxX = (int) axisX.ValueToPixelPosition(window.Item2.ToOADate());
                _graphics.FillRectangle(_windowBrush, minX, maxY, maxX - minX, minY - maxY);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // csv ファイルが選択されたときのみ以降を実行する。
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;

            // イベント駆動は実行時の順序保証がないので、
            // 明示的にデータの読み込みの終了を管理する必要がある。
            _dataLoadCompleted = false;

            chart1.Series[UpperChartArea].Points.Clear();
            chart1.Series[LowerChartArea].Points.Clear();
            foreach (var detectorName in Detectors.List)
            {
                chart1.Series[UpperChartArea + detectorName].Points.Clear();
                chart1.Series[LowerChartArea + detectorName].Points.Clear();
            }

            var path = Path.Combine("..", "data", treeView1.SelectedNode.FullPath);
            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine().Split(',').ToList();
                var dateColumnIndex = head.IndexOf("timestamp");
                var valueColumnIndex = head.IndexOf("value");
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var date = DateTime.ParseExact(line[dateColumnIndex], "yyyy-MM-dd HH:mm:ss", null);
                    var value = double.Parse(line[valueColumnIndex]);
                    chart1.Series[UpperChartArea].Points.AddXY(date, value);
                    chart1.Series[LowerChartArea].Points.AddXY(date, value);
                }
            }
//            var x = DetectionResults.By;
            DetectionResults.Load(treeView1.SelectedNode.FullPath);

            _dataReader = new DataReader(chart1.Series[UpperChartArea]);
            _dataLoadCompleted = true;

            UpdateChart();

            // Chart の仕様上、一度描画されないと ValueToPixelPosition が使えないらしい。
            // RecalculateAxesScale でなんとかならなかった。
            // chart1.ChartAreas[UpperChartArea].RecalculateAxesScale();
            chart1.Refresh();

            // 下の ChartArea[1] の Y 軸方向のスケールを、上の ChartArea[0] に合わせる。
            var axisY = chart1.ChartAreas[UpperChartArea].AxisY;
            chart1.ChartAreas[LowerChartArea].AxisY.Minimum = axisY.Minimum;
            chart1.ChartAreas[LowerChartArea].AxisY.Maximum = axisY.Maximum;

            // 選択範囲を現在の ChartArea[0] に合わせる。
            InitializeSelection();
            AdjustSelection();
        }

        private void UpdateChart()
        {
            var anomalyPoints = Detectors.List.ToDictionary(s => s, s => new List<DataPoint>());

            _dataReader.Rewind();
            while (!_dataReader.EndOfStream)
            {
                var point = _dataReader.Next;
                foreach (var detector in Detectors.List)
                {
                    var score = DetectionResults.Dictionary[detector][treeView1.SelectedNode.FullPath][DateTime.FromOADate(point.XValue)];
                    if (score >= DetectionThresholds.Dictionary[detector]) anomalyPoints[detector].Add(point);
                }
            }

            foreach (var detectorName in Detectors.List)
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

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;

            chart1.Refresh(); // 下の AdjustSelection() が動くのに必要。
            AdjustSelection();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (_selectionFixed) return;

            var axisY = chart1.ChartAreas[UpperChartArea].AxisY;
            var minY = axisY.ValueToPixelPosition(axisY.Minimum);
            var mousePosition = chart1.PointToClient(MousePosition);

            if (mousePosition.Y > minY) return;

            _selection.X = mousePosition.X - _selection.Width/2;
            AdjustSelection();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (_selectionFixed) return;

            var delta = e.Delta*SystemInformation.MouseWheelScrollLines/60.0f;
            _selection.Width += delta;
            _selection.X -= delta/2;
            chart1.ChartAreas[LowerChartArea].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            AdjustSelection();
            chart1.ChartAreas[LowerChartArea].AxisX.IntervalAutoMode = IntervalAutoMode.FixedCount;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _selectionFixed = !_selectionFixed;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_dataLoadCompleted) return;

            // 上の Chart の選択範囲に応じて下の Chart のデータを更新。
            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            var minX = axisX.PixelPositionToValue(_selection.X);
            var maxX = axisX.PixelPositionToValue(_selection.Right);
            chart1.ChartAreas[LowerChartArea].AxisX.Minimum = minX;
            chart1.ChartAreas[LowerChartArea].AxisX.Maximum = maxX;

            _graphics.Clear(Color.Transparent);
            DrawSelectedRange();
            DrawAnomaryWindow(UpperChartArea);
            DrawAnomaryWindow(LowerChartArea);
            pictureBox1.Refresh();
        }

        private ComboBox CreateDetectorComboBox(string detectorName)
        {
            var box = new ComboBox();
            box.Width = 70;
            box.Name = detectorName;
            for (int i = 0; i < 10; i++) box.Items.Add((MarkerStyle)i);
            box.SelectedIndex = 0;
            box.SelectedIndexChanged += detectorBox_SelectedIndexChanged;
            return box;
        }

        private TextBox CreateThresholdTextBox(string detectorName)
        {
            var box = new TextBox();
            box.Width = 60;
            box.Name = detectorName;
            box.Text = DetectionThresholds.Dictionary[detectorName].ToString();
            box.TextChanged += thresholdBox_TextChanged;
            return box;
        }

        private void detectorBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var box = (ComboBox) sender;
            var detectorName = box.Name;
            var markerStyle = (MarkerStyle)Enum.Parse(typeof(MarkerStyle), box.SelectedItem.ToString());
            if (markerStyle == MarkerStyle.None)
            {
                chart1.Series[UpperChartArea + detectorName].Enabled = false;
                chart1.Series[LowerChartArea + detectorName].Enabled = false;
                return;
            }
            chart1.Series[UpperChartArea + detectorName].Enabled = true;
            chart1.Series[LowerChartArea + detectorName].Enabled = true;
            chart1.Series[UpperChartArea + detectorName].MarkerStyle = markerStyle;
            chart1.Series[LowerChartArea + detectorName].MarkerStyle = markerStyle;
        }

        private void thresholdBox_TextChanged(object sender, EventArgs e)
        {
            var box = (TextBox)sender;
            var detectorName = box.Name;
            double tmp;
            if (!double.TryParse(box.Text, out tmp)) return;
            DetectionThresholds.Dictionary[detectorName] = tmp;
            UpdateChart();
        }
    }
}