using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    /// <summary>
    /// Base class for a character.
    /// </summary>
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
                savefile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Saved Characters");

        }

    }
}
