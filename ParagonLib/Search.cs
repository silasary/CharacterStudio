using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Search
    {
        private string System;
        private string Type;
        private string Category;

        public Search(string System, string Type, string Category)
        {
            this.System = System;
            this.Type = Type;
            this.Category = Category;
        }

        public IEnumerable<RulesElement> Results()
        {
            var Categories = Category == null ? new string[0] : Category.Split(',');
            var catCount = Categories.Count();
            var Comparer = new CategoryComparer();
            foreach (var item in RuleFactory.Rules.Values.ToArray())
            {
                if (String.IsNullOrEmpty(Type) || item.Type == Type)
                {
                    if (String.IsNullOrEmpty(System) || String.IsNullOrEmpty(item.System) || item.System == System)
                    {
                        if (Categories.Intersect(item.Category, Comparer).Count() == catCount)
                        {
                            yield return item;
                        }
                    }
                }
            }
            yield break;
        }
    }
}
