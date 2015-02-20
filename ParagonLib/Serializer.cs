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

      


        #region CharacterSheet
        private void WriteCharacterSheet()
        {
            WriteComment("The Character Sheet element contains data readable by 3rd Party Apps. iPlay4e is the one we're testing against, but we'll try to be as compatible as possible.");
            writer.WriteStartElement("CharacterSheet");
            // Details are a quick summary of your character.  Almost entirely fluff.
            WriteDetails();
            // AbilityScores. Summary of your 6 scores.  Just the totals.
            WriteAbilityScores();
            // StatBlock. Array of Stats.  There's a lot of them.
            WriteStatBlock();
            // RulesElementTally.  Boring.
            WriteRulesElementTally();
            // LootTally.  
            WriteLootTally();
            //PowerStats. This one is important. iPlay4e uses it.  
            WritePowerStats();
            //Companions.  Need to reseach what actually goes here, and also implement them myself.
            //Journal.  Our Adventure Log is far superior to the original, But it's also vital we remain Backwards-Compatible.
            WriteJournal();

            WriteComment("WARNING: This character sheet is incomplete.");
            writer.WriteEndElement( );
        }

        private void WriteJournal()
        {
            //This instance of the journal is used for External Reference Only 
            // This makes sense considering that they put it in the <CharacterSheet> tag.  
            // DDI's builder keeps the 'real' Entries as escaped XML in a Textstring. [For the record, that's really gross]
            writer.WriteStartElement("Journal");
            var ser = new DataContractSerializer(typeof(Adventure));
            foreach (var entry in c.workspace.AdventureLog)
            {
                ser.WriteObject(writer, entry);
            }
            writer.WriteEndElement();
        }

        private void WritePowerStats()
        {
            //0.07a includes basic stat calculations - Attack, Damage, and defences.
            //0.07b Includes more detailed data. 
            //WriteComment("\n         The fields for your powers. Each power is then followed\n         by the stats with that power paired with each legal weapon.\n         The weapons are listed in priority as the builder sees it.\n         Particularly, the first weapon listed is the default.\n      ");
            //TODO: Power Blocks!  VERY IMPORTANT.
        }

        private void WriteLootTally()
        {
            WriteComment("\n         The tally of the character&apos;s loot\n      ");
            writer.WriteStartElement("LootTally");
            //TODO: Loot!
            writer.WriteEndElement();
        }

        private void WriteRulesElementTally()
        {
            //0.07a includes just the Short Description.
            //0.07b includes Significantly more. (All non-internal specifics, and fluff text).
            //TODO: Right now we're emulating 0.07a. 
            // I'm not sure if anyone uses the full-text there, or if they refer to the other listings.  
            // Experiment further with ip4e before bloating file?
            WriteComment("\n         The list of all rules elements the character has, after taking into\n         account retraining, multiclass power swapping, etc.\n      ");
            writer.WriteStartElement("RulesElementTally");
            foreach (var rule in c.workspace.AllElements)
            {
                if (!rule.Value.IsAlive)
                    continue;
                var ele = (rule.Value.Target as CharElement);
                writer.WriteStartElement("RulesElement");
                writer.WriteAttributeString("name", ele.RulesElement.Name);
                writer.WriteAttributeString("type", ele.RulesElement.Type);
                writer.WriteAttributeString("internal-id", rule.Key);
                writer.WriteAttributeString("charelem", ele.SelfId.ToString());
                // TODO: URL
                // Legality
                if (ele.RulesElement.Specifics.ContainsKey("Short Description"))
                {
                    writer.WriteStartElement("specific");
                    writer.WriteAttributeString("name", "Short Description");
                    writer.WriteString(ele.RulesElement.Specifics["Short Description"].LastOrDefault());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
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

        #endregion

        private void WriteComment(string p)
        {
            writer.WriteComment(string.Format("{0}", p));
        }
    }

}
