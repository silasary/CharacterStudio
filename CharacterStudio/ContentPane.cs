using ParagonLib;
using ParagonLib.CharacterData;
using System.Collections.Generic;
using System.Linq;
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

        private PrimaryForm _primaryForm;
        public PrimaryForm PrimaryForm
        {
            get
            {
                if (_primaryForm == null)
                    _primaryForm = this.ParentForm as PrimaryForm;
                return _primaryForm;
            }
            set
            {
                this._primaryForm = value;
            }
        }

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
        public override void OnCharacterLoad()
        {
            base.OnCharacterLoad();
            foreach (var ctrl in this.GetAll<ContentControl>(this))
            {
                if (ctrl is ContentPane) // They get updated by themselves.
                    continue;
                (ctrl as ContentControl).OnCharacterLoad();
            }
            //this.OnCharacterUpdated();
        }

        public override void OnCharacterUpdated()
        {
            base.OnCharacterUpdated();
            foreach (var ctrl in this.GetAll<ContentControl>(this))
            {
                if (ctrl is ContentPane) // They get updated by themselves.
                    continue;
                (ctrl as ContentControl).OnCharacterUpdated();
            }
        }

        private IEnumerable<ControlType> GetAll<ControlType>(Control control) where ControlType : Control
        {
            var controls = (control as Control).Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll<ControlType>(ctrl).Cast<Control>())
                                      .Concat(controls)
                                      .Where(c => c.GetType() == typeof(ControlType))
                                      .Cast<ControlType>();
        }
    }
}