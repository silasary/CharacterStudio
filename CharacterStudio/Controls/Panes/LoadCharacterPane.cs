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
            private string path;
            private string Name;
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
    }
}
