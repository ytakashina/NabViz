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
        private bool _dataIsDefined;
        
        public Form1()
        {
            InitializeComponent();
            var dir = new DirectoryInfo(@"..\data");
            var files = dir.GetFiles().Select(str => str.ToString());
            foreach (var f in files) listBox1.Items.Add(f);
            pictureBox1.Parent = chart1;

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
            _selectedRange.X = 100;
            _selectedRange.Width = 100;

            timer1.Start();
        }

        private void AdjustSelectedRange()
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
            pictureBox1.Refresh();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // イベントは非同期に処理されている？ので
            // データを読み込んでいるときは `_dataIsDefined` を false にする必要がある。
            _dataIsDefined = false;

            var path = Path.Combine(@"..\data", listBox1.SelectedItem.ToString());
            var legend = listBox1.SelectedItem.ToString().Split('.')[0];
            chart1.Initialize();
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Line;
            chart2.Initialize();
            chart2.Series.Add(legend);
            chart2.Series[legend].ChartType = SeriesChartType.Line;

            using (var sr = new StreamReader(path))
            {
                var head = sr.ReadLine()?.Split(',');
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var dt = DateTime.ParseExact(line[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    var value = double.Parse(line[1]);
                    chart1.AddPoint(Tuple.Create(dt, value), legend);
                }
            }
            _dataIsDefined = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _fixed = !_fixed;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dataIsDefined) return;
            if (_fixed) return;

            var axisX = chart1.ChartAreas[0].AxisX;
            var x1 = axisX.ValueToPixelPosition(axisX.Minimum);
            var x2 = axisX.ValueToPixelPosition(axisX.Maximum);
            var minX = Math.Min(x1, x2);
            var maxX = Math.Max(x1, x2);
            var mouseX = pictureBox1.PointToClient(MousePosition).X;

            _selectedRange.X = mouseX - _selectedRange.Width / 2;
            if (_selectedRange.Right > maxX) _selectedRange.X = maxX - _selectedRange.Width;
            if (_selectedRange.X < minX) _selectedRange.X = minX;

            var left = axisX.PixelPositionToValue(_selectedRange.X);
            var right = axisX.PixelPositionToValue(_selectedRange.Right);
            Console.WriteLine("[" + DateTime.FromOADate(left) + ", " + DateTime.FromOADate(right) + "]");
            //for (var x = left; x < right;)
            //{
            //    chart2.AddPoint(Tuple.Create());
            //}



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
            var delta = e.Delta * SystemInformation.MouseWheelScrollLines / 60;
            _selectedRange.Width += delta;
            _selectedRange.X -= delta / 2;
            if (_selectedRange.Width < 1) _selectedRange.Width = 1;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!_dataIsDefined) return;
            AdjustSelectedRange();
            DrawSelectedRange();
        }
    }
}
