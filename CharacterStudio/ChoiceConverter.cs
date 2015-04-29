using CharacterStudio.Controls.Panes;
using ParagonLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CharacterStudio
{
    class ChoiceConverter : TypeConverter
    {
        private SelectionPane selectionPane;
        private Workspace ws { get { return selectionPane.CurrentWorkspace; } }

        System.Runtime.CompilerServices.ConditionalWeakTable<Selection, ListViewItem> cache = new System.Runtime.CompilerServices.ConditionalWeakTable<Selection, ListViewItem>();

        public ChoiceConverter(Controls.Panes.SelectionPane selectionPane)
        {
            this.selectionPane = selectionPane;
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(Selection))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Selection))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(ListViewItem))
            {
                var choice=value as Selection;
                ListViewItem item;
                if (!cache.TryGetValue(choice, out item))
                    cache.Add(choice, item = new ListViewItem());
                item.Text = choice.Name;
                if (item.SubItems.Count == 1)
                    item.SubItems.Add(new ListViewItem.ListViewSubItem());
                item.SubItems[1].Text = (choice.Child == null) ? string.IsNullOrEmpty(choice.Value) ? choice.UninformedGuess + "?" : choice.Value : choice.Child.Name;
                return item;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
