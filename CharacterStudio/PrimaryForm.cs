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
            DisplayPanel<HomePane>();
            this.HelpButton = true;
            this.tabControl1.Controls.Clear();
            this.tabControl1.Controls.Add(this.homeTab);
            this.tabControl1.Controls.Add(this.newCharTab);
            this.tabControl1.Controls.Add(this.loadCharTab);
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
            tabControl1.Controls.Clear();
            tabControl1.Controls.Add(buildTab);
            tabControl1.Controls.Add(shopTab);
            tabControl1.Controls.Add(adventureTab);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (CurrentWorkspace.CharacterRef == null)
                MessageBox.Show("No character Loaded!");
            else
                CurrentWorkspace.CharacterRef.Save(CurrentWorkspace.CharacterRef.Name + ".D20Character");
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPage.Name)
            {
                case "homeTab":
                    DisplayPanel<HomePane>();
                    break;
                case "newCharTab":
                    DisplayPanel<NewCharacterPane>();
                    break;
                case "loadCharTab":
                    DisplayPanel<LoadCharacterPane>();
                    break;
                default:
                    break;
            }
        }
    }
}
