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
using CharacterStudio.Controls.Common;
using ParagonLib;

namespace CharacterStudio.Controls.Panes
{
    public partial class LoadCharacterPane : ContentPane
    {
        struct Folder
        {
            public string path;
            public string Name;
            public Folder(string path)
            {
                this.path = path;
                Name = Path.GetFileName(path);
            }
            public override string ToString()
            {
                return Name;
            }
            public static implicit operator string(Folder f)
            {
                return f.path;
            }
        }

        public LoadCharacterPane()
        {
            InitializeComponent();
            checkedListBox1.Items.Add(new Folder("(Recent)"),true);
            checkedListBox1.Items.AddRange(Serializer.KnownFolders.Select<string,object>(f => new Folder(f)).ToArray());
        }

        private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            if (checkedListBox1.SelectedItem != null)
            {
                var folder = (Folder)checkedListBox1.SelectedItem;
                IEnumerable<string> chars;
                if (folder == "(Recent)")
                    chars = Serializer.KnownFiles;
                else
                    chars = Directory.GetFiles(folder.path, "*.dnd4e").Union(Directory.GetFiles(folder.path, "*.D20Character"));

                //listView1.Items.AddRange(chars.Select(c => new ListViewItem(c)).ToArray());
                flowLayoutPanel1.Controls.AddRange(chars.Select(c => new SimpleCharacterVis(c)).ToArray());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var browse = new System.Windows.Forms.OpenFileDialog();
            browse.Filter = "Character Builder Files|*.dnd4e;*.D20Character";
            browse.ShowDialog();
        }


                //private void listView1_DoubleClick(object sender, EventArgs e)
        //{
        //    if (listView1.SelectedItems.Count > 0)
        //    {
        //        new ParagonLib.Serializer().Load(listView1.SelectedItems[0].Text); 

        //    }
        //}
    }
}