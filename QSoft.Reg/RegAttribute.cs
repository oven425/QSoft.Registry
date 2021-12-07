using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Registry.Linq
{
    public class RegIgnore : Attribute
    {

    }

    public class RegPropertyName : Attribute
    {
        public string Name { set; get; }
    }

    public class RegSubKeyName:Attribute
    {
    }
}
