using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ParagonLib.CharacterData
{
    /// <summary>
    /// Base class for a character.
    /// </summary>
    public class Character : INotifyPropertyChanged
    {

        public string Company { get; set; }

        public string Height { get; set; }

        public Dictionary<string, InventoryItem> Loot { get; protected set; }

        public string Name { get { return name; } set { SetField(ref name, value); } }

        public string Player { get { return player; } set { SetField(ref player, value); } }

        public string Portrait { get { return portrait; } set { SetField(ref portrait, value); } }

        public string Class { get { return _class; } set { SetField(ref _class, value); } }

        public Dictionary<string, int> AbilityScores = new Dictionary<string, int>();
        /// <summary>
        /// These things are nasty, and for internal storage.  Try not to use them where possible.
        /// </summary>
        public Dictionary<string, string> TextStrings = new Dictionary<string, string>();

        public IEnumerable<InventoryItem> Weapons
        {
            get
            {
                var weapons = Loot.Where(i => i.Value.Type == "Weapon").ToArray();
                var implements = Loot.Where(i => i.Value.Type == "Implement" || i.Value.Type == "Superior Implement").ToArray();
                foreach (var q in weapons)
                {
                    var w = q.Value;
                    if (!(q.Value is Weapon))
                    {
                        w = Loot[q.Key] = new Weapon(q.Value); // Upgrade a generic Loot into a Weapon.
                    }
                    yield return w;
                }
                foreach (var q in implements)
                {
                    var w = q.Value;
                    if (!(q.Value is Implement))
                    {
                        w = Loot[q.Key] = new Implement(q.Value);
                    }
                    if (((Implement)w).IsWeapon)
                        yield return w;
                }

                //return weapons.Select(i => i.Value);
            }
        }

        public Workspace workspace;
        List<Adventure> AdventureLog { get { return workspace.AdventureLog; } set { workspace.AdventureLog = value; } }

        public Character(string System)
        {
            workspace = new Workspace(System, this);
            Loot = new Dictionary<string, InventoryItem>();
            foreach (var abil in Workspace.D20AbilityScores)
            {
                AbilityScores[abil] = 10;
            }
        }

        // TODO: Still not the best place.
        public static string DefaultPortrait(string Class, string Race, string Gender)
        {
            var ClassPort = String.Format("ClassPort{0}", Class).ToLower();
            var RaceGendered = string.Format("{0}_{1}", (Gender ?? "").FirstOrDefault(), Race).ToLower();
            var RacePort = string.Format("RacePort{0}", Race).ToLower();
            var Generic = "Generic".ToLower();
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Character Portraits");
            Directory.CreateDirectory(folder);
            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Select(n => n.ToLower());
            var port = files.Where(n => n.Contains(ClassPort)).FirstOrDefault()
                ?? files.Where(n => n.Contains(RaceGendered)).FirstOrDefault()
                ?? files.Where(n => n.Contains(RacePort)).FirstOrDefault()
                ?? files.Where(n => n.Contains(Generic)).FirstOrDefault();
            return port;
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
            new Serializer().Save(this, savefile);
            Serializer.AddKnown(savefile);
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private string name;
        private string player;
        private string portrait;
        private string _class;

        void SetField<T>(ref T field, T value, [CallerMemberName] string name="")
        {
            field = value;
            if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
