using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RichControls;

namespace ZetaOne
{
    public partial class Form1 : Form
    {
        //private RectangleD _selected;
        private Tuple<int, int> _selected;
        private bool _fixed;
        private bool _dataLoadCompleted;
        private DataReader _dataReader;
        private readonly System.Timers.Timer timer2;
        private readonly double _defaultInterval;

        public Form1()
        {
            InitializeComponent();
            var dir = new DirectoryInfo(@"..\data");
            var files = dir.GetFiles("*.csv").Select(str => str.ToString());
            foreach (var f in files) listBox1.Items.Add(f);
            //_selected.X = 50;
            //_selected.Width = 100;
            _selected = Tuple.Create(50, 100);

            chart1.ChartAreas.Add(new ChartArea("Global"));
            chart2.ChartAreas.Add(new ChartArea("Local"));

            chart1.Annotations.Add(new RectangleAnnotation
            {
                AllowAnchorMoving = false,
                LineColor = Color.Red,
                LineWidth = 1,
                BackColor = Color.Transparent,
                Name = "SelectedRange"
            });
            chart1.Annotations.Add(new VerticalLineAnnotation
            {
                //IsInfinitive = true,
                ClipToChartArea = chart1.ChartAreas[0].Name,
                AllowAnchorMoving = false,
                LineColor = Color.Red,
                LineWidth = 1,
                Name = "Global"
            });
            chart1.Annotations.Add(new VerticalLineAnnotation
            {
                IsInfinitive = true,
                AllowAnchorMoving = false,
                LineColor = Color.Red,
                LineWidth = 1,
                Name = "Local"
            });

            timer1.Start();
            timer2 = new System.Timers.Timer();
            timer2.Elapsed += timer2_Tick;
            _defaultInterval = timer2.Interval;
        }

        //private void AdjustSelectedRangeToAxisY()
        //{
        //    var axisY = chart1.ChartAreas[0].AxisY;
        //    var y1 = axisY.ValueToPixelPosition(axisY.Minimum);
        //    var y2 = axisY.ValueToPixelPosition(axisY.Maximum);
        //    _selected.Y = Math.Min(y1, y2);
        //    _selected.Height = Math.Abs(y1 - y2);
        //}

        //// 後ほど LineAnnotationを加える処理に書き換える
        //private void DrawDataScanner()
        //{
        //    if (_dataReader == null) return;
        //    chart1.ChartAreas[0].RecalculateAxesScale();
        //    var x = (int)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(_dataReader.Current.XValue);
        //    var y1 = (int)_selected.Top;
        //    var y2 = (int)_selected.Bottom;
        //}

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // イベントは非同期に処理されている？ので
            // データを読み込んでいるときは `_dataLoadCompleted` を false にする必要がある。
            _dataLoadCompleted = false;

            var path = Path.Combine(@"..\data", listBox1.SelectedItem.ToString());
            var legend = listBox1.SelectedItem.ToString().Split('.')[0];
            chart1.Series.Clear();
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Line;
            chart1.Series[legend].XValueType = ChartValueType.DateTime;
            chart2.Series.Clear();
            chart2.Series.Add(legend);
            chart2.Series[legend].ChartType = SeriesChartType.Line;
            chart2.Series[legend].XValueType = ChartValueType.DateTime;

            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine().Split(',');
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var dt = DateTime.ParseExact(line[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    var value = double.Parse(line[1]);
                    chart1.Series[legend].Points.AddXY(dt, value);
                }
            }
            _dataLoadCompleted = true;

            // なぜかchart1そのものの左端になってしまう
            //chart1.ChartAreas[0].RecalculateAxesScale();
            //var axisX = chart1.ChartAreas[0].AxisX;
            //_selected.X = axisX.ValueToPixelPosition(chart1.Series[0].Points[0].XValue);

            textBox1.Clear();

            _dataReader = new DataReader(chart1.Series[0]);

            // RectangleAnnotation
            chart1.Annotations[0].AxisX = chart1.ChartAreas[0].AxisX;
            chart1.Annotations[0].AxisY = chart1.ChartAreas[0].AxisY;
            chart1.Annotations[0].AnchorDataPoint = _dataReader.First;
            // GlobalCurrent
            chart1.Annotations[1].AxisX = chart1.ChartAreas[0].AxisX;
            chart1.Annotations[1].AxisY = chart1.ChartAreas[0].AxisY;
            chart1.Annotations[1].AnchorX = chart1.Series[0].Points[0].XValue;
            chart1.Annotations[1].AnchorY = chart1.ChartAreas[0].AxisY.Minimum;
            chart1.Annotations[1].SmartLabelStyle.Enabled = false;
            chart1.Annotations[1].Height = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
            chart1.Annotations[1].IsSizeAlwaysRelative = false;
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            _fixed = !_fixed;
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (_fixed) return;

