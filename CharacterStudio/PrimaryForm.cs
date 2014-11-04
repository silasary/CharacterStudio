using CharacterStudio.Controls.Panes;
using ParagonLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio
{
    public partial class PrimaryForm : Form
    {
        public PrimaryForm()
        {
            InitializeComponent();
            CurrentWorkspace = new Workspace(null,null);
            DisplayPanel<DetailsPane>();
            this.HelpButton = true;
        }

        public ParagonLib.Workspace CurrentWorkspace { get; set; }

        Dictionary<Type, ContentPane> LoadedPanels = new Dictionary<Type, ContentPane>();

        public void DisplayPanel<PanelType>() where PanelType : ContentPane, new()
        {
            if (!LoadedPanels.ContainsKey(typeof(PanelType)))
                LoadedPanels.Add(typeof(PanelType), new PanelType() { PrimaryForm = this });
            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(LoadedPanels[typeof(PanelType)]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisplayPanel<LoadCharacterPane>();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DisplayPanel<NewCharacterPane>();
        }
        
        internal void LoadCharacter(Character Char)
        {
            this.CurrentWorkspace = Char.workspace;
            this.Text = string.Format("Character Studio [{0}]", Char.workspace.System);
            foreach (var item in LoadedPanels.Values)
            {
                item.OnCharacterLoad();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CurrentWorkspace.CharacterRef.Save("test.D20Character");
        }
    }
}
