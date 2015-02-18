using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ParagonLib
{
	public class Serializer
	{
        /// <summary>
        /// 0.07a is OCB/CBLoader.
        /// 0.07a is also Silverlight [Henceforth called 0.07b for sanity reasons]
        /// 0.08a is CharacterStudio Alpha format.
        /// </summary>
        const string PreferedSaveFileVersion = "0.08a";
        string SaveFileVersion;

        XmlWriter writer;
        Character c;

        public bool Save(Character c, string savefile)
        {
            this.c = c;
            c.workspace.Recalculate(true);
            SaveFileVersion = PreferedSaveFileVersion;
            writer = XmlWriter.Create(savefile, new XmlWriterSettings() { Indent = true });
            writer.WriteStartDocument( );
            writer.WriteStartElement("D20Character");
            writer.WriteAttributeString("game-system", c.workspace.System);
            writer.WriteAttributeString("Version", SaveFileVersion); // Both the OCB and NCB report 0.07a.  [Why do people even bother versioning things if they're not going to bump the version?] 
            WriteComment("Character Studio character save file.  Schema compatibile with the Dungeons and Dragons Insider: Character Builder");

            WriteCharacterSheet();

            writer.WriteEndElement( );
            writer.WriteEndDocument( );
            writer.Close( );
            return true;
        }

        private void WriteComment(string p)
        {
      
            writer.WriteComment(string.Format("{0}", p));
        }

        private void WriteCharacterSheet()
        {
            writer.WriteStartElement("CharacterSheet");
            // Details are a quick summary of your character.  Almost entirely fluff.
            WriteDetails();
            // AbilityScores. Summary of your 6 scores.  Just the totals.
            WriteAbilityScores();
            // StatBlock. Array of Stats.  There's a lot of them.
            WriteStatBlock();
            // RulesElementTally.  Boring.
            WriteRulesElementTally();
            writer.WriteEndElement( );
        }

        private void WriteRulesElementTally()
        {
            //throw new NotImplementedException();
        }


        private void WriteDetails()
        {
            // 0.07[ab] both agree on the format here.
            writer.WriteStartElement("Details");
            writer.WriteElementString("name", c.Name); // This one is lowercase, whilst everything else is uppercase.  It confuses me. 
            writer.WriteElementString("Level", c.workspace.Level.ToString( ));
            //Player
            //Height
            //Weight
            //Gender
            //Age
            //Alignment
            //Company
            //Portrait //This one always irritated me - Even on the silverlight version it refers to a local file path.
            writer.WriteElementString("Experience", c.workspace.GetStat("XP Earned").Value.ToString());
            //CarriedMoney
            //StoredMoney
            //Traits
            //Appearance
            //Companions
            //Notes

            writer.WriteEndElement( );
        }

        private void WriteAbilityScores()
        {
            WriteComment("Base ability scores (see stats of same name for final adjusted score)");
            writer.WriteStartElement("AbilityScores");
            var Scores = new string[] { "Strength", "Constitution", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
            foreach (var score in Scores)
            {
                writer.WriteStartElement(score);
                writer.WriteAttributeString("score", c.workspace.GetStat(score).Value.ToString());
                writer.WriteEndElement( );
            }
            writer.WriteEndElement( );
        }

        private void WriteStatBlock()
        {
            WriteComment("Final computed stat values - the various numbers\n         on the character sheet are here along with behind the scenes\n         values to build them.");
            writer.WriteStartElement("StatBlock");
            foreach (var stat in c.workspace.Stats.Values.Distinct()) 
            {
                stat.Write(writer);
            }
            writer.WriteEndElement();
        }
	}

}
