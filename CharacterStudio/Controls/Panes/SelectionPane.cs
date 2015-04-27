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
    public partial class SelectionPane : ContentPane
    {
        ChoiceConverter converter;
        public readonly string[] Selectables;
        public SelectionPane(string[] p)
        {
            InitializeComponent();
            Selectables = p;
            converter = new ChoiceConverter(this);
        }

        public override void OnCharacterLoad()
        {
             base.OnCharacterLoad();
            OnCharacterUpdated();
        }

        public override void OnCharacterUpdated()
        {
            base.OnCharacterUpdated();
            Options = CurrentWorkspace.Selections(this.Selectables);
            if (Options.FirstOrDefault() == null)
                this.Enabled = false;
            else
                this.Enabled = true;
            listView1.Items.Clear();
            foreach (var choice in Options)
            {
                listView1.Items.Add(converter.ConvertTo(choice, typeof(ListViewItem)) as ListViewItem);
            }
        }

        public IEnumerable<Selection> Options { get; private set; }
    }
}
