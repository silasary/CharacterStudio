using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;

namespace ParagonLib.Utils
{
    static public class GrammarParser
    {
        private static Parser<string> AbilityScores;
        private static Parser<string> Defences;
        private static Parser<string> PlusMod;
        private static Parser<string> MinusMod;
        private static Parser<AbilModToken> AbilityPlusMod;
        private static Parser<AttackStatToken> AttackStatement;
        private static Parser<IEnumerable<AttackStat>> AttackLineParser;
        private static Parser<IEnumerable<AbilModToken>> MultiAbility;

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

        }

        public static void ParsePowerLines(params PowerLine[] lines)
        {
            var AttackLine = lines.FirstOrDefault(n => n.Name == "Attack" || n.Name == "Primary Attack");
            if (AttackLine.Name != null)
            {
                var res = AttackLineParser.Parse(AttackLine.Value).ToArray();
                //var res = AttackStatement.Parse(AttackLine.Value);
            }
        }


    }

    struct AbilModToken
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

    struct AttackStat
    {
        public string Ability;
        public int Modifier;
        public string Defence;

        public override string ToString()
        {
            return string.Format("{0} vs. {2}", string.Join(",", Ability), Defence);
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
}
