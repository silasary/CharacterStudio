using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    /// <summary>
    /// Base class for a character.
    /// </summary>
    [DataContract(Name="D20Character", Namespace="")]
    public class Character
    {
        public Workspace workspace;

        public string Name { get; set; }

        public Character(string System)
        {
            workspace = new Workspace(System, this);
        }

        public void Save(string savefile)
        {
            if (!Path.IsPathRooted(savefile))
                savefile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Saved Characters", savefile);
            string folder;
            if (!Directory.Exists(folder = Path.GetDirectoryName(savefile)))
                Directory.CreateDirectory(folder);
            var s = File.OpenWrite(savefile);
            new System.Runtime.Serialization.DataContractSerializer(typeof(Character)).WriteObject(s, this);
            s.Close();
        }

    }
}
