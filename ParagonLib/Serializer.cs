using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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
        static readonly string[] D20AbilityScores = new string[] { "Strength", "Constitution", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
        string SaveFileVersion;

        XmlWriter writer;
        Character c;

        public bool Save(Character c, string savefile)
        {
            this.c = c;
            c.workspace.Recalculate(true);
            SaveFileVersion = PreferedSaveFileVersion;
            if (Path.GetExtension(savefile) == ".dnd4e")
                SaveFileVersion = "0.07a";
            writer = XmlWriter.Create(savefile, new XmlWriterSettings() { Indent = true });
            writer.WriteStartDocument( );
            writer.WriteStartElement("D20Character");
            writer.WriteAttributeString("game-system", c.workspace.System);
            writer.WriteAttributeString("Version", SaveFileVersion); // Both the OCB and NCB report 0.07a.  [Why do people even bother versioning things if they're not going to bump the version?] 
            WriteComment("Character Studio character save file.  Schema compatibile with the Dungeons and Dragons Insider: Character Builder");

            WriteCharacterSheet();
            writer.Flush();
            WriteD20CampaignSetting();

            WriteLevels();

            c.TextStrings["Character Save File"] = savefile; 
            WriteTextStrings();

            writer.WriteEndElement( );
            writer.WriteEndDocument( );
            writer.Close( );
            return true;
        }

        private void WriteTextStrings()
        {
            WriteComment("\n      Textstrings are builder variables and contain entered text data, such\n      as character names, as well as internal data\n   ");
            SerializeTextString("Name", c.Name);
            SerializeTextString("Experience Points", c.workspace.GetStat("XP Earned").Value.ToString());
            foreach (var adv in c.workspace.AdventureLog)
            {
                SerializeAdventure(adv);
            }
            foreach (var item in c.TextStrings) // The ones that we didn't care about.
            {
                SerializeTextString(item.Key, item.Value);
            }
        }

        private void SerializeAdventure(Adventure adv)
        {
            var sb = new StringBuilder();
            sb.Append("ENTRY:");
            var writer = XmlWriter.Create(sb, new XmlWriterSettings() { Indent=true });
            bool ImageEntry = !string.IsNullOrEmpty(adv.Uri) && adv.XPGain == 0;
            writer.WriteStartDocument();
            writer.WriteStartElement("JournalEntry");
            //writer.WriteAttributeString("xsd", "x", "http://www.w3.org/2001/XMLSchema",""); //TODO: Specify prefix???
            
            if (ImageEntry)
                writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "ImageEntry");
            else
                writer.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "AdventureLogEntry");
            foreach (var p in typeof(Adventure).GetProperties())
            {
                var v = p.GetValue(adv);
                    writer.WriteStartElement(p.Name);
                    if (v != null)
                    {
                        if (p.PropertyType == typeof(DateTime))
                            writer.WriteValue(((DateTime)v).ToString("o"));
                        else
                            writer.WriteValue(v.ToString());
                    }
                    writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            SerializeTextString("NOTE_" + adv.guid.ToString(), sb.ToString());
        }

        /// <summary>
        /// This one does the brunt work of Serializing TextStrings.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void SerializeTextString(string name, string value)
        {
            writer.WriteStartElement("textstring");
            writer.WriteAttributeString("name", name);
            writer.WriteValue(string.Format("\n      {0}\n   ", value));
            writer.WriteEndElement();
        }

        private void WriteLevels()
        {
            foreach (var level in c.workspace.Levelset.Children)
            {
                writer.WriteStartElement("Level");
                WriteRulesElementNested(level);
                writer.WriteEndElement();
            }
        }

        private void WriteRulesElementNested(CharElement ele)
        {
            writer.WriteStartElement("RulesElement");

            SerializeRuleElement(ele, false);
            foreach (var child in ele.Children)
            {
                WriteRulesElementNested(child);
            }
            writer.WriteEndElement();
        }

        private void WriteD20CampaignSetting()
        {

            writer.WriteStartElement("D20CampaignSetting");
            if (c.workspace.Setting == null)
                writer.WriteAttributeString("name", "");
            else
                writer.WriteAttributeString("name", c.workspace.Setting.Name);
            WriteComment("\n         Character Builder campaign save file.\n      ");
            //TODO: Write URL.
            /*
               <Houserules>
                <RulesElement name="UpdateURL" type="Internal" internal-id="ID_INTERNAL_INTERNAL_UPDATEURL" >
                 https://dl.dropboxusercontent.com/u/4187827/CharBuilder/Eberron/eberron.setting
                </RulesElement>
               </Houserules>
             */
            writer.WriteEndElement();
        }

        public Character Load(string savefile)
        {
            var doc = XDocument.Load(savefile);
            SaveFileVersion = doc.Root.Attribute("Version").Value;
            var system = doc.Root.Attribute("game-system").Value;
            c = new Character(system);
            foreach (var node in doc.Root.Elements())
            {
                switch (node.Name.LocalName)
                {
                    case "CharacterSheet": 
                        ReadCharacterSheet(node); // Documentation actually lies to us.  
                        break;                    // We store the base ability scores here.
                    case "D20CampaignSetting":
                        ReadD20CampaignSetting(node);
                        break;
                    case "Level": // This is the beefy one.
                        ReadLevel(node);
                        break;
                    case "textstring":
                        // TODO: These things have so many different meanings :/
                        ReadTextString(node);
                        break;
                    default:

                        break;
                }
            }
            // Done reading everything.  Time to do housekeeping:
            c.workspace.Recalculate(true);
            ValidateExperiencePoints();
                    
            c.Save("Temp");
            c.Save("Temp.dnd4e");
            return c;

        }

        private void ValidateExperiencePoints()
        {
            int val = int.Parse(c.TextStrings["Experience Points"]);
            if (val != c.workspace.GetStat("XP Earned").Value)
            {
                if (c.workspace.AdventureLog.Count==0)
                c.workspace.AdventureLog.Add(new Adventure()
                {
                    Title = "Early Adventures",
                    XPGain = val,
                    Notes = "This is a generic entry explaining the XP that was earned before I got my Journal."
                });
                else
                    c.workspace.AdventureLog.Add(new Adventure()
                    {
                        Title = "Missing Adventures",
                        XPGain = val,
                        Notes = "This is a generic entry explaining the XP that I forgot to write in my Journal."
                    });
            }
            c.TextStrings.Remove("Experience Points");
        }

        private void ReadCharacterSheet(XElement node)
        {
            // Despite what is mentioned in the comments, 
            // there is one node here that is actually used:
            var xscores = node.Element("AbilityScores");
            foreach (var score in D20AbilityScores)
            {
                c.AbilityScores[score] = int.Parse(xscores.Element(score).Attribute("score").Value);
            }
        }

        private void ReadTextString(XElement node)
        {
            var name = node.Attribute("name").Value;
            var value = node.Value.Trim();
            // TODO: Journal Entries are stored here.  name=NOTE_6d0d6a19-7756-4702-9e62-34daaec6b161
            // These are unfortunately important.  We're also going to abuse them even further:
            // Extra info for the AdventureLog needs to survive a round trip, 
            // so we'll use name=EXT_{GUID}
            if (name.StartsWith("NOTE_") || name.StartsWith("EXT_"))
                DeserializeAdventure(node);
            else
            switch (name)
            {
                case "Name":
                    c.Name = value;
                    break;
                case "Experience Points":  //We'll deal with this later.
                    c.TextStrings[name] = value;
                    break;
                default:
                    c.TextStrings[name] = value;
                    break;
            }
            
        }

        private void DeserializeAdventure(XElement node)
        {
            var name = node.Attribute("name").Value;
            var gs = name.Substring(4).Trim('_'); // This works for both NOTE_ and EXT_, leaving just the guid.
            Guid guid = Guid.Parse(gs);
            var adv = c.workspace.AdventureLog.FirstOrDefault(n => n.guid == guid);
            if (adv == null)
                c.workspace.AdventureLog.Add(adv = new Adventure(guid));
            var value = node.Value.Trim();
            if (value.StartsWith("ENTRY:"))
                value = value.Substring(6);
            var xml = XDocument.Parse(value);
            foreach (var item in xml.Root.Elements())
            {
                var prop = typeof(Adventure).GetProperty(item.Name.LocalName);
                if (prop == null)
                    continue;
                if (string.IsNullOrEmpty(item.Value))
                    continue;
                if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(adv, DateTime.Parse(item.Value));
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(adv, int.Parse(item.Value));
                else
                    prop.SetValue(adv, item.Value);
            }
        }

        private void ReadLevel(XElement node)
        {
            //This is the Level Node.  It's a terrifying thing.
            // On the plus side, due to the (slightly nasty) way Grant works, 
            // we can actually infer inheritance when we call Recalculate().
            // Selections, on the other hand...
            foreach (var element in node.Elements("RulesElement"))
            {
                ReadRulesElement(element, c.workspace.Levelset);
            }
            //c.workspace.Recalculate();
        }
        int GenericNegativeNumber = -1;
        private void ReadRulesElement(XElement element, CharElement parent)
        {
            if (element.Attribute("internal-id") == null)
                return; // Empty Choice.
            var ruleid = element.Attribute("internal-id").Value;
            int charid;
            if (!int.TryParse(element.Attribute("charelem").Value, out charid))
                charid = GenericNegativeNumber--;
            var child = new CharElement(ruleid, charid, c.workspace, RuleFactory.FindRulesElement(ruleid, c.workspace.System));
            child.Name = element.Attribute("name").Value;
            child.Type = element.Attribute("type").Value;
            if (parent != null)
                parent.Children.Add(child);
            child.Method = CharElement.AquistitionMethod.Unknown;
            foreach (var xc in element.Elements("RulesElement"))
            {
                ReadRulesElement(xc, child);
            }
        }

        private void ReadD20CampaignSetting(XElement node)
        {
            // Ideally, we match these to a Campaign Setting file.
            // If we fail, load it from in here.
            // Note that DDI will crash if we insert anything other than the 
            // individual <Restricted> entries, so most of this data is useless.
            var setting = node.Attribute("name").Value;

            c.workspace.Setting = CampaignSetting.Load(setting, c.workspace.System);
            if (c.workspace.Setting == null)
            {
                // We can cheat though:
                var updateurl = node.Descendants("RulesElement").FirstOrDefault(re => re.Attribute("internal-id").Value == "ID_INTERNAL_INTERNAL_UPDATEURL");
                // We define a 'Houseruled Element' of type 'Internal' [Thereby not affecting anything]
                if (updateurl == null)
                    return;
                // And store a URL inside it.
                var url = updateurl.Value;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    url = url.Trim();
                    // TODO:  Download and Apply.
                    var wc = new System.Net.WebClient();
                    var file = Path.Combine(RuleFactory.SettingsFolder, setting + ".setting");
                    Directory.CreateDirectory(RuleFactory.SettingsFolder);
                    wc.DownloadFileCompleted += (o, e) => 
                    {
                        if (e.Error != null)
                            return;
                        RuleFactory.LoadFile(file);
                        c.workspace.Setting = CampaignSetting.Load(setting, c.workspace.System);
                    };
                    wc.DownloadFileAsync(new Uri(url), file);
                }
            }
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
            if (c.workspace.AdventureLog.Count==0)             writer.WriteRaw("\n    ");

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
            writer.WriteRaw("\n    ");
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
                SerializeRuleElement(ele, true);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Attaches the attributes to a RulesElement.  Called for both RulesElementTally and WriteLevels.
        /// </summary>
        /// <param name="ele">CharElement to serialize</param>
        /// <param name="IncludeDetails">The Tally includes specifics, while Levels doesn't.</param>
        /// <remarks>This method just attaches the attributes.  It's up to you to Open and Close the Xml Element.</remarks>
        private void SerializeRuleElement(CharElement ele, bool IncludeDetails)
        {
            writer.WriteAttributeString("name", ele.Name);
            writer.WriteAttributeString("type", ele.Type);
            writer.WriteAttributeString("internal-id", ele.RulesElementId);
            writer.WriteAttributeString("charelem", ele.SelfId.ToString());
            if (ele.RulesElement == null)
                writer.WriteAttributeString("loaded", "false");
            else
            {

                // TODO: URL
                // Legality
                if (IncludeDetails && ele.RulesElement.Specifics.ContainsKey("Short Description"))
                {
                    writer.WriteStartElement("specific");
                    writer.WriteAttributeString("name", "Short Description");
                    writer.WriteString(ele.RulesElement.Specifics["Short Description"].LastOrDefault());
                    writer.WriteEndElement();
                }
            }
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
            foreach (var score in D20AbilityScores)
            {
                writer.WriteStartElement(score);
                writer.WriteAttributeString("score", c.AbilityScores[score].ToString());
                writer.WriteEndElement( );
            }
            writer.WriteEndElement( );
        }

        private void WriteStatBlock()
        {
            WriteComment("Final computed stat values - the various numbers\n         on the character sheet are here along with behind the scenes\n         values to build them.");
            writer.WriteStartElement("StatBlock");
            foreach (var stat in c.workspace.Stats.Values.ToArray().Distinct()) 
            {
                stat.Write(writer, SaveFileVersion);
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
