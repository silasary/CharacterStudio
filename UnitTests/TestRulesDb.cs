using db = RulesDb.RulesDb;
using NUnit.Framework;

namespace UnitTests
{
    class TestRulesDb
    {
        [Test]
        [Category("RulesDb")]
        [TestCaseSource(typeof(FullTests), "Parts")]
        public void LoadRuleXml(string part)
        {
            db.LoadRulesFile(part);
        }

    }
}
