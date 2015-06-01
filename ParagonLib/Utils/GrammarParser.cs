using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;
using ParagonLib.Utils;

namespace ParagonLib.Grammar
{
    static public class GrammarParser
    {
        private static string[] DamageTypes = { "fire", "thunder", "lightning", "force"};

        private static Parser<string> AbilityScores;
        private static Parser<string> Defences;
        private static Parser<string> PlusMod;
        private static Parser<string> MinusMod;
        private static Parser<AbilModToken> AbilityPlusMod;
        private static Parser<AttackStatToken> AttackStatement;
        private static Parser<IEnumerable<AttackStat>> AttackLineParser;
        private static Parser<IEnumerable<AbilModToken>> MultiAbility;
        
        private static Parser<DiceStat> Dice;
        private static Parser<AbilityModToken> AbilityModifier;
        private static Parser<DamageStat> DamageLineParser;

        static GrammarParser()
        {
            AbilityScores = Parse.Regex(String.Join("|", Workspace.D20AbilityScores))
                .Or(Parse.Regex(new Regex(@"Highest( Mental| Physical)? Ability", RegexOptions.IgnoreCase))).Token();
            Defences = Parse.Regex("AC|Reflex|Will|Fortitude").Token();
            PlusMod =
                from plus in Parse.Char('+')
                from w in Parse.WhiteSpace
                from num in Parse.Digit.XMany().Text()
                select num;
            MinusMod =
                from minus in Parse.Char('-')
                from num in Parse.Digit.Many().Text()
                select string.Concat(minus, num);
            AbilityPlusMod =
                from abil in AbilityScores.Token()
                from mod in PlusMod.XOr(MinusMod).Text().Token().Optional()
                select new AbilModToken(abil, mod);
            MultiAbility =(
                from ability in AbilityPlusMod.Token()
                from comma in Parse.Char(',').Optional()
                from or in Parse.Regex("or").Token().Optional()
                select ability).XMany();
            AttackStatement =
                from Attack in MultiAbility.Token()
                from vs in Parse.Regex(@"vs\.").Token()
                from Defence in Defences.Token()
                select new AttackStatToken(Attack, Defence);
            AttackLineParser =
                 from attacktokens in
                     (from your in Parse.Regex("Your").Token().Optional()
                      from attacks in
                          (from attack in AttackStatement.Token()
                           from comma in Parse.Char(',').Optional()
                           from or in Parse.Regex("or").Token().Optional()
                           select attack).AtLeastOnce()
                      select attacks)
                 select AttackStat.New(attacktokens);
            // Damage:
            Dice = (from x in Parse.Digit.Many().Text()
                    from d in Parse.Char('d')
                    from y in Parse.Digit.Many().Text()
                    select new DiceStat { x = int.Parse( x), y = int.Parse( y) })
                   .Or
                    (from x in Parse.Digit.Many().Text()
                     from open in Parse.Char('[')
                     from name in Parse.Letter.Many().Text()
                     from close in Parse.Char(']')
                     select new DiceStat { name = name, x=int.Parse(x) });
            AbilityModifier =
                from ability in AbilityScores.Token()
                from modifer in Parse.Return("modifier").Token()
                from plus in Parse.Char('+').Token().Optional()
                select new AbilityModToken(ability);
            var DamageType = Parse.Regex(string.Join("|", DamageTypes)).Token().Optional();
            DamageLineParser =
                from dice in _Plus(Dice).Token().Many()
                from mod in AbilityModifier.Token().Many()
                from type in DamageType
                from dmg in Parse.Return("damage")
                select new DamageStat(dice, mod, type);
        }
            
        public static void ParsePowerLines(out AttackStat[] AttackComponents, out DamageStat DamageComponents, params PowerLine[] lines)
        {
            var AttackLine = lines.FirstOrDefault(n => n.Name == "Attack" || n.Name == "Primary Attack");
            if (AttackLine.Name != null)
            {
                AttackComponents = AttackLineParser.Parse(AttackLine.Value).ToArray();
                //var res = AttackStatement.Parse(AttackLine.Value);
            }
            else
                AttackComponents = null;
            var HitLine = lines.FirstOrDefault(n => n.Name == "Hit");
            if (HitLine.Name != null)
            {
                DamageComponents = DamageLineParser.Parse(HitLine.Value);
            }
            else
                DamageComponents = default(DamageStat);
        }

        static Parser<T> _Plus<T>(Parser<T> p)
        {
            return from q in p.Token()
                   from plus in Parse.Char('+').Token().Optional()
                   select q;
        }
    }

    public struct AbilModToken
    {
        public string Ability;
        public int Modifier;
        

        public AbilModToken(string abil, IOption<string> mod)
        {
            // TODO: Complete member initialization
            this.Ability = abil;
            this.Modifier = int.Parse(string.Concat(mod.GetOrElse("0")));
        }
        public override string ToString()
        {
            if (Modifier != 0)
                return string.Format("{0} + {1}", Ability, Modifier);
            return Ability;
        }
    }

    struct AttackStatToken
    {
        public IEnumerable<AbilModToken> Attack;
        public string Defence;

        public AttackStatToken(IEnumerable<AbilModToken> Attack, string Defence)
        {
            this.Attack = Attack;
            this.Defence = Defence;
        }
        public override string ToString()
        {
           return string.Format("{0} vs. {2}", string.Join(",", Attack), Defence);
        }
    }

    public struct AttackStat
    {
        public string Ability;
        public int Modifier;
        public string Defence;

        public override string ToString()
        {
            return string.Format("{0} vs. {1}", Ability, Defence);
        }

        internal static IEnumerable<AttackStat> New(IEnumerable<AttackStatToken> Attacks)
        {
            foreach (var attack in Attacks)
            {
                foreach (var att in attack.Attack)
                {
                    
                yield return new AttackStat()
                {
                    Ability = att.Ability,
                    Modifier = att.Modifier,
                    Defence = attack.Defence
                };
                }
            }
        }
    }

    public struct DamageStat
    {
        public DiceStat[] Dice;
        public AbilityModToken[] Mods;
        public string type;

        public string Damage { get { return string.Join(" + ", Dice.Cast<object>().Concat(Mods.Cast<object>())); } }

        public DamageStat(IEnumerable<DiceStat> dice, IEnumerable<AbilityModToken> mod, IOption<string> type)
        {
            // TODO: Complete member initialization
            this.Dice = dice.ToArray();
            this.Mods = mod.ToArray();
            this.type = type.GetOrDefault();
        }
        public override string ToString()
        {
            return Damage + (string.IsNullOrEmpty(type) ? " damage" : string.Format(" {0} damage", type));
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DamageStat))
                return false;
            var other = (DamageStat)obj;

            if (!(Dice ?? new DiceStat[0]).SequenceEqual(other.Dice ?? new DiceStat[0]))
                return false;
            if (!(Mods ?? new AbilityModToken[0]).SequenceEqual(other.Mods ?? new AbilityModToken[0]))
                return false;
            return type == other.type;
        }
    }

    public struct DiceStat
    {
        public int x;
        public int y;
        public string name;

        public override string ToString()
        {
            return y == 0 ? string.Format("{0}[{1}]", x, name) : string.Format("{0}d{1}", x, y);
        }
    }

    public struct AbilityModToken
    {
        private string ability;

        public AbilityModToken(string ability)
        {
            this.ability = ability;
        }

        public override string ToString()
        {
            return string.Format("{0} modifier", ability);
        }
    }
}
