﻿using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharacterStudio;
using CharacterStudio.Rules;

namespace ParagonLib.RuleEngine
{
    public class Search
    {
        object LastUpdated;
        private RuleData[] results;
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

        public RuleData[] Results
        {
            get
            {
                if (results == null || RuleFactory.LastUpdated != this.LastUpdated)
                {
                    LastUpdated = RuleFactory.LastUpdated;
                    results = Find().ToArray();
                }
                return results;
            }
        }

        public IEnumerable<RuleData> Find()
        {
            //TODO: We need to deal with the Campaign Setting at some point here. 
            var Categories = Category == null ? new string[0] : Category.Split(',');
            var catCount = Categories.Count();
            var Comparer = new CategoryComparer();
            foreach (var item in RuleFactory.RulesBySystem.Values)
            {
                if (!(String.IsNullOrEmpty(Type) || item.Type == Type))
                    continue;
                if (!(String.IsNullOrEmpty(System) || String.IsNullOrEmpty(item.GameSystem) || item.GameSystem == System))
                    continue;
                //TODO:
                if (catCount > 0 && item.Categories == null)
                    continue;
                //if (catCount > 0 && Categories.Intersect(item.Category, Comparer).Count() != catCount)
                //    continue;
                if (Workspace != null && Workspace.Setting != null && !Workspace.Setting.IsRuleLegal(item))
                    continue;
                yield return item;
            }
            if (Workspace != null && Workspace.Setting != null && Workspace.Setting.CustomRules != null)
                foreach (var item in Workspace.Setting.CustomRules.Value.Values)
                {
                    if (!(String.IsNullOrEmpty(Type) || item.Type == Type))
                        continue;
                    if (!(String.IsNullOrEmpty(System) || String.IsNullOrEmpty(item.GameSystem) || item.GameSystem == System))
                        continue;
                    if (catCount > 0 && item.Category == null)
                        continue;
                    if (catCount > 0 && Categories.Intersect(item.Category, Comparer).Count() != catCount)
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
