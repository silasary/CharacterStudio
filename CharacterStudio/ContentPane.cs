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
        public PrimaryForm PrimaryForm { get; set; }

        public Workspace CurrentWorkspace { get {
            if (PrimaryForm == null)
                return null;
            return PrimaryForm.CurrentWorkspace; } }

        public void DisplayPanel<PanelType>() where PanelType : ContentPane, new()
        {
            PrimaryForm.DisplayPanel<PanelType>();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode)
                return;
            if (CurrentWorkspace != null && CurrentWorkspace.CharacterRef != null)
                OnCharacterLoad();
        }

        protected void LoadCharacter(Character Char)
        {
            PrimaryForm.LoadCharacter(Char);
        }

        public virtual void OnCharacterLoad()
        {

        }
        public virtual void OnCharacterUpdated()
        {

        }

        //public override DockStyle Dock
        //{
        //    get
        //    {
                
        //        return DockStyle.Fill;
        //    }
        //    set
        //    {
        //        base.Dock = value;
        //    }
        //}
    }
}
