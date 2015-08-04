using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterStudio.Rules
{
    public interface IImplement : IItem
    {
        string WeaponEquiv { get; }
    }
}
