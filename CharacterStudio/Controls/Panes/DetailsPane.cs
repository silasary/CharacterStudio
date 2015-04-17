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
        public override void OnCharacterUpdated()
        {
            UpdateData();
            base.OnCharacterUpdated();
        }
        public override void OnCharacterLoad()
        {
            UpdateData();

            base.OnCharacterLoad();
            this.Enabled = true;
        }

        private void UpdateData()
        {
            XPLabel.Text = string.Format("Level {0} ({1} XP)", CurrentWorkspace.Level, CurrentWorkspace.GetStat("XP Earned").Value);
            this.charNameField.Text = CurrentWorkspace.CharacterRef.Name;
            this.playerNameField.Text = CurrentWorkspace.CharacterRef.Player;
        }        
    }
}
