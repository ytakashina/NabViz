using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RichControls
{
    public static class RichTextBox
    {

        public static void Write(this TextBox self, string str="")
        {
            self.Lines = (self.Text + str).Split('\n');
        }

        public static void WriteLine(this TextBox self, string str="")
        {
            self.Lines = (self.Text + str + '\n').Split('\n');
        }

        public static void WriteBefore(this TextBox self, string str="")
        {
            self.Lines = (str + self.Text).Split('\n');
        }

        public static void WriteLineBefore(this TextBox self, string str="")
        {
            self.Lines = (str + '\n' + self.Text).Split('\n');
        }

    }
}
