﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QSoft.Registry.Linq
{
    internal class Ex
    {
        public bool Handled { set; get; }
        public Expression SourceExpr { set; get; }
        public Expression Expr { set; get; }
        public Expression Expr1 { set; get; }
        public RegQueryConvert Convert { set; get; }
    }

    internal class EnumableExWrapper<T>
    {
        public string SubKey { set; get; }
        public T Obj { set; get; }
    }
}
