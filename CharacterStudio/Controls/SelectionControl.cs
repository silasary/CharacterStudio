using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterStudio.Controls.Common
{
    public partial class SelectionControl : ContentControl
    {
        private Search search;

        public SelectionControl(string Label, Search search) // Maybe just pass a ParagonLib.Selection?
        {
            InitializeComponent();
            this.search = search;
        }

        public string Selection { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            var res = search.Results;
            var setting = CurrentWorkspace.Setting;
            if (setting != null)
                res = res.Where(n => setting.IsRuleLegal(n)).ToArray();
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