using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib.Rules
{
    public interface IItem
    {
        string Weight { get; }
        int Gold { get; }
        int Silver { get; }
        int Copper { get;}
        string Group { get; }
        string FullText { get; }
        string ItemSlot { get; }
        int count { get; }
    }
}
