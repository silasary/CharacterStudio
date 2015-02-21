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
            checkedListBox1.Items.Add(new Folder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Saved Characters")),true);
            
        }

        private void checkedListBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedItem == null)
            {
                listView1.Items.Clear();
            }
            else
            {
                var folder = (Folder)checkedListBox1.SelectedItem;
                var chars = Directory.GetFiles(folder.path);
                listView1.Items.Clear();
                listView1.Items.AddRange(chars.Select(c => new ListViewItem(c)).ToArray());
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                new ParagonLib.Serializer().Load(listView1.SelectedItems[0].Text); 

            }
        }
    }
}
