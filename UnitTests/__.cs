using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if DEBUG
namespace UnitTests
{
    class __ // Exists as a place to decompile things into IL that I'm not sure how to emit.
    {       // Ignore me
        void md()
        {
            RuleData ruleData = new RuleData();
            ruleData.InternalId = "ID_FMP_CLASS_FEATURE_1460";
            RuleFactory.RegisterMetadata(ruleData);
            ruleData = new RuleData();
            ruleData.InternalId = "ID_FMP_CLASS_FEATURE_3507";
            RuleFactory.RegisterMetadata(ruleData);
            ruleData = new RuleData();
            ruleData.InternalId = "ID_FMP_POWER_466";
            RuleFactory.RegisterMetadata(ruleData);
            ruleData = new RuleData();
            ruleData.InternalId = "ID_FMP_POWER_13285";
            RuleFactory.RegisterMetadata(ruleData);
        }
        
        int sw(string c)
        {
            switch (c)
            {
                case"a":
                    return 1;
                case"b":
                    return 2;
                case"3":
                    return 3;
                case"4":
                    return 4;
                case"dtyf":
                    return 6;
                case"q":
                    return 8;
                case "w":
                    return 9;
                default:
                    return 0;
            }
        }
    }
}
#endif