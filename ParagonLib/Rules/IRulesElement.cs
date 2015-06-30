using System;
namespace ParagonLib.Rules
{
    public interface IRulesElement
    {
        string _Text { get; }
        //Action<CharElement, Workspace> Calculate { get; }
        string[] Category { get; }
        string Flavor { get; }
        string GameSystem { get; }
        string InternalId { get; }
        string Name { get; }
        string Prereqs { get; }
        string PrintPrereqs { get; }
        string ShortDescription { get; }
        string Source { get; }
        string Type { get; }
    }
}
