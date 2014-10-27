﻿using System;
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
    public partial class NewCharacterPane : ContentPane
    {
        public NewCharacterPane()
        {
            InitializeComponent();
            RuleFactory.FileLoaded += RuleFactory_FileLoaded;
            AddAllSystems();
        }

        void RuleFactory_FileLoaded(string Filename)
        {
            AddAllSystems();
        }

        private void AddAllSystems()
        {
            foreach (var system in RuleFactory.KnownSystems)
            {
                if (!listBox1.Items.Contains(system))
                    listBox1.Items.Add(system);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = ((ListBox)sender);
            if (lb.SelectedItem == null)
                ManualBuild.Enabled = false;
            else
                ManualBuild.Enabled = true;

        }

        private void ManualBuild_Click(object sender, EventArgs e)
        {
            Character Char = new Character((string)listBox1.SelectedItem);
            LoadCharacter(Char);
            DisplayPanel<SelectionPanes.SelectRace>();
        }
    }
}
