using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParagonLib;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestStatAdd()
        {
            Workspace ws = new Workspace();
            CharElement ce = new CharElement("test_statadd", 0);
            var param =new System.Collections.Generic.Dictionary<string,string>();
            param.Add("name","Speed");
            param.Add("value", "+5");
            var inst= new Instruction("statadd", param);
            inst.Calculate(ce, ws);
            Debug.Assert(ws.GetStat("Speed").Value == 5);
        }

        [TestMethod]
        public void TestStatAlias()
        {
            Workspace ws = new Workspace();
            CharElement ce = new CharElement("test_statalias", 0);
            var param = new System.Collections.Generic.Dictionary<string, string>();
            param.Add("name", "Strength");
            param.Add("value", "+12");
            var inst = new Instruction("statadd", param);
            inst.Calculate(ce, ws);
            var param2 = new System.Collections.Generic.Dictionary<string, string>();
            param2.Add("name", "Strength");
            param2.Add("alias", "str");
            var inst2 = new Instruction("statalias", param2);
            inst2.Calculate(ce, ws);
            Debug.Assert(ws.GetStat("str").Value == 12, "Expected 'str' to equal assigned value of 'Strength'");
        }

        [TestMethod]
        public void LoadRules()
        {
            RuleFactory.New("ID_INTERNAL_LEVEL_1");
        }
    }
}
