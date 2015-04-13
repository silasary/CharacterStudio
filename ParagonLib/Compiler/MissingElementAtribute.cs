using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.Compiler
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class MissingElementAttribute : Attribute
    {
        readonly string element;
        readonly string value;

        public MissingElementAttribute(bool specific, string element, string value)
        {
            this.element = element;
            this.value = value;
        }

        public string Element
        {
            get { return element; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}
