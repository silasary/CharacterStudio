using ParagonLib.RuleBases;
using ParagonLib.RuleEngine;
using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib.CharacterData
{
    class Implement : Item
    {
        public Implement(Item item) : base(item)
        {
            weaponid = ((IImplement)this.Base).WeaponEquiv;
        }

        internal string weaponid;

        public bool IsWeapon { get { return string.IsNullOrEmpty(weaponid); } }

        private RulesElement _weapon = null;
        public RulesElement WeaponEquiv
        {
            get
            {
                if (_weapon == null)
                    _weapon = RuleFactory.FindRulesElement(weaponid, GameSystem);
                return _weapon;
            }
        }
    }
}
