namespace ParagonLib.RuleEngine
{
    public struct RuleData
    {
        public string GameSystem { get; set; }

        public string[] Categories { get; set; }

        public string InternalId { get; set; }

        public string Name { get; set; }

        public string Prereqs { get; set; }

        public string PrintPrereqs { get; set; }

        public string Type { get; set; }

        public static implicit operator RuleData(RuleBases.RulesElement e){
            return new RuleData()
            {
                Name = e.Name,
                Type = e.Type,
                GameSystem = e.GameSystem,
                Categories = e.Category,
                InternalId = e.InternalId,
                Prereqs = e.Prereqs,
                PrintPrereqs = e.PrintPrereqs
            };
        }
    }
}