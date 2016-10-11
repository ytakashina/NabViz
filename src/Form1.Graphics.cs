using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace NabViz
{
    public partial class Form1
    {
        private void InitializePictureBox()
        {
            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);
            bmp.MakeTransparent(Color.White);
            _graphics = Graphics.FromImage(bmp);
            pictureBox1.BackgroundImage = bmp;
            pictureBox1.Parent = chart1;
        }

        private void InitializeSelection()
        {
            var axisX = chart1.ChartAreas[UpperChartArea].AxisX;
            _selection.X = (float) axisX.ValueToPixelPosition(axisX.Minimum);
            _selection.Width = 100;
        }

        /// <summary>
        /// Adjust the selection range to the current ChartArea[UpperChartArea].
        /// Need refreshing the chart before using `ValueToPixelPosition`.
        /// </summary>
        private void AdjustSelection()
        {
            if (treeView1.SelectedNode.Text.Split('.').Last() != "csv") return;

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

        private void DrawSelection()
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
            foreach (var window in AnomalyLabels.Dictionary[treeView1.SelectedNode.FullPath])
            {
                var minX = (int) axisX.ValueToPixelPosition(window.Item1.ToOADate());
                var maxX = (int) axisX.ValueToPixelPosition(window.Item2.ToOADate());
                _graphics.FillRectangle(_windowBrush, minX, maxY, maxX - minX, minY - maxY);
            }
        }
    }
}
