namespace CharacterStudio.Rules
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

        public string Source { get; set; }

        public string PartFile { get; set; }

        public int LineNumber { get; set; }

        public RuleData(IRulesElement e){
            Name = e.Name;
            Type = e.Type;
            GameSystem = e.GameSystem;
            Categories = e.Category;
            InternalId = e.InternalId;
            Prereqs = e.Prereqs;
            PrintPrereqs = e.PrintPrereqs;
            Source = e.Source;
            PartFile = null;
            LineNumber = -1;
        }
    }
}