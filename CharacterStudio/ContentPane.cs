using ParagonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio
{
    public class ContentPane : UserControl
    {
        public Workspace CurrentWorkspace { get { return (this.ParentForm as PrimaryForm).CurrentWorkspace; } }

        public void DisplayPanel<PanelType>() where PanelType : ContentPane, new()
        {
            (this.ParentForm as PrimaryForm).DisplayPanel<PanelType>();
        }

        public override DockStyle Dock
        {
            get
            {
                return DockStyle.Fill;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
