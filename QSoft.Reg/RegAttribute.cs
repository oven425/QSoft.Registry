﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Registry.Linq
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RegIgnore : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RegPropertyName : Attribute
    {
        public string Name { set; get; }
        public object Default { set; get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RegSubKeyName:Attribute
    {
        public bool IsFullName { set; get; } = false;
        public bool AutoGenerate { set; get; } = true;
    }
}
