namespace CharacterStudio.Controls.Panes
{
    partial class NewCharacterPane
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ManualBuild = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.AutoBuild = new System.Windows.Forms.Button();
            this.field3 = new CharacterStudio.Controls.Common.Field();
            this.field2 = new CharacterStudio.Controls.Common.Field();
            this.field1 = new CharacterStudio.Controls.Common.Field();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(36, 78);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(120, 95);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Pick a System";
            // 
            // ManualBuild
            // 
            this.ManualBuild.Enabled = false;
            this.ManualBuild.Location = new System.Drawing.Point(32, 196);
            this.ManualBuild.Name = "ManualBuild";
            this.ManualBuild.Size = new System.Drawing.Size(124, 23);
            this.ManualBuild.TabIndex = 2;
            this.ManualBuild.Text = "Build";
            this.ManualBuild.UseVisualStyleBackColor = true;
            this.ManualBuild.Click += new System.EventHandler(this.ManualBuild_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.AutoBuild);
            this.groupBox1.Controls.Add(this.field3);
            this.groupBox1.Controls.Add(this.field2);
            this.groupBox1.Controls.Add(this.field1);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(235, 50);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(256, 244);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Autobuild";
            // 
            // AutoBuild
            // 
            this.AutoBuild.Location = new System.Drawing.Point(16, 146);
            this.AutoBuild.Name = "AutoBuild";
            this.AutoBuild.Size = new System.Drawing.Size(187, 23);
            this.AutoBuild.TabIndex = 1;
            this.AutoBuild.Text = "AutoBuild";
            this.AutoBuild.UseVisualStyleBackColor = true;
            // 
            // field3
            // 
            this.field3.Label = "Background";
            this.field3.Location = new System.Drawing.Point(16, 104);
            this.field3.Name = "field3";
            this.field3.Size = new System.Drawing.Size(187, 32);
            this.field3.TabIndex = 0;
            // 
            // field2
            // 
            this.field2.Label = "Race";
            this.field2.Location = new System.Drawing.Point(16, 66);
            this.field2.Name = "field2";
            this.field2.Size = new System.Drawing.Size(187, 32);
            this.field2.TabIndex = 0;
            // 
            // field1
            // 
            this.field1.Label = "Class";
            this.field1.Location = new System.Drawing.Point(16, 28);
            this.field1.Name = "field1";
            this.field1.Size = new System.Drawing.Size(187, 32);
            this.field1.TabIndex = 0;
            // 
            // NewCharacterPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ManualBuild);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listBox1);
            this.Name = "NewCharacterPane";
            this.Size = new System.Drawing.Size(494, 384);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ManualBuild;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button AutoBuild;
        private Common.Field field3;
        private Common.Field field2;
        private Common.Field field1;

    }
}
