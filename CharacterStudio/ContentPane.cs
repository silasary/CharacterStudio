using ParagonLib;
using System.Windows.Forms;

namespace CharacterStudio
{
    public class ContentControl : UserControl
    {
        public Workspace CurrentWorkspace
        {
            get
            {
                if (PrimaryForm == null)
                    return null;
                return PrimaryForm.CurrentWorkspace;
            }
        }

        public PrimaryForm PrimaryForm { get; set; }

        public void DisplayPanel<PanelType>() where PanelType : ContentPane, new()
        {
            PrimaryForm.DisplayPanel<PanelType>();
        }

        public virtual void OnCharacterLoad()
        {
        }

        public virtual void OnCharacterUpdated()
        {
        }

        protected void LoadCharacter(Character Char)
        {
            PrimaryForm.LoadCharacter(Char);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode)
                return;
            if (CurrentWorkspace != null && CurrentWorkspace.CharacterRef != null)
                OnCharacterLoad();
        }
    }

    public class ContentPane : ContentControl
    {
    }
}