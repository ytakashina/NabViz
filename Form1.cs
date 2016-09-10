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
        private Graphics _graphics;
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
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
            pictureBox1.Parent = chart1;

            timer1.Start();
            timer2 = new System.Timers.Timer();
            timer2.Elapsed += timer2_Tick;
            _defaultInterval = timer2.Interval;

            chart1.ChartAreas.Add(new ChartArea(UpperChartAreaName));
            chart1.ChartAreas.Add(new ChartArea(LowerChartAreaName));
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

        /// <summary>
        /// このメソッドの実行前には chart1.Refresh() などで一度 Chart を再描画する必要がある。
        /// クソだけど Windows.Forms の Chart の仕様なので仕方ない。
        /// </summary>
        private void AdjustSelection()
        {
            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
            var axisY = chart1.ChartAreas[UpperChartAreaName].AxisY;
            _selection.X = axisX.ValueToPixelPosition(axisX.Minimum);
            _selection.Y = axisY.ValueToPixelPosition(axisY.Maximum);
            _selection.Height = axisY.ValueToPixelPosition(axisY.Minimum) - _selection.Y;
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // イベント駆動は実行時の順序保証がないので、
            // 明示的にデータの読み込みの終了を管理する必要がある。
            _dataLoadCompleted = false;

            var path = Path.Combine(@"..\data", listBox1.SelectedItem.ToString());
            var legend = listBox1.SelectedItem.ToString().Split('.')[0];

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();

            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine().Split(',');
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var dt = DateTime.ParseExact(line[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
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
            // InnerPlotPosition の二回目以降の呼び出しを禁止する。
            //chart1.ChartAreas[LowerChartAreaName].InnerPlotPosition.Auto = false;

            // 選択範囲を現在の ChartArea[0] に合わせる。
            AdjustSelection();

            textBox1.Clear();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dataLoadCompleted) return;
            // trace が ON だったら無視。
            if (checkBox1.Checked) return;

            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
            var axisY = chart1.ChartAreas[UpperChartAreaName].AxisY;
            var minX = axisX.ValueToPixelPosition(axisX.Minimum);
            var maxX = axisX.ValueToPixelPosition(axisX.Maximum);
            var minY = axisY.ValueToPixelPosition(axisY.Minimum);
            var mousePosition = chart1.PointToClient(MousePosition);

            if (mousePosition.Y > minY) return;

            _selection.X = mousePosition.X - _selection.Width / 2;
            if (_selection.Right > maxX) _selection.X = maxX - _selection.Width;
            if (_selection.X < minX) _selection.X = minX;
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

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked) return;
            var delta = e.Delta * SystemInformation.MouseWheelScrollLines / 60.0;
            _selection.Width += delta;
            _selection.X -= delta / 2;
            if (_selection.Width < 1) _selection.Width = 1;
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

            // 上の Chart の選択範囲に応じて下の Chart のデータを更新。
            var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
            var left = axisX.PixelPositionToValue(_selection.X);
            var right = axisX.PixelPositionToValue(_selection.Right);
            // 応急処置的。_selection が chartArea[0] をはみ出しても落ちなくするため。
            // そもそも _selection が絶対はみ出さないように作るほうが望ましい。
            if (left < _dataReader.First.XValue) left = _dataReader.First.XValue;
            if (right > _dataReader.Last.XValue) right = _dataReader.Last.XValue;
            chart1.ChartAreas[LowerChartAreaName].AxisX.Minimum = left;
            chart1.ChartAreas[LowerChartAreaName].AxisX.Maximum = right;

            _graphics.Clear(Color.Transparent);
            DrawSelectedRange();
            DrawDataScanner(UpperChartAreaName);
            DrawDataScanner(LowerChartAreaName);
            pictureBox1.Refresh();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (_dataReader == null || _dataReader.EndOfStream) return;
            var point = _dataReader.Next;

            // trace
            if (checkBox1.Checked)
            {
                var axisX = chart1.ChartAreas[UpperChartAreaName].AxisX;
                var x = axisX.ValueToPixelPosition(_dataReader.Current.XValue) - _selection.Width / 2;
                if (x > _selection.X) _selection.X = x;
            }

            // log
            if (checkBox3.Checked)
            {
                var str = "[" + DateTime.FromOADate(point.XValue) + ", " + point.YValues[0] + "]";
                textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore(str); }));
            }

            if (_dataReader.EndOfStream)
            {
                textBox1.Invoke((Action)(() => { textBox1.WriteLineBefore("[I] Reached EOF."); }));
                checkBox2.Invoke((Action)(() => { checkBox2.Checked = false; }));
                _dataReader.Rewind();
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.WriteLineBefore("[I] Logging: " + (checkBox3.Checked ? "ON" : "OFF"));
            if (checkBox3.Checked) textBox1.WriteLineBefore("[W] The Application will crash when logging under high speed, especially over x4.");
        }
    }
}
