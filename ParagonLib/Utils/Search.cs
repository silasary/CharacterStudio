﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Search
    {
        public string System { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Default { get; set; }

        public Search(string System, string Type, string Category, string Default, Workspace ws)
        {
            this.System = System;
            this.Type = Type;
            this.Category = Category;
            this.Default = Default;
            this.Workspace = ws;
        }

        public IEnumerable<RulesElement> Results()
        {
            //TODO: We need to deal with the Campaign Setting at some point here. 
            var Categories = Category == null ? new string[0] : Category.Split(',');
            var catCount = Categories.Count();
            var Comparer = new CategoryComparer();
            foreach (var item in RuleFactory.Rules.Values)
            {
                if (!(String.IsNullOrEmpty(Type) || item.Type == Type))
                    continue;
                if (!(String.IsNullOrEmpty(System) || String.IsNullOrEmpty(item.System) || item.System == System))
                    continue;
                if (Categories.Intersect(item.Category, Comparer).Count() != catCount)
                    continue;
                if (Workspace != null && Workspace.Setting != null && !Workspace.Setting.IsRuleLegal(item))
                    continue;
                yield return item;
            }
            yield break;
        }

        public Workspace Workspace { get; set; }
    }
}