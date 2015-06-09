using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class Implement : Item, IImplement
    {
        protected string weaponEquiv;

        public string WeaponEquiv
        {
            get { return weaponEquiv; }
        }
    }
}
