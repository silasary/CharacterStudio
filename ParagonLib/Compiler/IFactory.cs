using ParagonLib.RuleBases;
using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.Compiler
{
    public interface IFactory
    {
        RulesElement New(string internalId);
        string GameSystem { get; }
        void DescribeCategories(Dictionary<string,CategoryInfo> dict);
    }
}
