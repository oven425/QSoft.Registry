using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    static public class ExpressionEx
    {
        public static Expression Clone(this Expression src)
        {
            if(src is ConstantExpression)
            {
                return ((ConstantExpression)src).Clone();
            }
            return null;
        }

        public static MethodCallExpression Clone(this MethodCallExpression src)
        {
            MethodCallExpression mm = null;
            return mm;
        }

        public static ConstantExpression Clone(this ConstantExpression src)
        {
            if(src.Value != null  && src.Type != null)
            {
                return Expression.Constant(src.Value, src.Type);
            }
            return Expression.Constant(src.Value);
        }

        public static ParameterExpression Clone(this ParameterExpression src)
        {
            ParameterExpression mm = null;
            mm = Expression.Parameter(src.Type, src.Name);
            return mm;
        }

        public static MemberExpression Clone(this MemberExpression src)
        {
            MemberExpression mm = null;
            mm = Expression.MakeMemberAccess(src.Expression, src.Member);
            return mm;
        }


    }
}
