using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharacterStudio.Rules;

namespace ParagonLib.RuleBases
{
    public class Implement : Item, IImplement
    {
        protected string weaponEquiv;

        public string WeaponEquiv
        {
            get { return weaponEquiv; }
        }
#region _GENERATED_

protected override string GetSpecific(string specific) {
    return base.GetSpecific(specific);
}
#endregion _GENERATED_
	}
}
