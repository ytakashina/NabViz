using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz
{
    public partial class Form1
    {
        private void InitializeTableLayoutPanel()
        {
            tableLayoutPanel1.Controls.Add(new Label { Text = "Detector" });
            tableLayoutPanel1.Controls.Add(new Label { Text = "Marker", Width = 70 });
            tableLayoutPanel1.Controls.Add(new Label { Text = "Threshold", Width = 60 });
            foreach (var detectorName in Detection.Detectors)
            {
                tableLayoutPanel1.Controls.Add(new Label { Text = detectorName });
                tableLayoutPanel1.Controls.Add(CreateDetectorComboBox(detectorName));
                tableLayoutPanel1.Controls.Add(CreateThresholdTextBox(detectorName));
            }
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
            box.Text = Detection.Thresholds[detectorName].ToString();
            box.TextChanged += thresholdBox_TextChanged;
            return box;
        }

        private void detectorBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
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
            Detection.Thresholds[detectorName] = tmp;
            LoadDetectionDataToChart();
        }
    }
}
