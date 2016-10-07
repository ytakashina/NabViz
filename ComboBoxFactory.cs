using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NabViz
{
    class ComboBoxFactory
    {
        private static int _index;
        private static ComboBoxFactory _instance = new ComboBoxFactory();

        private ComboBoxFactory() { }

        public ComboBox GetComboBox()
        {
            var box = new ComboBox();
            for (int i = 0; i < 9; i++)
            {
                box.Items.Add((MarkerStyle)(i%9 + 1));
            }
            box.SelectedIndex = _index++;
            return box;
        }

        public static ComboBoxFactory Instance => _instance ?? (_instance = new ComboBoxFactory());
    }
}
