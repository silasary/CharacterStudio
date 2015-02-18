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
        const string SaveFileVersion = "0.08a";

        public bool Save(Character c, string savefile)
        {
            var writer = XmlWriter.Create(savefile, new XmlWriterSettings() { Indent = true });
            writer.WriteStartDocument( );
            writer.WriteStartElement("D20Character");
            writer.WriteAttributeString("game-system", c.workspace.System);
            writer.WriteAttributeString("Version", SaveFileVersion); // Both the OCB and NCB report 0.07a.  [Why do people even bother versioning things if they're not going to bump the version?] 
            WriteCharacterSheet(c, writer);

            writer.WriteEndElement( );
            writer.WriteEndDocument( );
            writer.Close( );
            return true;
        }

        private void WriteCharacterSheet(Character c, XmlWriter writer)
        {
            writer.WriteStartElement("CharacterSheet");
            // Details are a quick summary of your character.  Almost entirely fluff.
            WriteDetails(c, writer);
            // AbilityScores. Summary of your 6 scores.  Just the totals.

            // StatBlock. Array of Stats.  There's a lot of them.

            // RulesElementTally.  Boring.
            writer.WriteEndElement( );
        }


        private void WriteDetails(Character c, XmlWriter writer)
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
	}

}
