using System;
namespace CharacterStudio.Rules
{
    public interface ILevel
    {
        ILevel PreviousLevel { get; set; }
        int TotalXpNeeded { get; }
        int XpNeeded { get; }
    }
}
