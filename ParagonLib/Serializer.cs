using ParagonLib.CharacterData;
using ParagonLib.RuleBases;
using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CharacterStudio.Rules;

namespace ParagonLib
{
	public class Serializer
	{
        /// <summary>
        /// 0.07a is OCB/CBLoader.
        /// 0.07a is also Silverlight [Henceforth called 0.07b for sanity reasons]
        /// 0.08a is CharacterStudio Alpha format.
        /// </summary>
        public enum SFVersion { v007a, v007b, v008a }
        const SFVersion PreferedSaveFileVersion = SFVersion.v008a;
        static readonly string[] D20AbilityScores = new string[] { "Strength", "Constitution", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
        SFVersion SaveFileVersion;

        XmlWriter writer;
        Character c;

        List<Loot> AllLoot = new List<Loot>();

        public bool Save(Character c, string savefile)
        {
            this.c = c;
            c.workspace.Recalculate(true);
            SaveFileVersion = PreferedSaveFileVersion;
            if (Path.GetExtension(savefile) == ".dnd4e")
                SaveFileVersion = SFVersion.v007a;
            writer = XmlWriter.Create(savefile, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration=true });
            writer.WriteStartDocument( );
            writer.WriteStartElement("D20Character");
            writer.WriteAttributeString("xml", "space", null, "preserve");
            writer.WriteAttributeString("game-system", c.workspace.System);
            writer.WriteAttributeString("Version", SaveFileVersion.ToString().Replace("v0","0.").Replace("0.07b","0.07a")); // Both the OCB and NCB report 0.07a.  [Why do people even bother versioning things if they're not going to bump the version?] 
            if (SaveFileVersion < SFVersion.v008a)
                WriteComment("Character Studio character save file.  \n Schema compatibile with the Dungeons and Dragons Insider: Character Builder");
            else
                WriteComment("Character Studio character save file.  \n *** Incompatibile with the Dungeons and Dragons Insider: Character Builder ***");
            WriteCharacterSheet();
            writer.Flush();
            WriteD20CampaignSetting();

            if (SaveFileVersion >= SFVersion.v008a)
            {
                WriteChoices();
                
            }
            //else
                WriteLevels();

            c.TextStrings["Character Save File"] = savefile; 
            WriteTextStrings();

            writer.WriteEndElement( );
            writer.WriteEndDocument( );
            writer.Close( );
            return true;
        }

        private void WriteChoices()
        {
            writer.WriteStartElement("Choices");
            foreach (var item in c.workspace.Choices)
            {
                var parent = item.Value.Parent;
                if (parent == null)
                    continue;
                writer.WriteStartElement("Choice");
                writer.WriteAttributeString("hash", item.Key);
                writer.WriteAttributeString("Type", item.Value.Type);
                writer.WriteAttributeString("Parent", parent.RulesElementId);
                writer.WriteAttributeString("Value", item.Value.Value);
                writer.WriteEndElement( );
            }
            writer.WriteEndElement();
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

       

        public Character Load(string savefile)
        {
            var doc = XDocument.Load(savefile);
            SaveFileVersion = (SFVersion)Enum.Parse(typeof(SFVersion), doc.Root.Attribute("Version").Value.Replace("0.", "v0"));
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
                        // These things have so many different meanings :/
                        ReadTextString(node);
                        break;
                    default:

                        break;
                }
            }
            // Done reading everything.  Time to do housekeeping:
            c.workspace.Recalculate(true);
            ValidateExperiencePoints();
            AssignLostLootToAdventures( );                    
            c.Save("Temp"); // Latest version of interpreted save file.
            c.Save("Temp.dnd4e"); // DDI-compatible version of interpreted save file.
            return c;
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



