using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.Rules
{
    public class CategoryInfo
    {
        public string Name { get; set; }
        public string InternalId { get; set; }
        public List<string> Members { get; private set; }

        public CategoryInfo()
        {
            Members = new List<string>();
        }

        public void Merge(CategoryInfo other)
        {
            if (string.IsNullOrEmpty(this.Name))
                this.Name = other.Name;
            if (string.IsNullOrEmpty(this.InternalId))
                this.InternalId = other.InternalId;
            this.Members = new List<string>(this.Members.Union(other.Members).Distinct());
        }
    }
}
