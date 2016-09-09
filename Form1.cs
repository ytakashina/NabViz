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
        private Graphics _graphics;
        private RectangleD _selectedRange;
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
            pictureBox1.Parent = chart1;

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
            _selectedRange.X = 50;
            _selectedRange.Width = 100;

            timer1.Start();
            timer2 = new System.Timers.Timer();
            timer2.Elapsed += timer2_Tick;
            _defaultInterval = timer2.Interval;
        }

        private void AdjustSelectedRangeToAxisY()
        {
            var axisY = chart1.ChartAreas[0].AxisY;
            var y1 = axisY.ValueToPixelPosition(axisY.Minimum);
            var y2 = axisY.ValueToPixelPosition(axisY.Maximum);
            _selectedRange.Y = Math.Min(y1, y2);
            _selectedRange.Height = Math.Abs(y1 - y2);
        }

        private void DrawSelectedRange()
        {
            _graphics.Clear(Color.Transparent);
            _graphics.DrawRectangle(_fixed ? Pens.Red : Pens.Orange, (Rectangle)_selectedRange);
        }

        private void DrawDataScanner()
        {
            if (_dataReader == null) return;
            var x = (int)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(_dataReader.Current.XValue);
            var y1 = (int)_selectedRange.Top;
            var y2 = (int)_selectedRange.Bottom;
            _graphics.DrawLine(Pens.Red, new Point(x, y1), new Point(x, y2));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // イベントは非同期に処理されている？ので
            // データを読み込んでいるときは `_dataLoadCompleted` を false にする必要がある。
            _dataLoadCompleted = false;

            var path = Path.Combine(@"..\data", listBox1.SelectedItem.ToString());
            var legend = listBox1.SelectedItem.ToString().Split('.')[0];
            chart1.Initialize();
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Line;
            chart1.Series[legend].XValueType = ChartValueType.DateTime;
            chart2.Initialize();
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
                    chart1.AddPoint(Tuple.Create(dt, value), legend);
                }
            }
            _dataLoadCompleted = true;

            // なぜかchart1そのものの左端になってしまう
            //chart1.ChartAreas[0].RecalculateAxesScale();
            //var axisX = chart1.ChartAreas[0].AxisX;
            //_selectedRange.X = axisX.ValueToPixelPosition(chart1.Series[0].Points[0].XValue);

            textBox1.Clear();

            _dataReader = new DataReader(chart1.Series[0]);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _fixed = !_fixed;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            if (_fixed) return;

            var axisX = chart1.ChartAreas[0].AxisX;
            var minX = axisX.ValueToPixelPosition(axisX.Minimum);
            var maxX = axisX.ValueToPixelPosition(axisX.Maximum);
            var mouseX = pictureBox1.PointToClient(MousePosition).X;

            _selectedRange.X = mouseX - _selectedRange.Width / 2;
            if (_selectedRange.Right > maxX) _selectedRange.X = maxX - _selectedRange.Width;
            if (_selectedRange.X < minX) _selectedRange.X = minX;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_fixed) return;
            var delta = e.Delta * SystemInformation.MouseWheelScrollLines / 60.0;
            _selectedRange.Width += delta;
            _selectedRange.X -= delta / 2;
            if (_selectedRange.Width < 1) _selectedRange.Width = 1;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
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
            AdjustSelectedRangeToAxisY();
            DrawSelectedRange();
            DrawDataScanner();
            pictureBox1.Refresh();

            // 上の Chart の選択範囲に応じて下の Chart のデータを更新。
            if (_fixed) return;
            chart1.ChartAreas[0].RecalculateAxesScale();
            var axisX = chart1.ChartAreas[0].AxisX;
            var left = axisX.PixelPositionToValue(_selectedRange.X);
            var right = axisX.PixelPositionToValue(_selectedRange.Right);
            //Console.WriteLine("[" + DateTime.FromOADate(left) + ", " + DateTime.FromOADate(right) + "]");
            chart2.Series[0].Points.Clear();
            foreach (var point in chart1.Series[0].Points)
            {
                if (point.XValue > left && point.XValue < right)
                {
                    chart2.Series[0].Points.Add(new DataPoint(point.XValue, point.YValues));
                }
            }
            chart2.ChartAreas[0].AxisY.Maximum = chart1.ChartAreas[0].AxisY.Maximum;
            chart2.ChartAreas[0].AxisY.Minimum = chart1.ChartAreas[0].AxisY.Minimum;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_dataReader == null || _dataReader.EndOfStream) return;
            var point = _dataReader.Next;

            // log
            if (checkBox3.Checked)
            {
                var str = "[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]";
                textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore(str); }));
            }

            // trace
            if (!checkBox1.Checked) return;
            var axisX = chart1.ChartAreas[0].AxisX;
            var x = axisX.ValueToPixelPosition(_dataReader.Current.XValue) - _selectedRange.Width / 2;
            if (x > _selectedRange.X) _selectedRange.X = x;
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
