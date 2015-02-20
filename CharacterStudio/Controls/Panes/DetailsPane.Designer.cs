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
            this.LevelUpButton = new System.Windows.Forms.Button();
            this.DeityField = new CharacterStudio.Controls.Common.Field();
            this.playerNameField = new CharacterStudio.Controls.Common.Field();
            this.charNameField = new CharacterStudio.Controls.Common.Field();
            this.SuspendLayout();
            // 
            // XPLabel
            // 
            this.XPLabel.AutoSize = true;
            this.XPLabel.Location = new System.Drawing.Point(240, 12);
            this.XPLabel.Name = "XPLabel";
            this.XPLabel.Size = new System.Drawing.Size(90, 13);
            this.XPLabel.TabIndex = 2;
            this.XPLabel.Text = "Level {0} ({1} XP)";
            // 
            // LevelUpButton
            // 
            this.LevelUpButton.Location = new System.Drawing.Point(345, 6);
            this.LevelUpButton.Name = "LevelUpButton";
            this.LevelUpButton.Size = new System.Drawing.Size(75, 23);
            this.LevelUpButton.TabIndex = 1;
            this.LevelUpButton.Text = "Level Up";
            this.LevelUpButton.UseVisualStyleBackColor = true;
            this.LevelUpButton.Click += new System.EventHandler(this.LevelUpButton_Click);
            // 
            // DeityField
            // 
            this.DeityField.AutoSize = true;
            this.DeityField.Label = "Deity";
            this.DeityField.Location = new System.Drawing.Point(3, 67);
            this.DeityField.Name = "DeityField";
            this.DeityField.Size = new System.Drawing.Size(216, 27);
            this.DeityField.TabIndex = 0;
            // 
            // playerNameField
            // 
            this.playerNameField.AutoSize = true;
            this.playerNameField.Label = "Player Name";
            this.playerNameField.Location = new System.Drawing.Point(3, 35);
            this.playerNameField.Name = "playerNameField";
            this.playerNameField.Size = new System.Drawing.Size(216, 27);
            this.playerNameField.TabIndex = 0;
            // 
            // charNameField
            // 
            this.charNameField.AutoSize = true;
            this.charNameField.Label = "Character Name";
            this.charNameField.Location = new System.Drawing.Point(3, 3);
            this.charNameField.Name = "charNameField";
            this.charNameField.Size = new System.Drawing.Size(216, 27);
            this.charNameField.TabIndex = 0;
            // 
            // DetailsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.XPLabel);
            this.Controls.Add(this.LevelUpButton);
            this.Controls.Add(this.DeityField);
            this.Controls.Add(this.playerNameField);
            this.Controls.Add(this.charNameField);
            this.Name = "DetailsPane";
            this.Size = new System.Drawing.Size(423, 390);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Field charNameField;
        private Common.Field playerNameField;
        private Common.Field DeityField;
        private System.Windows.Forms.Button LevelUpButton;
        private System.Windows.Forms.Label XPLabel;
    }
}
