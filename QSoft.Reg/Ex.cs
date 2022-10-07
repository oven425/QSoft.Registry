using System;
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
        public RegQueryConvert Convert { set; get; }
        public string Name { set; get; }
    }
}
