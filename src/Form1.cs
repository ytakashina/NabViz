using System;
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
        private RectangleF _selection;
        private bool _selectionFixed;
        private readonly DataReader _dataReader;
        private readonly Brush _windowBrush = new SolidBrush(Color.FromArgb(64, Color.Red));

        public Form1()
        {
            InitializeComponent();

            InitializeChart();
            InitializeTreeView();
            InitializePictureBox();
            InitializeTableLayoutPanel();

            _dataReader = new DataReader(chart1.Series[UpperChartArea]);

            timer1.Start();
        }

        private void InitializeTreeView()
        {
            treeView1.PathSeparator = Path.DirectorySeparatorChar.ToString();

            var rootDir = new DirectoryInfo(Path.Combine("..", "data"));
            foreach (var dir in rootDir.GetDirectories())
            {
                var node = new TreeNode(dir.ToString());
                var files = dir.GetFiles("*.csv");
                foreach (var f in files) node.Nodes.Add(f.ToString());
                treeView1.Nodes.Add(node);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;

            LoadRawDataToChart();
            LoadDetectionDataToChart();

            InitializeSelection();
            AdjustSelection();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            InitializePictureBox();
            // chart1.Refresh(); // necessary for the below `AdjustSelection()` to work
            AdjustSelection();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;
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
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;
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
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;

            // Update the lower chart's AxisX range corresponding to the upper chart's selection.
            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            var minX = axisX.PixelPositionToValue(_selection.X);
            var maxX = axisX.PixelPositionToValue(_selection.Right);
            chart1.ChartAreas[LowerChartArea].AxisX.Minimum = minX;
            chart1.ChartAreas[LowerChartArea].AxisX.Maximum = maxX;

            _graphics.Clear(Color.Transparent);
            DrawSelection();
            DrawAnomaryWindow(UpperChartArea);
            DrawAnomaryWindow(LowerChartArea);
            pictureBox1.Refresh();
        }
    }
}