using System;
using System.Drawing;
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
//        private Point _pos;
        private int _width;
        private bool _fixed;
        
        public Form1()
        {
            InitializeComponent();
            var dir = Directory.CreateDirectory("../data");
            var files = dir.GetFiles().Select(str => str.ToString());
            foreach (var f in files)
            {
                listBox1.Items.Add(f);
            }
            pictureBox1.Parent = chart1;

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;

            _width = 100;

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path = Path.Combine("../data", listBox1.SelectedItem.ToString());
            var legend = listBox1.SelectedItem.ToString().Split('.')[0];
            chart1.Initialize();
            //chart1.Legends.Add(legend);
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Line;

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
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            _fixed = !_fixed;
//            chart1_Click(sender, e);
        }

        private void DrawSelectedRange(float x, float y, float w, float h)
        {
            _graphics.Clear(Color.Transparent);
            _graphics.DrawRectangle(_fixed ? Pens.Red : Pens.Orange, x, y, w, h);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_fixed) return;
            var minAxisY = chart1.ChartAreas[0].AxisY.Minimum;
            var maxAxisY = chart1.ChartAreas[0].AxisY.Maximum;
            var y1 = (float)chart1.ChartAreas[0].AxisY.ValueToPixelPosition(minAxisY);
            var y2 = (float)chart1.ChartAreas[0].AxisY.ValueToPixelPosition(maxAxisY);

            var minAxisX = chart1.ChartAreas[0].AxisX.Minimum;
            var maxAxisX = chart1.ChartAreas[0].AxisX.Maximum;
            var x1 = (float)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(minAxisX);
            var x2 = (float)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(maxAxisX);

            var minY = Math.Min(y1, y2);
            var height = Math.Abs(y1 - y2);

            var minX = Math.Min(x1, x2);
            var maxX = Math.Max(x1, x2);

            var pointedX = pictureBox1.PointToClient(MousePosition).X;
            if (pointedX - _width / 2 < minX) pointedX = (int)minX + _width / 2;
            if (pointedX + _width / 2 > maxX) pointedX = (int)maxX - _width / 2 - 1;
            DrawSelectedRange(pointedX - _width / 2, minY, _width, height);
            pictureBox1.Refresh();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.MakeTransparent();
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
//            DrawSelectedRange();
//            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_fixed)
            {
                _width += e.Delta * SystemInformation.MouseWheelScrollLines / 60;
                if (_width < 1) _width = 1;
                if (_width >= pictureBox1.Width) _width = pictureBox1.Width - 1;
            }
//            DrawSelectedRange();
//            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

//        private void chart1_Click(object sender, EventArgs e)
//        {
////            var x = chart1.ChartAreas[0].AxisX.ValueToPixelPosition(0);
//            var y = chart1.ChartAreas[0].AxisY.ValueToPixelPosition(0);
//            var ymax = chart1.ChartAreas[0].AxisY.ValueToPixelPosition(100);
//
//            Console.WriteLine(y + " to " + ymax);
//
//        }
    }
}
