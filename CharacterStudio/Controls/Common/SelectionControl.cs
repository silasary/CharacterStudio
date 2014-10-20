using ParagonLib;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CharacterStudio.Controls.Common
{
    public partial class SelectionControl : UserControl
    {
        private Search search;

        public SelectionControl(string Label, Search search)
        {
            InitializeComponent();
            this.search = search;
        }

        public string Selection { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            var res = search.Results();
            if (res.Count() == 1)
            {
                this.Selection = res.FirstOrDefault().InternalId;
            }
            if (this.Selection == null && !string.IsNullOrEmpty(search.Default))
            {
                this.Selection = search.Default;
            }
            base.OnLoad(e);
        }
    }
}