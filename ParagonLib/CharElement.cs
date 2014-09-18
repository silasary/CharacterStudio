using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    public class CharElement
    {
        public List<CharElement> Children = new List<CharElement>();

        public CharElement(string id, int p, Workspace workspace, RulesElement re)
        {
            RulesElementId = id;
            SelfId = p;
            this.workspace = workspace;
            this.RulesElement = re;
        }
        public string RulesElementId {get; private set;}

        public int SelfId { get; set; }

        public void Grant(string name, string type, string requires, string Level)
        {
            var child = RuleFactory.New(name, workspace, type);
            this.Children.Add(child);
            //child.Parent = this;
            
        }

        public void Select(string name, string number, string type, string requires, string Level)
        {
            throw new NotImplementedException();
        }


        public Workspace workspace { get; set; }

        internal RulesElement RulesElement { get; set; }
    }
}
