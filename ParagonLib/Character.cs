using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ParagonLib
{
    /// <summary>
    /// Base class for a character.
    /// </summary>
    public class Character
    {
        
        public Workspace workspace;

        /// <summary>
        /// These things are nasty, and for internal storage.  Try not to use them where possible.
        /// </summary>
        public Dictionary<string, string> TextStrings = new Dictionary<string, string>();

        public string Name { get; set; }

        public string Player { get; set; }

        public string Height { get; set; }

        public string Company { get; set; }

        public Dictionary<string, Item> Loot { get; protected set; }

        //public int StartingXp { get; set; }

        List<Adventure> AdventureLog { get { return workspace.AdventureLog; } set { workspace.AdventureLog = value; } }

        public Character(string System)
        {
            workspace = new Workspace(System, this);
            Loot = new Dictionary<string, Item>();
            foreach (var abil in Workspace.D20AbilityScores)
            {
                AbilityScores[abil] = 10;
            }
        }

        public void Save(string savefile)
        {
            if (!Path.HasExtension(savefile))
                savefile = Path.ChangeExtension(savefile, ".D20Character");
            if (!Path.IsPathRooted(savefile))
                savefile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Saved Characters", savefile);
            string folder;
            if (!Directory.Exists(folder = Path.GetDirectoryName(savefile)))
                Directory.CreateDirectory(folder);
//            var s = File.OpenWrite(savefile);
//            new DataContractSerializer(typeof(Character), new DataContractSerializerSettings() { DataContractResolver = new ContractResolver() }).WriteObject(s, this);
//            s.Close();
            new Serializer().Save(this, savefile);
        }

        public Dictionary<string, int> AbilityScores = new Dictionary<string, int>();

    }
}
