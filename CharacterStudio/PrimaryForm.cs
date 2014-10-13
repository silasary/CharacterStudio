using CharacterStudio.Controls.Panes;
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
            DisplayPanel<DetailsPane>();
        }

        public ParagonLib.Workspace CurrentWorkspace { get; set; }

        Dictionary<Type, ContentPane> LoadedPanels = new Dictionary<Type, ContentPane>();

        public void DisplayPanel<PanelType>() where PanelType : ContentPane, new()
        {
            if (!LoadedPanels.ContainsKey(typeof(PanelType)))
                LoadedPanels.Add(typeof(PanelType), new PanelType());
            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(LoadedPanels[typeof(PanelType)]);
        }
    }
}
