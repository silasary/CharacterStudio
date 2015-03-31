using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio.Controls.Panes
{
    public partial class DetailsPane : ContentPane
    {
        public DetailsPane()
        {
            InitializeComponent();
            Enabled = false;
        }

        public override void OnCharacterLoad()
        {
            //charNameField.Text = CurrentWorkspace.Character.Name;
            XPLabel.Text = string.Format("Level {0} ({1} XP)", CurrentWorkspace.Level, CurrentWorkspace.GetStat("XP Earned").Value);
            this.charNameField.Text = CurrentWorkspace.CharacterRef.Name;
            this.playerNameField.Text = CurrentWorkspace.CharacterRef.Player;

            base.OnCharacterLoad();
            this.Enabled = true;
        }

        private void LevelUpButton_Click(object sender, EventArgs e)
        {

        }

        
    }
}
