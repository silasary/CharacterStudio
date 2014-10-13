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
            this.field3 = new CharacterStudio.Controls.Common.Field();
            this.field2 = new CharacterStudio.Controls.Common.Field();
            this.field1 = new CharacterStudio.Controls.Common.Field();
            this.SuspendLayout();
            // 
            // field3
            // 
            this.field3.AutoSize = true;
            this.field3.Label = "Deity";
            this.field3.Location = new System.Drawing.Point(3, 67);
            this.field3.Name = "field3";
            this.field3.Size = new System.Drawing.Size(216, 27);
            this.field3.TabIndex = 0;
            // 
            // field2
            // 
            this.field2.AutoSize = true;
            this.field2.Label = "Player Name";
            this.field2.Location = new System.Drawing.Point(3, 35);
            this.field2.Name = "field2";
            this.field2.Size = new System.Drawing.Size(216, 27);
            this.field2.TabIndex = 0;
            // 
            // field1
            // 
            this.field1.AutoSize = true;
            this.field1.Label = "Character Name";
            this.field1.Location = new System.Drawing.Point(3, 3);
            this.field1.Name = "field1";
            this.field1.Size = new System.Drawing.Size(216, 27);
            this.field1.TabIndex = 0;
            // 
            // DetailsPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.field3);
            this.Controls.Add(this.field2);
            this.Controls.Add(this.field1);
            this.Name = "DetailsPane";
            this.Size = new System.Drawing.Size(423, 390);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Field field1;
        private Common.Field field2;
        private Common.Field field3;
    }
}
