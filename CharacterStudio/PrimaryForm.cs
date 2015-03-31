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
            LoadedPanels[typeof(PanelType)].Dock = DockStyle.Fill;
            this.PerformLayout();
        }

        private void Load_Click(object sender, EventArgs e)
        {
            DisplayPanel<LoadCharacterPane>();
        }

        private void New_Click(object sender, EventArgs e)
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

        private void Save_Click(object sender, EventArgs e)
        {
            if (CurrentWorkspace.CharacterRef == null)
                MessageBox.Show("No character Loaded!");
            else
                CurrentWorkspace.CharacterRef.Save("test.D20Character");
        }
    }
}
