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
        Control[] MenuTabs, HomeTabs, BuildTabs, AdventureTabs, ShoppingTabs;
        public PrimaryForm()
        {
            InitializeComponent();
            CurrentWorkspace = new Workspace(null,null);
            this.LoadedPanels.Add(typeof(LeftSidebar), this.leftSidebar1);
            DisplayPanel<HomePane>();
            this.HelpButton = true;
            MenuTabs = new Control[] { this.homeTab, this.newCharTab, this.loadCharTab };
            HomeTabs = new Control[] { this.charDetailsTab, this.buildTab, this.shopTab, this.adventureTab };

            this.tabControl1.Controls.Clear();
            this.tabControl1.Controls.AddRange(MenuTabs);
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
            this.SetTab(typeof(PanelType));
            this.PerformLayout();
        }

        private void SetTab(Type type)
        {
            foreach (var c in this.tabControl1.Controls)
            {
                if ((Type)(c as Control).Tag == type)
                    tabControl1.SelectedTab = c as TabPage;
            }
            if (type == typeof(DetailsPane))
            {
                tabControl1.Controls.Clear();
                tabControl1.Controls.AddRange(HomeTabs);
            }
        }

        private void DisplayPanel(Type t) // This must be private for it to work.
        {
            var method = typeof(PrimaryForm).GetMethod("DisplayPanel");
            var genericMethod = method.MakeGenericMethod(t);
            genericMethod.Invoke(this, null);
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
                CurrentWorkspace.CharacterRef.Save(CurrentWorkspace.CharacterRef.Name + ".D20Character");
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == null)
                return;
            if (e.TabPage.Tag is Type && (e.TabPage.Tag as Type).IsSubclassOf(typeof(ContentPane)))
            {
                DisplayPanel(e.TabPage.Tag as Type);
                return;
            }
            switch (e.TabPage.Name)
            {
                default:
                    break;
            }
        }
    }
}
