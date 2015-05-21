namespace CharacterStudio.Controls.Panes
{
    partial class DetailsPane
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.XPLabel = new System.Windows.Forms.Label();
            this.DeityField = new CharacterStudio.Controls.Common.Field();
            this.playerNameField = new CharacterStudio.Controls.Common.Field();
            this.charNameField = new CharacterStudio.Controls.Common.Field();
            this.SuspendLayout();
            // 
            // XPLabel
            // 
            this.XPLabel.AutoSize = true;
            this.XPLabel.Location = new System.Drawing.Point(360, 18);
            this.XPLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.XPLabel.Name = "XPLabel";
            this.XPLabel.Size = new System.Drawing.Size(127, 20);
            this.XPLabel.TabIndex = 2;
            this.XPLabel.Text = "Level {0} ({1} XP)";
            // 
            // DeityField
            // 
            this.DeityField.AutoSize = true;
            this.DeityField.Label = "Deity";
            this.DeityField.Location = new System.Drawing.Point(4, 103);
            this.DeityField.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.DeityField.Name = "DeityField";
            this.DeityField.Size = new System.Drawing.Size(324, 42);
            this.DeityField.TabIndex = 0;
            // 
            // playerNameField
            // 
            this.playerNameField.AutoSize = true;
            this.playerNameField.Label = "Player Name";
            this.playerNameField.Location = new System.Drawing.Point(4, 54);
            this.playerNameField.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.playerNameField.Name = "playerNameField";
            this.playerNameField.Size = new System.Drawing.Size(324, 42);
            this.playerNameField.TabIndex = 0;
            // 
            // charNameField
            // 
            this.charNameField.AutoSize = true;
            this.charNameField.Label = "Character Name";
            this.charNameField.Location = new System.Drawing.Point(4, 5);
            this.charNameField.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.charNameField.Name = "charNameField";
            this.charNameField.Size = new System.Drawing.Size(324, 42);
            this.charNameField.TabIndex = 0;
            // 
            // DetailsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.XPLabel);
            this.Controls.Add(this.DeityField);
            this.Controls.Add(this.playerNameField);
            this.Controls.Add(this.charNameField);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DetailsPane";
            this.Size = new System.Drawing.Size(634, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Field charNameField;
        private Common.Field playerNameField;
        private Common.Field DeityField;
        private System.Windows.Forms.Label XPLabel;
    }
}
