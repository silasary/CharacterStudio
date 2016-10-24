using NUnit.Framework;
using ParagonLib.Grammar;
using ParagonLib.Utils;
using System.Collections.Generic;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;
using System;

namespace UnitTests
{
    internal class TestGrammar
    {
        [Test,]
        public void TestAttackLine()
        {
            // Below are a list of sample lines, in increasing complexity:
            Dictionary<string, AttackStat[]> lines = new Dictionary<string, AttackStat[]> {
             {"Dexterity vs. AC",
             new AttackStat[] {
                 new AttackStat { Ability = "Dexterity", Defence="AC"}
             }
             },
             {
                "Strength or Wisdom vs. Will",
                 new AttackStat[] {
                 new AttackStat { Ability = "Strength", Defence="Will"},
                 new AttackStat { Ability = "Wisdom", Defence="Will"}
                }
             },
             {
             "Strength, Wisdom, or Charisma vs. Fortitude",
             new AttackStat[]{
                    new AttackStat { Ability = "Strength", Defence = "Fortitude"},
                    new AttackStat { Ability = "Wisdom", Defence = "Fortitude"},
                    new AttackStat { Ability = "Charisma", Defence = "Fortitude"},
                }
             },
             {
                 "Strength vs. AC, Dexterity vs. AC, or Constitution vs. AC",
                 new AttackStat[] {
                     new AttackStat { Ability = "Strength", Defence="AC"},
                     new AttackStat { Ability = "Dexterity", Defence="AC"},
                     new AttackStat { Ability = "Constitution", Defence="AC"},
                 }
             },
             {
                "Strength -2 vs. AC",
                new AttackStat[] {
                    new AttackStat { Ability = "Strength", Modifier=-2, Defence="AC"}
                }
             },
             {
             "Strength + 2 vs. AC, three attacks",
             new AttackStat[] {
                 new AttackStat{Ability="Strength", Modifier=+2, Defence="AC"}
                }
            },
            {
             "Strength + 2 vs. Reflex, Constitution + 2 vs. Reflex, or Dexterity + 2 vs. Reflex",
             new AttackStat[] {
                 new AttackStat { Ability="Strength", Modifier = +2, Defence = "Reflex"},
                 new AttackStat { Ability="Constitution", Modifier = +2, Defence = "Reflex"},
                 new AttackStat { Ability="Dexterity", Modifier = +2, Defence = "Reflex"},
                }
            },
            {
             "Your highest ability vs. AC",
             new AttackStat[]{
                 new AttackStat{ Ability="highest ability", Defence="AC"}
                }
            },
            {
             "Highest mental ability vs. Will",
             new AttackStat[] {
                    new AttackStat { Ability = "Highest mental ability", Defence = "Will"}
                }
            },
            {
             "Your highest physical ability vs. AC",
             new AttackStat[] {
                    new AttackStat { Ability = "highest physical ability", Defence="AC"}
                }
            }
            };
            //TODO: Twin Strike.
             //"Strength vs. AC (melee; main weapon and off-hand weapon) or Dexterity vs. AC (ranged), two attacks",
            foreach (var item in lines)
            {
                AttackStat[] AttackComponents;
                DamageStat DamageComponents;
                GrammarParser.ParsePowerLines(out AttackComponents, out DamageComponents, new PowerLine("Attack", item.Key));
                Assert.AreEqual(item.Value, AttackComponents);
            }
        }

        //[Test]
        //[Ignore]
        public void TestDamageLine()
        {
            Func<DamageStat,DamageStat[]> array = (DamageStat d) => new DamageStat[] { d };
            
            var lines = new Dictionary<PowerLine, DamageStat>
            {
                {
                new PowerLine("Hit", "1[W] + Strength modifier damage."),
                new DamageStat { 
                    Dice = new DiceStat[] {new DiceStat { x=1, name="W" }},
                    Mods= new AbilityModToken[] { new AbilityModToken("Strength")},
                    }
                },
                {
                new PowerLine("Hit", "1d6 + Intelligence modifier lightning damage."),
                new DamageStat{ 
                    //Dice= new DiceStat[] { new DiceStat{ x=1,y=6}}, 
                    //Mods= new AbilityModToken[] { new AbilityModToken("Intelligence")},
                    //type= "lightning"
                }
                },
                {
                new PowerLine("Hit", "1d8 + Constitution modifier damage, and the target takes a -2 penalty to attack rolls and ongoing 2 poison damage (save ends both)."),
                default(DamageStat)
                },
                {
                new PowerLine("Hit", "2[W] + Strength modifier damage per attack."),
                default(DamageStat)
                },
                {
                    new PowerLine("Hit", "2d10 + Wisdom modifier lightining damage."),
                    default(DamageStat)
                },
                {
                    new PowerLine("Hit", "Dexterity modifier damage, and the target is blinded until the end of your next turn."),
                    default(DamageStat)
                },
                {
                new PowerLine("Hit", "1[B] + beast's Strength modifier + your Wisdom modifier damage."),
                default(DamageStat)                
                },
                {
                new PowerLine("Hit", "1[W] + 1d6 + Strength modifier damage."),
                default(DamageStat)                
                },
                
                
            };
            foreach (var item in lines)
            {
                AttackStat[] AttackComponents;
                DamageStat DamageComponents;
                GrammarParser.ParsePowerLines(out AttackComponents, out DamageComponents, item.Key);
                if (item.Value.Equals(default(DamageStat)))
                    Assert.Inconclusive();
                else
                    Assert.AreEqual(item.Value, DamageComponents);
            }
        }
    }
}