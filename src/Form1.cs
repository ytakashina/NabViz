using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RichForms;

namespace NabViz
{
    public partial class Form1 : Form
    {
        private const string UpperChartArea = "Global";
        private const string LowerChartArea = "Local";
        private readonly List<string> _detectors;
        private Graphics _graphics;
        private readonly Brush _windowBrush;
        private RectangleD _selection;
        private bool _dataLoadCompleted;
        private DataReader _dataReader;
        private readonly System.Timers.Timer timer2;
        private readonly double _defaultInterval;

        public Form1()
        {
            InitializeComponent();

            treeView1.PathSeparator = Path.DirectorySeparatorChar.ToString();

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent(Color.White);
            _graphics = Graphics.FromImage(bmp);
            _windowBrush = new SolidBrush(Color.FromArgb(64, Color.Red));
            pictureBox1.BackgroundImage = bmp;
            pictureBox1.Parent = chart1;

            timer1.Start();
            timer2 = new System.Timers.Timer();
            timer2.Elapsed += timer2_Tick;
            _defaultInterval = timer2.Interval;

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

            var rootDir = new DirectoryInfo(Path.Combine("..", "data"));
            foreach (var dir in rootDir.GetDirectories())
            {
                var node = new TreeNode(dir.ToString());
                var files = dir.GetFiles("*.csv");
                foreach (var f in files) node.Nodes.Add(f.ToString());
                treeView1.Nodes.Add(node);
            }

            _detectors = new List<string>();
//            foreach (var elm in AnomalyResults.Dictionary)
//            {
//                _detectors.Add(elm.Key);
//            }
            foreach (var detector in _detectors)
            {
                tableLayoutPanel1.Controls.Add(new CheckBox());
                tableLayoutPanel1.Controls.Add(new Label { Text = detector });
                tableLayoutPanel1.Controls.Add(ComboBoxFactory.Instance.GetComboBox());
            }
            for (var i = 0; i < _detectors.Count; i++)
            {
                chart1.Series.Add(new Series
                {
                    Name = UpperChartArea + _detectors[i],
                    XValueType = ChartValueType.DateTime,
                    ChartArea = UpperChartArea,
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = (MarkerStyle)(i%9+1),
                    MarkerSize = 10,
                    MarkerBorderWidth = 1,
                    MarkerBorderColor = Color.Red,
                    Color = Color.Transparent
                });
                chart1.Series.Add(new Series
                {
                    Name = LowerChartArea + _detectors[i],
                    XValueType = ChartValueType.DateTime,
                    ChartArea = LowerChartArea,
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = (MarkerStyle)(i % 9 + 1),
                    MarkerSize = 20,
                    MarkerBorderWidth = 1,
                    MarkerBorderColor = Color.Red,
                    Color = Color.Transparent
                });
            }

            _selection.Width = 100;
        }

        private void InitializeInnerPlotPosition(string name)
        {
            var inner = chart1.ChartAreas[name].InnerPlotPosition;
            inner.Width = 94;
            inner.Height = 90;
            inner.X = 6;
            inner.Y = 0;
        }

        private void InitializeSelection()
        {
            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            _selection.X = axisX.ValueToPixelPosition(axisX.Minimum);
        }

        /// <summary>
        /// Chart 更新後にこのメソッドするには chart1.Refresh() などで一度 Chart を再描画する必要がある。
        /// クソだけど Windows.Forms の Chart の仕様なので仕方ない。
        /// </summary>
        private void AdjustSelection()
        {
            if (!_dataLoadCompleted) return;
            var axisY = chart1.ChartAreas[UpperChartArea].AxisY;
            _selection.Y = axisY.ValueToPixelPosition(axisY.Maximum);
            _selection.Height = axisY.ValueToPixelPosition(axisY.Minimum) - _selection.Y;

            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            var minX = axisX.ValueToPixelPosition(axisX.Minimum);
            var maxX = axisX.ValueToPixelPosition(axisX.Maximum);

            if (_selection.Width < 1) _selection.Width = 1;
            if (_selection.Width > maxX - minX) _selection.Width = maxX - minX;

            if (_selection.Right > maxX) _selection.X = maxX - _selection.Width;
            if (_selection.X < minX) _selection.X = minX;
        }

        private void DrawSelectedRange()
        {
            _graphics.DrawRectangle(checkBox1.Checked ? Pens.Red : Pens.Orange, (Rectangle)_selection);
        }

        private void DrawDataScanner(string name)
        {
            if (!_dataLoadCompleted) return;
            if (_dataReader == null) return;
            var axisX = chart1.ChartAreas[name].AxisX;
            var axisY = chart1.ChartAreas[name].AxisY;
            var x = (int)axisX.ValueToPixelPosition(_dataReader.Current.XValue);
            var minY = (int)axisY.ValueToPixelPosition(axisY.Minimum);
            var maxY = (int)axisY.ValueToPixelPosition(axisY.Maximum);
            _graphics.DrawLine(Pens.Red, new Point(x, minY), new Point(x, maxY));
        }

