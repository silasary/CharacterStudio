﻿using CharacterStudio.Controls.Panes;
namespace CharacterStudio
{
    partial class PrimaryForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.HeaderPanel = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.buildTab = new System.Windows.Forms.TabPage();
            this.shopTab = new System.Windows.Forms.TabPage();
            this.adventureTab = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SidebarPanel = new System.Windows.Forms.Panel();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.Help = new System.Windows.Forms.HelpProvider();
            this.leftSidebar1 = new CharacterStudio.Controls.Panes.LeftSidebar();
            this.homeTab = new System.Windows.Forms.TabPage();
            this.newCharTab = new System.Windows.Forms.TabPage();
            this.loadCharTab = new System.Windows.Forms.TabPage();
            this.charDetailsTab = new System.Windows.Forms.TabPage();
            this.HeaderPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SidebarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.Controls.Add(this.tabControl1);
            this.HeaderPanel.Controls.Add(this.label1);
            this.HeaderPanel.Controls.Add(this.menuStrip1);
            this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.HeaderPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.HeaderPanel.Name = "HeaderPanel";
            this.HeaderPanel.Size = new System.Drawing.Size(1256, 109);
            this.HeaderPanel.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.buildTab);
            this.tabControl1.Controls.Add(this.shopTab);
            this.tabControl1.Controls.Add(this.adventureTab);
            this.tabControl1.Controls.Add(this.homeTab);
            this.tabControl1.Controls.Add(this.newCharTab);
            this.tabControl1.Controls.Add(this.loadCharTab);
            this.tabControl1.Controls.Add(this.charDetailsTab);
            this.tabControl1.Location = new System.Drawing.Point(13, 36);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(537, 73);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // buildTab
            // 
            this.buildTab.Location = new System.Drawing.Point(4, 63);
            this.buildTab.Name = "buildTab";
            this.buildTab.Padding = new System.Windows.Forms.Padding(3);
            this.buildTab.Size = new System.Drawing.Size(529, 6);
            this.buildTab.TabIndex = 0;
            this.buildTab.Text = "Build Character";
            this.buildTab.UseVisualStyleBackColor = true;
            this.buildTab.Tag = typeof(Controls.Panes.SelectionPanes.SelectRace);
            // 
            // shopTab
            // 
            this.shopTab.Location = new System.Drawing.Point(4, 32);
            this.shopTab.Name = "shopTab";
            this.shopTab.Padding = new System.Windows.Forms.Padding(3);
            this.shopTab.Size = new System.Drawing.Size(529, 37);
            this.shopTab.TabIndex = 1;
            this.shopTab.Text = "Shopping";
            this.shopTab.UseVisualStyleBackColor = true;
            // 
            // adventureTab
            // 
            this.adventureTab.Location = new System.Drawing.Point(4, 32);
            this.adventureTab.Name = "adventureTab";
            this.adventureTab.Padding = new System.Windows.Forms.Padding(3);
            this.adventureTab.Size = new System.Drawing.Size(529, 37);
            this.adventureTab.TabIndex = 1;
            this.adventureTab.Text = "Adventure";
            this.adventureTab.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(824, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(311, 80);
            this.label1.TabIndex = 0;
            this.label1.Text = "Need Layout concepts.\r\nDo people want horizonal flow (Old Builder)\r\nOr Vertical f" +
    "low (New Builder)\r\nOr something else?\r\n";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1256, 33);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(50, 29);
            this.fileMenu.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(123, 30);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.New_Click);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(123, 30);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(123, 30);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.Save_Click);
            // 
            // SidebarPanel
            // 
            this.SidebarPanel.Controls.Add(this.leftSidebar1);
            this.SidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.SidebarPanel.Location = new System.Drawing.Point(0, 109);
            this.SidebarPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SidebarPanel.Name = "SidebarPanel";
            this.SidebarPanel.Size = new System.Drawing.Size(234, 919);
            this.SidebarPanel.TabIndex = 1;
            // 
            // ContentPanel
            // 
            this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentPanel.Location = new System.Drawing.Point(234, 109);
            this.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(1022, 919);
            this.ContentPanel.TabIndex = 2;
            // 
            // leftSidebar1
            // 
            this.leftSidebar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftSidebar1.Location = new System.Drawing.Point(0, 0);
            this.leftSidebar1.Name = "leftSidebar1";
            this.leftSidebar1.PrimaryForm = null;
            this.leftSidebar1.Size = new System.Drawing.Size(234, 919);
            this.leftSidebar1.TabIndex = 0;
            // 
            // homeTab
            // 
            this.homeTab.Location = new System.Drawing.Point(4, 32);
            this.homeTab.Name = "homeTab";
            this.homeTab.Padding = new System.Windows.Forms.Padding(3);
            this.homeTab.Size = new System.Drawing.Size(529, 37);
            this.homeTab.TabIndex = 4;
            this.homeTab.Tag = typeof(CharacterStudio.Controls.Panes.HomePane);
            this.homeTab.Text = "Home";
            this.homeTab.UseVisualStyleBackColor = true;
            // 
            // newCharTab
            // 
            this.newCharTab.Location = new System.Drawing.Point(4, 32);
            this.newCharTab.Name = "newCharTab";
            this.newCharTab.Padding = new System.Windows.Forms.Padding(3);
            this.newCharTab.Size = new System.Drawing.Size(529, 37);
            this.newCharTab.TabIndex = 2;
            this.newCharTab.Tag = typeof(CharacterStudio.Controls.Panes.NewCharacterPane);
            this.newCharTab.Text = "New Character";
            this.newCharTab.UseVisualStyleBackColor = true;
            // 
            // loadCharTab
            // 
            this.loadCharTab.Location = new System.Drawing.Point(4, 63);
            this.loadCharTab.Name = "loadCharTab";
            this.loadCharTab.Padding = new System.Windows.Forms.Padding(3);
            this.loadCharTab.Size = new System.Drawing.Size(529, 6);
            this.loadCharTab.TabIndex = 3;
            this.loadCharTab.Tag = typeof(CharacterStudio.Controls.Panes.LoadCharacterPane);
            this.loadCharTab.Text = "Load Character";
            this.loadCharTab.UseVisualStyleBackColor = true;
            // 
            // charDetailsTab
            // 
            this.charDetailsTab.Location = new System.Drawing.Point(4, 63);
            this.charDetailsTab.Name = "charDetailsTab";
            this.charDetailsTab.Padding = new System.Windows.Forms.Padding(3);
            this.charDetailsTab.Size = new System.Drawing.Size(529, 6);
            this.charDetailsTab.TabIndex = 5;
            this.charDetailsTab.Tag = typeof(DetailsPane);
            this.charDetailsTab.Text = "Character";
            this.charDetailsTab.UseVisualStyleBackColor = true;
            // 
            // PrimaryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1256, 1028);
            this.Controls.Add(this.ContentPanel);
            this.Controls.Add(this.SidebarPanel);
            this.Controls.Add(this.HeaderPanel);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "PrimaryForm";
            this.Text = "Form1";
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.SidebarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel HeaderPanel;
        private System.Windows.Forms.Panel SidebarPanel;
        private System.Windows.Forms.Panel ContentPanel;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.HelpProvider Help;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage buildTab;
        private System.Windows.Forms.TabPage shopTab;
        private System.Windows.Forms.TabPage adventureTab;
        private System.Windows.Forms.TabPage newCharTab;
        private System.Windows.Forms.TabPage loadCharTab;
        private System.Windows.Forms.TabPage homeTab;
        private LeftSidebar leftSidebar1;
        private System.Windows.Forms.TabPage charDetailsTab;
    }
}

