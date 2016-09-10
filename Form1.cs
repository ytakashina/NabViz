using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using RichForms;

namespace ZetaOne
{
    public partial class Form1 : Form
    {
        private const string UpperChartAreaName = "Global";
        private const string LowerChartAreaName = "Local";
        private string _fileName;
        private Graphics _graphics;
        private Brush _windowBrush;
        private RectangleD _selection;
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

            chart1.ChartAreas.Add(new ChartArea(UpperChartAreaName));
            chart1.ChartAreas.Add(new ChartArea(LowerChartAreaName));
            chart1.ChartAreas[UpperChartAreaName].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            chart1.ChartAreas[LowerChartAreaName].AxisX.LabelStyle.Format = "M/d\nhh:mm";
            InitializeInnerPlotPosition(UpperChartAreaName);
            InitializeInnerPlotPosition(LowerChartAreaName);

            chart1.Series.Add(new Series
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                ChartArea = UpperChartAreaName,
                Color = Color.CornflowerBlue
            });
            chart1.Series.Add(new Series
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                ChartArea = LowerChartAreaName,
                Color = Color.CornflowerBlue
            });
            _selection.Width = 100;
        }

        private void InitializeInnerPlotPosition(string name)
        {
            var inner = chart1.ChartAreas[name].InnerPlotPosition;
            inner.Width = 92;
            inner.Height = 90;
            inner.X = 6;
            inner.Y = 0;
        }

        private void InitializeSelection()
        {
            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
            _selection.X = axisX.ValueToPixelPosition(axisX.Minimum);
        }

        /// <summary>
        /// Chart 更新後にこのメソッドするには chart1.Refresh() などで一度 Chart を再描画する必要がある。
        /// クソだけど Windows.Forms の Chart の仕様なので仕方ない。
        /// </summary>
        private void AdjustSelection()
        {
            var axisY = chart1.ChartAreas[UpperChartAreaName].AxisY;
            _selection.Y = axisY.ValueToPixelPosition(axisY.Maximum);
            _selection.Height = axisY.ValueToPixelPosition(axisY.Minimum) - _selection.Y;

            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
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
            if (_dataReader == null || _dataReader.EndOfStream) return;
            var axisX = chart1.ChartAreas[name].AxisX;
            var axisY = chart1.ChartAreas[name].AxisY;
            var x = (int)axisX.ValueToPixelPosition(_dataReader.Current.XValue);
            var minY = (int)axisY.ValueToPixelPosition(axisY.Minimum);
            var maxY = (int)axisY.ValueToPixelPosition(axisY.Maximum);
            _graphics.DrawLine(Pens.Red, new Point(x, minY), new Point(x, maxY));
        }

        private void DrawAnomaryWindow(string name)
        {
            if (_fileName == null) return;
            var axisX = chart1.ChartAreas[name].AxisX;
            var axisY = chart1.ChartAreas[name].AxisY;
            var minY = (int)axisY.ValueToPixelPosition(axisY.Minimum);
            var maxY = (int)axisY.ValueToPixelPosition(axisY.Maximum);
            foreach (var window in AnomalyLabels.Instance[_fileName])
            {
                var minX = (int)axisX.ValueToPixelPosition(window.Item1.ToOADate());
                var maxX = (int)axisX.ValueToPixelPosition(window.Item2.ToOADate());
                _graphics.FillRectangle(_windowBrush, minX, maxY, maxX - minX, minY - maxY);
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // イベント駆動は実行時の順序保証がないので、
            // 明示的にデータの読み込みの終了を管理する必要がある。
            _dataLoadCompleted = false;

            var path = Path.Combine(@"..\data", listBox1.SelectedItem.ToString());
            _fileName = listBox1.SelectedItem.ToString();

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine().Split(',');
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var dt = DateTime.ParseExact(line[0], "yyyy-MM-dd HH:mm:ss", null);
                    var value = double.Parse(line[1]);
                    chart1.Series[0].Points.AddXY(dt, value);
                    chart1.Series[1].Points.AddXY(dt, value);
                }
            }
            _dataReader = new DataReader(chart1.Series[0]);
            _dataLoadCompleted = true;

            // Chart の仕様上、一度描画されないと ValueToPixelPosition が使えないらしい。
            // RecalculateAxesScale でなんとかならなかった。
            // chart1.ChartAreas[UpperChartAreaName].RecalculateAxesScale();
            chart1.Refresh();

            // 下の ChartArea[1] の Y 軸方向のスケールを、上の ChartArea[0] に合わせる。
            var axisY = chart1.ChartAreas[UpperChartAreaName].AxisY;
            chart1.ChartAreas[LowerChartAreaName].AxisY.Minimum = axisY.Minimum;
            chart1.ChartAreas[LowerChartAreaName].AxisY.Maximum = axisY.Maximum;

            // 選択範囲を現在の ChartArea[0] に合わせる。
            InitializeSelection();
            AdjustSelection();

            textBox1.Clear();
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

            var axisY = chart1.ChartAreas[UpperChartAreaName].AxisY;
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
            chart1.ChartAreas[LowerChartAreaName].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            AdjustSelection();
            chart1.ChartAreas[LowerChartAreaName].AxisX.IntervalAutoMode = IntervalAutoMode.FixedCount;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;
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

            // 上の Chart の選択範囲に応じて下の Chart のデータを更新。
            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
            var minX = axisX.PixelPositionToValue(_selection.X);
            var maxX = axisX.PixelPositionToValue(_selection.Right);
            // 応急処置的。_selection が chartArea[0] をはみ出しても落ちなくするため。
            // そもそも _selection が絶対はみ出さないように作るほうが望ましい。
            if (minX < _dataReader.First.XValue) minX = _dataReader.First.XValue;
            if (maxX > _dataReader.Last.XValue) maxX = _dataReader.Last.XValue;
            chart1.ChartAreas[LowerChartAreaName].AxisX.Minimum = minX;
            chart1.ChartAreas[LowerChartAreaName].AxisX.Maximum = maxX;

            _graphics.Clear(Color.Transparent);
            DrawSelectedRange();
            DrawDataScanner(UpperChartAreaName);
            DrawDataScanner(LowerChartAreaName);
            DrawAnomaryWindow(UpperChartAreaName);
            DrawAnomaryWindow(LowerChartAreaName);
            pictureBox1.Refresh();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_dataReader == null) return;
            if (_dataReader.EndOfStream)
            {
                textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore("[I] Reached EOS."); }));
                checkBox2.Invoke((Action)(() => { checkBox2.Checked = false; }));
                _dataReader.Rewind();
                InitializeSelection();
                return;
            }

            var point = _dataReader.Next;

            // trace
            if (checkBox1.Checked)
            {
                var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
                var x = axisX.ValueToPixelPosition(_dataReader.Current.XValue) - _selection.Width / 2;
                if (x > _selection.X) _selection.X = x;
                var maxX = axisX.ValueToPixelPosition(axisX.Maximum);
                if (_selection.Right > maxX) _selection.X = maxX - _selection.Width;
            }

            // log
            if (checkBox3.Checked)
            {
                var str = "[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]";
                textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore(str); }));
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.WriteLineBefore("[I] Trace: " + (checkBox1.Checked ? "ON" : "OFF"));
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (!_dataLoadCompleted)
                {
                    textBox1.WriteLineBefore("[E] Data is not defined.");
                    checkBox2.Checked = false;
                    return;
                }
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.WriteLineBefore("[I] Logging: " + (checkBox3.Checked ? "ON" : "OFF"));
            if (checkBox3.Checked) textBox1.WriteLineBefore("[W] The Application will crash when logging under high speed, especially over x4.");
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
