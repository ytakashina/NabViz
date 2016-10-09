using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz
{
    static class ComboBoxFactory
    {
        private static int _index;

        public static ComboBox Create()
        {
            var box = new ComboBox();
            for (int i = 0; i < 9; i++)
            {
                box.Items.Add((MarkerStyle)(i%9 + 1));
            }
            box.SelectedIndex = _index++ % 9;
            return box;
        }
    }
}