            //chart1.ChartAreas[0].RecalculateAxesScale();
            //var axisX = chart1.ChartAreas[0].AxisX;
            //var pointedX = axisX.PixelPositionToValue(chart1.PointToClient(MousePosition).X);
            //if (pointedX > axisX.Maximum) pointedX = axisX.Maximum;
            //if (pointedX < axisX.Minimum) pointedX = axisX.Minimum;
            //textBox1.WriteLineBefore("X: " + pointedX);

            //chart1.Annotations[0].AnchorX = pointedX;
            //chart1.Annotations[0].Right = pointedX + 10;
            //chart1.Annotations[0].SetAnchor(new DataPoint(pointedX, chart1.ChartAreas[0].AxisY.Minimum),
            //                                new DataPoint(pointedX + 10, chart1.ChartAreas[0].AxisY.Maximum));
            //chart1.Annotations[0].AnchorY = chart1.ChartAreas[0].AxisY.Maximum;
            //chart1.Annotations[0].Bottom = chart1.ChartAreas[0].AxisY.Minimum;
            //chart1.Annotations[0].Width = 13;
            //chart1.Annotations[0].Height = chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum;
            //chart1.Annotations[0].IsSizeAlwaysRelative = false;
            //chart1.Annotations[0].SetAnchor(_dataReader.Current, _dataReader.Last);
            //var ann = chart1.Annotations[0];
            //textBox1.WriteLineBefore("X: " + ann.AnchorX + ", Y:" + ann.AnchorY + ", W:" + ann.Width + ", H:" + ann.Height);

            //_selected.X = mouseX - _selected.Width / 2;
            //if (_selected.Right > maxX) _selected.X = maxX - _selected.Width;
            //if (_selected.X < minX) _selected.X = minX;

        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_fixed) return;
            var delta = e.Delta * SystemInformation.MouseWheelScrollLines / 60.0;
            //_selected.Width += delta;
            //_selected.X -= delta / 2;
            //if (_selected.Width < 1) _selected.Width = 1;
        }

        private void chart1_MouseEnter(object sender, EventArgs e)
        {
            chart1.Focus();
        }

        /// <summary>
        /// next button
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (_dataReader == null || _dataReader.EndOfStream) return;
            var point = _dataReader.Next;
            textBox1.WriteLineBefore("[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]");
        }

        /// <summary>
        /// prev button
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (_dataReader == null || _dataReader.StartOfStream) return;
            var point = _dataReader.Prev;
            textBox1.WriteLineBefore("[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_dataLoadCompleted) return;
            //AdjustSelectedRangeToAxisY();
            //DrawDataScanner();

            // 上の Chart の選択範囲に応じて下の Chart のデータを更新。
            if (_fixed) return;
            //chart1.ChartAreas[0].RecalculateAxesScale();
            //var axisX = chart1.ChartAreas[0].AxisX;
            //var left = axisX.PixelPositionToValue(_selected.X);
            //var right = axisX.PixelPositionToValue(_selected.Right);
            //chart2.Series[0].Points.Clear();
            //foreach (var point in chart1.Series[0].Points)
            //{
            //    if (point.XValue > left && point.XValue < right)
            //    {
            //        chart2.Series[0].Points.Add(point);
            //    }
            //}
            //chart2.ChartAreas[0].AxisY.Maximum = chart1.ChartAreas[0].AxisY.Maximum;
            //chart2.ChartAreas[0].AxisY.Minimum = chart1.ChartAreas[0].AxisY.Minimum;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_dataReader == null || _dataReader.EndOfStream) return;
            var point = _dataReader.Next;
            chart1.Invoke((Action)(() => { chart1.Annotations[1].AnchorX = point.XValue; }));

            if (!checkBox3.Checked) return;
            var date = DateTime.FromOADate(point.XValue);
            textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore("[" + date + ", " + point.YValues[0] + "]"); }));


            // 選択範囲をついていかせる処理
            if (!checkBox1.Checked) return;
            //var axisX = chart1.ChartAreas[0].AxisX;
            //var x = axisX.ValueToPixelPosition(_dataReader.Current.XValue) - _selected.Width / 2;
            //if (x > _selected.X) _selected.X = x;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.WriteLineBefore("trace " + (checkBox1.Checked ? "ON" : "OFF"));
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                timer2.Start();
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                timer2.Stop();
                button1.Enabled = true;
                button2.Enabled = true;
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
