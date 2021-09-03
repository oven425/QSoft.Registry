using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QSoft.Registry
{
    class RegExpressionVisitor: ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
    }
}
