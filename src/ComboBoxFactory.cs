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
            for (int i = 0; i < 10; i++)
            {
                box.Items.Add((MarkerStyle)i);
            }

            box.SelectedIndex = _index++ % 9 + 1;
            return box;
        }
    }
}
