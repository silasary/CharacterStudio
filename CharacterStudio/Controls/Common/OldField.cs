using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio.Controls.Common
{
    class OldField : Panel
    {
        public Label label { get; set; }
        public TextBox text_box { get; set; }

        public OldField()
            : base()
        {
            AutoSize = true;

            label = new Label();
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Dock = DockStyle.Left;
            label.Top = this.Height / 4;

            Controls.Add(label);

            text_box = new TextBox();
            text_box.Anchor = AnchorStyles.Right;
            text_box.Dock = DockStyle.Right;
            text_box.Top = this.Height / 4;
            Controls.Add(text_box);
        }

        [Description("Label for the textbox")]
        public string Label { get { return label.Text; } set { label.Text = value; } }

        public string Text { get { return text_box.Text; } set { text_box.Text = value; } }
    }
}
