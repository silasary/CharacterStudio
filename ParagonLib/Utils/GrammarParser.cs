using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;

namespace ParagonLib.Utils
{
    static public class GrammarParser
    {
        private static Parser<string> AbilityScores;
        private static Parser<string> Defences;
        private static Parser<IEnumerable<char>> PlusMod;
        private static Parser<string> MinusMod;
        private static Parser<AbilModToken> AbilityPlusMod;
        private static Parser<AttackStat> AttackStatement;
        private static Parser<IEnumerable<AttackStat>> AttackLineParser;
        private static Parser<IEnumerable<AbilModToken>> MultiAbility;

        static GrammarParser()
        {
            AbilityScores = Parse.Regex(String.Join("|", Workspace.D20AbilityScores)).Token();
            Defences = Parse.Regex("AC|Reflex|Will|Fortitude").Token();
            PlusMod =
                from plus in Parse.Char('+')
                from w in Parse.WhiteSpace
                from num in Parse.Digit.XMany()
                select num;
            MinusMod =
                from minus in Parse.Char('-')
                from num in Parse.Digit.Many()
                select string.Concat(minus, num);
            AbilityPlusMod =
                from abil in AbilityScores.Token()
                from mod in PlusMod.XOr(MinusMod).Token().Optional()
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
                select new AttackStat(Attack, Defence);
            AttackLineParser =
                (from attack in AttackStatement.Token()
                 from comma in Parse.Char(',').Optional()
                 from or in Parse.Regex("or").Token().Optional()
                select attack).XMany();
                
        }

        public static void ParsePowerLines(params PowerLine[] lines)
        {
            var AttackLine = lines.FirstOrDefault(n => n.Name == "Attack" || n.Name == "Primary Attack");
            if (AttackLine.Name != null)
            {
                var res = AttackLineParser.Parse(AttackLine.Value);
                //var res = AttackStatement.Parse(AttackLine.Value);

            }
            
        }


    }

    struct AbilModToken
    {
        public string Ability;
        private int Modifier;
        

        public AbilModToken(string abil, IOption<IEnumerable<char>> mod)
        {
            // TODO: Complete member initialization
            this.Ability = abil;
            this.Modifier = int.Parse(string.Concat(mod.GetOrElse(new char[] {'0'})));
        }
        public override string ToString()
        {
            if (Modifier != 0)
                return string.Format("{0} + {1}", Ability, Modifier);
            return Ability;
        }
    }

    struct AttackStat
    {
        public IEnumerable<AbilModToken> Attack;
        public string Defence;

        public AttackStat(IEnumerable<AbilModToken> Attack, string Defence)
        {
            this.Attack = Attack;
            this.Defence = Defence;
        }
        public override string ToString()
        {
           return string.Format("{0} vs. {2}", string.Join(",", Attack), Defence);
        }
    }
}
