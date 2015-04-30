using System;
namespace ParagonLib.RuleBases
{
    public interface ILevel
    {
        ILevel PreviousLevel { get; set; }
        int TotalXpNeeded { get; }
        int XpNeeded { get; }
    }
}
