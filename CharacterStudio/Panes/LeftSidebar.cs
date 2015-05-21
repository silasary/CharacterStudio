using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ParagonLib;

namespace CharacterStudio.Controls.Panes
{
    public partial class LeftSidebar : ContentPane
    {
        public LeftSidebar()
        {
            InitializeComponent();
        }

        public override void OnCharacterLoad()
        {
            //TODO: Race; Class.
            if (string.IsNullOrEmpty(CurrentWorkspace.CharacterRef.Portrait))
                this.pictureBox1.ImageLocation = Character.DefaultPortrait(CurrentWorkspace.CharacterRef.Class, "", ""); //CurrentWorkspace.CharacterRef.Race, CurrentWorkspace.CharacterRef.Gender);
            else
                this.pictureBox1.ImageLocation = CurrentWorkspace.CharacterRef.Portrait;
                CurrentWorkspace.CharacterRef.PropertyChanged += CharacterRef_PropertyChanged;
            base.OnCharacterLoad();
        }

        void CharacterRef_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Portrait":
                    this.pictureBox1.ImageLocation = CurrentWorkspace.CharacterRef.Portrait;
                    break;
            }
        }
    }
}
