using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio.Controls.Common
{
    public partial class Field : UserControl
    {
        public Field()
        {
            InitializeComponent();
        }

        [Description("Label for the textbox")]
        public string Label { get { return label1.Text; } set { label1.Text = value; } }

        public string Text { get { return textBox1.Text; } set { textBox1.Text = value; } }

        private void Field_Resize(object sender, EventArgs e)
        {
            this.textBox1.Left = this.Width - textBox1.Width - 6    ;
        }
    }
}