        #region CharacterSheet
        private void ReadCharacterSheet(XElement node)
        {
            // Despite what is mentioned in the comments, 
            // we do actually need to pull things from here.
            var xdetails = node.Element("Details");
            c.TextStrings["Experience Points"] = xdetails.Element("Experience").Value;
            c.Name = xdetails.Element("name").Value.Trim();
            var props = new string[]
            { 
                /* "name", "Level", */ "Player", "Height", "Weight", /* "Gender", "Alignment", */ "Company", "Portrait",
                /* "Experience", */ "CarriedMoney", "StoredMoney", "Traits", "Appearance", "Companions", "Notes"
            };
            foreach (var p in props)
            {
                var prop = typeof(Character).GetProperty(p);
                if (prop == null)
                    c.TextStrings[p] = xdetails.Element(p).Value;
                else
                    prop.SetValue(c, xdetails.Element(p).Value.Trim( ));
            }

            var xscores = node.Element("AbilityScores");
            foreach (var score in D20AbilityScores)
            {
                c.AbilityScores[score] = int.Parse(xscores.Element(score).Attribute("score").Value);
            }
        }

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
            WriteCompanions();
            //Journal.  Our Adventure Log is far superior to the original, But it's also vital we remain Backwards-Compatible.
            WriteJournal();

            WriteComment("WARNING: This character sheet is incomplete.");
            writer.WriteEndElement( );
        }

        private void WriteCompanions()
        {
            writer.WriteStartElement("Companions");
            writer.WriteRaw("\n    ");
            writer.WriteEndElement();
        }

        private void WriteJournal()
        {
            //This instance of the journal is used for External Reference Only 
            // This makes sense considering that they put it in the <CharacterSheet> tag.  
            // DDI's builder keeps the 'real' Entries as escaped XML in a Textstring. [For the record, that's really gross]
            writer.WriteStartElement("Journal");
            //var ser = new DataContractSerializer(typeof(Adventure));
            foreach (var entry in c.workspace.AdventureLog)
            {
                //ser.WriteObject(writer, entry);
                SerializeAdventure(entry, false);
            }
            if (c.workspace.AdventureLog.Count==0)
                writer.WriteRaw("\n    ");

            writer.WriteEndElement();
        }

        private void WritePowerStats()
        {
            //0.07a includes basic stat calculations - Attack, Damage, and defences.
            //0.07b Includes more detailed data. 
            WriteComment("\n         The fields for your powers. Each power is then followed\n         by the stats with that power paired with each legal weapon.\n         The weapons are listed in priority as the builder sees it.\n         Particularly, the first weapon listed is the default.\n      ");
            //TODO: Power Blocks!  VERY IMPORTANT. (for iPlay4e)
            writer.WriteStartElement("PowerStats");
            foreach (var power in c.workspace.AllElements.Where(e=> e.Type == "Power"))
            {
                var PowerInfo = power.RulesElement as IPower;
                if (PowerInfo != null)
                {
                    writer.WriteStartElement("Power");
                    writer.WriteAttributeString("name", power.Name);
                    writer.WriteStartElement("specific");
                    writer.WriteAttributeString("name", "Power Usage");
                    writer.WriteRaw(PowerInfo.PowerUsage);
                    writer.WriteFullEndElement();
                    writer.WriteStartElement("specific");
                    writer.WriteAttributeString("name", "Action Type");
                    writer.WriteRaw(PowerInfo.ActionType);
                    writer.WriteFullEndElement();

                // Specifics.  Minimum required: "Power Usage", "Action Type".  Ideally, everything else.
                // Foreach Weapon, Stats.
                //TODO: FIX
                    //if (PowerInfo.AttackComponents != null)
                    //foreach (var weapon in c.Weapons)
                    //{
                    //    writer.WriteStartElement("Weapon");
                    //    writer.WriteAttributeString("name", weapon.Name);
                    //    SerializeItem(weapon, false);
                    //    writer.WriteElementString("AttackBonus", 0.ToString());
                    //    writer.WriteElementString("Damage", PowerInfo.DamageComponents.Damage);
                    //    if (!string.IsNullOrEmpty(PowerInfo.DamageComponents.type))
                    //        writer.WriteElementString("DamageType", PowerInfo.DamageComponents.type);

                    //    //PowerInfo.AttackComponents[0].
                    //    writer.WriteEndElement();
                    //}
                writer.WriteFullEndElement( );
                }
            }
            //Foreach power
            writer.WriteFullEndElement();
        }

