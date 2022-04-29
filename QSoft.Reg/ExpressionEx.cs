using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QSoft.Registry
{
    public static class ExpressionEx
    {
        public static Expression DefaultExpr(this Type src)
        {
            ConstantExpression default_value = null;
            var typecode = Type.GetTypeCode(src);
            switch(typecode)
            {
                case TypeCode.Boolean:
                    {
                        default_value = Expression.Constant(false);
                    }
                    break;
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Double:
                    {
                        default_value = Expression.Constant(0, src);
                    }
                    break;
                case TypeCode.DateTime:
                    {
                        default_value = Expression.Constant(DateTime.MinValue, src);
                    }
                    break;
                case TypeCode.Object:
                case TypeCode.String:
                    {
                        default_value = Expression.Constant(null, src);
                    }
                    break;
            }

            return default_value;
        }
    }
}
