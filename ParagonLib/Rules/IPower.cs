using System;
namespace ParagonLib.Rules
{
    interface IPower
    {
        string ActionType { get; }
        string AttackType { get; }
        string Class { get; }
        string Display { get; }
        string[] Keywords { get; }
        int Level { get; }
        string PowerType { get; }
        string PowerUsage { get; }
        string Target { get; }
        string Trigger { get; }
    }
}
