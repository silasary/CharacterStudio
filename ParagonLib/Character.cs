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
    /// <summary>
    /// Base class for a character.
    /// </summary>
    [DataContract(Name="D20Character")]
    public class Character
    {
        [DataMember]
        public Workspace workspace;
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        List<Adventure> AdventureLog { get { return workspace.AdventureLog; } set { workspace.AdventureLog = value; } }

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
//            var s = File.OpenWrite(savefile);
//            new DataContractSerializer(typeof(Character), new DataContractSerializerSettings() { DataContractResolver = new ContractResolver() }).WriteObject(s, this);
//            s.Close();
            Serializer.Save(this, savefile);
        }
    }
}