        private void WriteLootTally()
        {
            WriteComment("\n         The tally of the character&apos;s loot\n      ");
            writer.WriteStartElement("LootTally");
            //TODO: Loot!
            foreach (var item in c.Loot)
            {
                writer.WriteStartElement("loot");
                writer.WriteAttributeString("count", item.Value.Count.ToString());
                //writer.WriteAttributeString("equip-count", item.Value.EquipCount.ToString());
                //writer.WriteAttributeString("ShowPowerCard", item.Value.ShowPowerCard);
                
                SerializeItem(item.Value,true);
                writer.WriteEndElement();
            }
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
            foreach (var ele in c.workspace.AllElements)
            {
                if (ele.Disabled)
                    continue;
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

                // TODO: compendium URL attribute.
                // Legality
                if (IncludeDetails && !String.IsNullOrEmpty(ele.RulesElement.ShortDescription))
                {
                    writer.WriteStartElement("specific");
                    writer.WriteAttributeString("name", "Short Description");
                    writer.WriteString(ele.RulesElement.ShortDescription);
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Attaches RulesElements to the inside of a <loot/> tag.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="IncludeDetails"></param>
        private void SerializeItem(InventoryItem item, bool IncludeDetails, XmlWriter writer = null)
        {
            if (writer == null)  // We can blame the serialized XML journal Entries for this.
                writer = this.writer;
            if (item == null) return;
            Dictionary<string,RulesElement> rules = new Dictionary<string,RulesElement>();
            rules[item.baseId]=item.Base;
            if (!string.IsNullOrEmpty(item.enchantmentId))
                rules[item.enchantmentId]=item.Enchantment;
            if (!string.IsNullOrEmpty(item.curseId))
                rules[item.curseId] = item.Curse;
            
            foreach (var rule in rules)
            {
                writer.WriteStartElement("RulesElement");
                if (rule.Value != null)
                {
                    writer.WriteAttributeString("name", rule.Value.Name);
                    writer.WriteAttributeString("type", rule.Value.Type);
                }
                else
                {
                    writer.WriteAttributeString("name", "Unknown");
                    var inferredtype = rule.Key.Split('_')[2];
                    inferredtype = char.ToUpper(inferredtype[0]) + inferredtype.Substring(1).ToLower();
                    if (inferredtype == "Magic")
                        inferredtype = "Magic Item";
                    writer.WriteAttributeString("type", inferredtype);                 
                }
                writer.WriteAttributeString("internal-id", rule.Key);
                //writer.WriteAttributeString("charelem", ele.SelfId.ToString());            
                writer.WriteEndElement();
            }
         }

        private void WriteDetails()
        {
            // 0.07[ab] both agree on the format here.
            // The difference is that 7b cares about this, while 7a uses TextStrings.
            writer.WriteStartElement("Details");

            var props = new string[]
                { 
                    "name", "Level", "Player", "Height", "Weight", "Gender", "Alignment", "Company", "Portrait",
                    "Experience", "CarriedMoney", "StoredMoney", "Traits", "Appearance", "Companions", "Notes"
                };
            foreach (var p in props)
            {
                switch (p)
                {
                    case"name":
                        writer.WriteElementString("name", c.Name); // This one is lowercase, whilst everything else is uppercase.  It confuses me.              
                        break;
                    case "Level":
                        writer.WriteElementString("Level", c.workspace.Level.ToString( ));
                        break;
                        // It's worth noting that I have never seen these two nodes filled in.
//                    case "Alignment": 
//                        break;
//                    case "Gender":
//                        break;
                    case "Experience":
                        writer.WriteElementString("Experience", c.workspace.GetStat("XP Earned").Value.ToString( ));
                        break;
                    default:
                        var prop = typeof(Character).GetProperty(p);
                        if (prop == null)
                        {
                            if (c.TextStrings.ContainsKey(p))
                            {
                                writer.WriteStartElement(p);
                                writer.WriteString(c.TextStrings[p]);
                                writer.WriteFullEndElement();
                                
                            }
                            else
                            {
                                writer.WriteStartElement(p);
                                writer.WriteWhitespace("  ");
                                writer.WriteFullEndElement();

                            }
                        }
                        else
                            writer.WriteElementString(p, (string)prop.GetValue(c));
                        break;
                }
            }
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
        #region D20Campaign
        private void WriteD20CampaignSetting()
        {

            writer.WriteStartElement("D20CampaignSetting");
            if (c.workspace.Setting == null)
            {
                writer.WriteAttributeString("name", "");
                WriteComment("\n         Character Builder campaign save file.\n      ");
            }
            else
            {
                // We embed the URL for the Campaign Setting into a setting-specific Rules Element.
                writer.WriteAttributeString("name", c.workspace.Setting.Name);
                WriteComment("\n         Character Builder campaign save file.\n      ");
                writer.WriteStartElement("Houserules");
                writer.WriteStartElement("RulesElement");
                writer.WriteAttributeString("name", "UpdateURL");
                writer.WriteAttributeString("type", "Internal"); // It's an Internal, so it doesn't mess with anything.
                writer.WriteAttributeString("internal-id", "ID_INTERNAL_INTERNAL_UPDATEURL");
                writer.WriteString(c.workspace.Setting.UpdateUrl);
                writer.WriteEndElement( );
                writer.WriteEndElement( );
            }
            writer.WriteEndElement();
        }
        private void ReadD20CampaignSetting(XElement node)
        {
            // Ideally, we match these to a Campaign Setting file.
            // If we fail, load it from in here.
            // Note that DDI will crash if we insert anything other than the 
            // individual <Restricted> entries, so most of this data is useless.
            var setting = node.Attribute("name").Value;

            // But we cheat and define a 'Houseruled Element' of type 'Internal' [Thereby not affecting anything]
            // And store a URL inside it.
            var updateurl = node.Descendants("RulesElement").FirstOrDefault(re => re.Attribute("internal-id").Value == "ID_INTERNAL_INTERNAL_UPDATEURL");
            if (updateurl != null)
            {
                var url = updateurl.Value.Trim();
                c.workspace.Setting = CampaignSetting.Load(setting, c.workspace.System, url);
            }
        }
        #endregion
        #region Levels
        private void WriteLevels()
        {
            foreach (var level in c.workspace.Levelset.Children)
            {
                if (level.Children.Count==0)
                    continue;
                writer.WriteStartElement("Level");
                WriteRulesElementNested(level);
                if (SaveFileVersion < SFVersion.v008a)
                {
                    foreach (var loot in c.workspace.AdventureLog.SelectMany(a => a.LootDiff).Where(l => l.levelAquired.ToString() == level.Name))
                    {
                        SerializeLoot(loot);
                    }
                }
                writer.WriteFullEndElement();
            }
        }
        private void ReadLevel(XElement node)
        {
            //This is the Level Node.  It's a terrifying thing.
            // On the plus side, due to the (slightly nasty) way Grant works, 
            // we can actually infer inheritance when we call Recalculate().
            // Selections, on the other hand...
            int Level = int.Parse(node.Elements("RulesElement").FirstOrDefault().Attribute("name").Value);
            foreach (var element in node.Elements("RulesElement"))
            {
                ReadRulesElement(element, c.workspace.Levelset);
            }
            foreach (var lootnode in node.Elements("loot"))
            {
                 var loot = DeserializeLoot(lootnode);
                 loot.levelAquired = Level;
                 AllLoot.Add(loot);
            }
            //c.workspace.Recalculate();
        }

        private Loot DeserializeLoot(XElement node)
        {
            var parts = node.Elements();
            var ids = parts.Select(p => p.Attribute("internal-id").Value).ToArray();
            var id = string.Join("__", ids);
            InventoryItem item;
            if (c.Loot.ContainsKey(id))
                item = c.Loot[id];
            else
                item = c.Loot[id] = new InventoryItem(ids, c.workspace.System);
            var loot = new Loot(); // We use += below so we don't need explicit casts.
            loot.Count += int.Parse(node.Attribute("count").Value);
            loot.Equipped += int.Parse(node.Attribute("equip-count").Value);
            loot.ShowPowerCard += int.Parse(node.Attribute("ShowPowerCard").Value);
            if (node.Attribute("Silver") != null)
                loot.Silvered += int.Parse(node.Attribute("Silver").Value);
            loot.ItemRef = item;
            //TODO: Augments :/
            // Overrides.
            return loot;
        }
        private void SerializeLoot(Loot loot, XmlWriter writer = null)
        {
            if (writer == null)  // We can blame the serialized XML journal Entries for this.
                writer = this.writer;
            writer.WriteStartElement("loot");
            writer.WriteAttributeString("count", ((int)loot.Count).ToString());
            writer.WriteAttributeString("equip-count",  ((int)loot.Equipped).ToString());
            writer.WriteAttributeString("ShowPowerCard", ((int)loot.ShowPowerCard).ToString());
            if (loot.Silvered != 0)
                writer.WriteAttributeString("Silver", ((int)loot.Silvered).ToString());
            SerializeItem(loot.ItemRef, false, writer);
            //TODO: Augments :/
            // Overrides.
            writer.WriteEndElement();
        }

        #endregion
        #region TextStrings!

        private void WriteTextStrings()
        {
            WriteComment("\n      Textstrings are builder variables and contain entered text data, such\n      as character names, as well as internal data\n   ");
            if (SaveFileVersion < SFVersion.v008a)
            {
                SerializeTextString("Name", c.Name);
                SerializeTextString("Experience Points", c.workspace.GetStat("XP Earned").Value.ToString( ));
                SerializeTextString("Player", c.Player);
                SerializeTextString("Height", c.Height);
                SerializeTextString("Company", c.Company);
            }
            foreach (var adv in c.workspace.AdventureLog)
            {
                SerializeAdventure(adv, true);
            }
            foreach (var item in c.TextStrings) // The ones that we didn't care about.
            {
                SerializeTextString(item.Key, item.Value);
            }
        }

        private void ReadTextString(XElement node)
        {
            var name = node.Attribute("name").Value;
            var value = node.Value.Trim();
            // Journal Entries are stored here.  name=NOTE_6d0d6a19-7756-4702-9e62-34daaec6b161
            // These are unfortunately important.  We're also going to abuse them even further:
            // Extra info for the AdventureLog needs to survive a round trip, 
            // so we'll use name=EXT_{GUID}
            if (name.StartsWith("NOTE_") || name.StartsWith("EXT_"))
                DeserializeAdventure(node);
            else
                switch (name)
            {
                case "Name":
                case "name":
                    //c.Name = value;
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
            Guid guid;
            if (!Guid.TryParse(gs, out guid))
            {
                // Not a Journal Entry.
                return;
            }

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
                else if (prop.PropertyType == typeof(double))
                    prop.SetValue(adv, double.Parse(item.Value));
                else if (prop.Name == "LootDiff")
                    LoadLootFromAdventure(item, adv);
                else if (prop.PropertyType == typeof(Guid))
                    prop.SetValue(adv, Guid.Parse(item.Value));
                else
                    prop.SetValue(adv, item.Value);
            }
        }

        private void LoadLootFromAdventure(XElement LootTally, Adventure adv)
        {
//TODO:             DeserializeLoot, Compare against AllLoot, Assign to Adventure.
        }

        private void SerializeAdventure(Adventure adv, bool asTextstring)
        {
            XmlWriter mainwriter;
            bool ImageEntry = !string.IsNullOrEmpty(adv.Uri) && adv.XPGain == 0;
            string[] std;
            StringBuilder sb=null;

            if (asTextstring)
            {
                sb = new StringBuilder();
                sb.Append("ENTRY:");
                mainwriter = XmlWriter.Create(sb, new XmlWriterSettings() { Indent = true });
                mainwriter.WriteStartDocument();
            }
            else
                mainwriter = this.writer;
            XmlWriter writer = mainwriter;

            mainwriter.WriteStartElement("JournalEntry");
            if (ImageEntry)
            {
                if (asTextstring)
                    mainwriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "ImageEntry");
                std = new string[] { "TimeStamp", "Title", "Uri" };
            }
            else
            {
                if (asTextstring)
                    mainwriter.WriteAttributeString("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance", "AdventureLogEntry");
                std = new string[] { "TimeStamp", "Title", "LevelAtEnd", "GPTotal", "GPDelta", "GPStart", "XPTotal", "XPGain", "XPStart", "Treasure", "Region", "Notes" };
            }

            StringBuilder sb2 = null;
            XmlWriter extrawriter = null;
            if (SaveFileVersion < SFVersion.v008a && asTextstring)
            {
                sb2 = new StringBuilder();
                extrawriter = XmlWriter.Create(sb2, new XmlWriterSettings() { Indent = true });
                extrawriter.WriteStartDocument();
                extrawriter.WriteStartElement("JournalEntry");
            }
            else
                std = typeof(Adventure).GetProperties().Select(n => n.Name).ToArray(); // v8+ just includes everything in one.

            foreach (var p in typeof(Adventure).GetProperties())
            {
                if (std.Contains(p.Name))
                    writer = mainwriter;
                else
                    writer = extrawriter;
                var v = p.GetValue(adv);
                writer.WriteStartElement(p.Name);
                if (v != null)
                {
                    if (p.PropertyType == typeof(DateTime))
                        writer.WriteValue(((DateTime)v).ToString("o"));
                    else if (p.PropertyType == typeof(Loot[]))
                    {
                        foreach (var loot in (Loot[])v) SerializeLoot(loot, writer);
                    }
                    else
                        writer.WriteValue(v.ToString());
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (asTextstring)
            {
                mainwriter.Close();
                SerializeTextString("NOTE_" + adv.guid.ToString(), sb.ToString());
                if (SaveFileVersion < SFVersion.v008a)
                {
                    extrawriter.Close();
                    SerializeTextString("EXT_" + adv.guid.ToString(), sb2.ToString());
                }
            }
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
        #endregion

        #region Validation

        private void ValidateExperiencePoints()
        {

            int val;
            int.TryParse(c.TextStrings["Experience Points"], out val);
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
        private void AssignLostLootToAdventures()
        {
            var AssignedLoot = c.workspace.AdventureLog.SelectMany(a => a.LootDiff).ToArray();
            var LostLoot = AllLoot.Where(l => !AssignedLoot.Contains(l));
            var ShoppingTrips = c.workspace.AdventureLog.Where(a => a.XPGain == 0 && string.IsNullOrEmpty(a.Uri)).ToList();
            foreach (var loot in LostLoot)
            {
                var trip = ShoppingTrips.FirstOrDefault(a => a.LevelAtEnd == loot.levelAquired);
                if (trip == null)
                {
                    trip = new Adventure() { Title = "Shopping Trip", LevelAtEnd = loot.levelAquired };
                    ShoppingTrips.Add(trip);
                    var next = c.workspace.AdventureLog.FirstOrDefault(a => a.LevelAtEnd > loot.levelAquired);
                    if (next == null)
                        c.workspace.AdventureLog.Add(trip);
                    else
                        c.workspace.AdventureLog.Insert(c.workspace.AdventureLog.IndexOf(next), trip);
                }
                var total = trip.LootDiff.ToList( );
                total.Add(loot);
                trip.LootDiff = total.ToArray( );
            }
        }

        #endregion

        private void WriteComment(string p)
        {
            writer.WriteComment(string.Format("{0}", p));
        }

        public static IEnumerable<string> KnownFiles
        {
            get
            {
                List<string> files = new List<string>();
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "builder_known_files.txt");
                if (File.Exists(path))
                    files.AddRange(File.ReadAllLines(path));
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "builder_known_files.txt");
                if (File.Exists(path))
                    files.AddRange(File.ReadAllLines(path));
                return files.Distinct().Where(f => File.Exists(f));
            }
        }

        public static IEnumerable<string> KnownFolders
        {
            get
            {
                List<string> files = new List<string>();
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "builder_known_files.txt");
                if (File.Exists(path))
                    files.AddRange(File.ReadAllLines(path));
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "builder_known_files.txt");
                if (File.Exists(path))
                    files.AddRange(File.ReadAllLines(path));
                return files.Distinct().Where(f => Directory.Exists(f));
            }
        }

        public static bool AddKnown(string FileFolder)
        {
            if (Path.GetFileNameWithoutExtension(FileFolder) == "Temp")
                return false;
            return false;
        }
    }
}