        private void DrawAnomaryWindow(string name)
        {
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;
            var axisX = chart1.ChartAreas[name].AxisX;
            var axisY = chart1.ChartAreas[name].AxisY;
            var minY = (int)axisY.ValueToPixelPosition(axisY.Minimum);
            var maxY = (int)axisY.ValueToPixelPosition(axisY.Maximum);
            foreach (var window in AnomalyLabels.Instance[treeView1.SelectedNode.FullPath])
            {
                var minX = (int)axisX.ValueToPixelPosition(window.Item1.ToOADate());
                var maxX = (int)axisX.ValueToPixelPosition(window.Item2.ToOADate());
                _graphics.FillRectangle(_windowBrush, minX, maxY, maxX - minX, minY - maxY);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 実行を止める。
            checkBox2.Checked = false;

            // csv ファイルが選択されたときのみ以降を実行する。
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;

            // イベント駆動は実行時の順序保証がないので、
            // 明示的にデータの読み込みの終了を管理する必要がある。
            _dataLoadCompleted = false;

            chart1.Series[UpperChartArea].Points.Clear();
            chart1.Series[LowerChartArea].Points.Clear();
            foreach (var detector in _detectors)
            {
                chart1.Series[UpperChartArea + detector].Points.Clear();
                chart1.Series[LowerChartArea + detector].Points.Clear();
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


            //foreach (var detector in AnomalyResults.Dictionary)
            //{
            //    foreach (var score in detector.Value[detector.Key + "_" + _fileName.SplitPath.DirectorySeparatorChar).Last()])
            //    {
            //        if (score.Item2 > 0.9999)
            //        {
            //            chart1.Invoke((Action)(() => chart1.Series[UpperChartArea + detector.Key].Points.AddXY(score.Item1, score.Item2)));
            //            chart1.Invoke((Action)(() => chart1.Series[LowerChartArea + detector.Key].Points.AddXY(score.Item1, score.Item2)));
            //        }
            //    }
            //}

            _dataReader = new DataReader(chart1.Series[UpperChartArea]);
            _dataLoadCompleted = true;

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
            if (checkBox1.Checked) return; // trace が ON だったら無視。

            var axisY = chart1.ChartAreas[UpperChartArea].AxisY;
            var minY = axisY.ValueToPixelPosition(axisY.Minimum);
            var mousePosition = chart1.PointToClient(MousePosition);

            if (mousePosition.Y > minY) return;

            _selection.X = mousePosition.X - _selection.Width / 2;
            AdjustSelection();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (checkBox1.Checked) return; // trace が ON だったら無視。
            var delta = e.Delta * SystemInformation.MouseWheelScrollLines / 60.0;
            _selection.Width += delta;
            _selection.X -= delta / 2;
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
            checkBox1.Checked = !checkBox1.Checked;
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
            DrawDataScanner(UpperChartArea);
            DrawDataScanner(LowerChartArea);
            DrawAnomaryWindow(UpperChartArea);
            DrawAnomaryWindow(LowerChartArea);
            pictureBox1.Refresh();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (_dataReader == null) return;
            if (_dataReader.EndOfStream)
            {
                checkBox2.Invoke((Action)(() => { checkBox2.Checked = false; }));
                return;
            }

            var point = _dataReader.Next;
            //int i = 0;
            //foreach (var detector in _detectors)
            //{
            //    var score = AnomalyResults.Dictionary[detector][listBox1.SelectedItem.ToString().Split(Path.DirectorySeparatorChar).Last()][i++].Item2;
            //    if (score > 0.9999)
            //    {
            //        chart1.Invoke((Action)(() => chart1.Series[UpperChartArea + detector].Points.AddXY(point.XValue, point.YValues[0])));
            //        chart1.Invoke((Action)(() => chart1.Series[LowerChartArea + detector].Points.AddXY(point.XValue, point.YValues[0])));
            //        var str = "[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]";
            //    }
            //}

            // trace
            if (checkBox1.Checked)
            {
                var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
                var x = axisX.ValueToPixelPosition(_dataReader.Current.XValue) - _selection.Width / 2;
                if (x > _selection.X) _selection.X = x;
                var maxX = axisX.ValueToPixelPosition(axisX.Maximum);
                if (_selection.Right > maxX) _selection.X = maxX - _selection.Width;
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (!_dataLoadCompleted)
                {
                    checkBox2.Checked = false;
                    return;
                }
                timer2.Start();
            }
            else
            {
                timer2.Stop();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Interval = _defaultInterval;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Interval = _defaultInterval / 2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Interval = _defaultInterval / 4;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Interval = _defaultInterval / 8;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            timer2.Interval = _defaultInterval / 16;
        }
    }
}
