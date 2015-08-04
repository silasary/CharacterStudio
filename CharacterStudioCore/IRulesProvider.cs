using CharacterStudio.Rules;

namespace CharacterStudio
{
    public interface IRulesProvider
    {
        void ResetAllRules();
        void LoadRuleXml(string XmlPath);
        //RulesElement GetRule(string id, string GameSystem, CampaignSetting setting);
        RuleData GetAllRules(string GameSystem = null);
    }
}
