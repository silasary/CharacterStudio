using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CharacterStudio.Controls.Panes
{
    public partial class HomePane : ContentPane
    {
        public HomePane()
        {
            InitializeComponent();
        }

        private void NewCharButton_Click(object sender, EventArgs e)
        {
            DisplayPanel<NewCharacterPane>();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DisplayPanel<LoadCharacterPane>();
        }

        private void HomePane_Load(object sender, EventArgs e)
        {
            var knownfiles = LoadCharacterPane.KnownFiles;
            flowLayoutPanel1.Controls.AddRange(knownfiles.Select(c => new Controls.Common.SimpleCharacterVis(c)).ToArray());

        }
    }
}
