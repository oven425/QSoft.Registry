﻿using Microsoft.Win32;
using QSoft.Registry.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace QSoft.Registry
{
    public static class ExpressionEx
    {
        public static MethodCallExpression WriteLineExpr(this string src)
        {
            return Expression.Constant(src).WriteLineExpr();
        }

        public static MethodCallExpression WriteLineExpr(this Expression src)
        {
            var expr = src.ToStringExpr();
            var method = typeof(System.Diagnostics.Trace).GetMethod("WriteLine", new Type[] { src.Type });
            MethodCallExpression methodcall = null;
            TypeCode typecode = Type.GetTypeCode(src.Type);
            if (typecode == TypeCode.String)
            {
                methodcall = Expression.Call(method, src);
            }
            else if (typecode != TypeCode.Object)
            {
                methodcall = Expression.Call(method, src.ToStringExpr());
            }
            else
            {
                methodcall = Expression.Call(method, src);
            }
            return methodcall;
        }

        public static MethodCallExpression ToStringExpr(this Expression src)
        {
            var method = src.Type.GetMethod("ToString", new Type[] { });
            var methodcall = Expression.Call(src, method);
            return methodcall;
        }

        public static MethodCallExpression DisposeExpr(this Expression src)
        {
            var method = src.Type.GetMethod("Dispose");
            var methodcall = Expression.Call(src, method);
            return methodcall;
        }

        public static MemberExpression PropertyExpr(this Expression src, string name)
        {
            return Expression.Property(src, name);
        }

        public static ParameterExpression ParameterExpr(this Type src, string name)
        {
            if(src.GetInterfaces().Any(x => x == typeof(IEnumerable)))
            {
                var pp = Expression.Parameter(typeof(IEnumerable<RegistryKey>), name);

                return pp;
            }
            else
            {
                var pp = Expression.Parameter(typeof(RegistryKey), name);

                return pp;
            }
        }

        public static MethodCallExpression GetValueExpr(this Expression src, string name, Type type)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
            var hr = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(type), src, Expression.Constant(name));

            return hr;
        }

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

        public static MethodCallExpression OpenSubKeyExr(this IEnumerable<string> src, Expression param)
        {
            var subkeyname = src.Aggregate((x, y) => $"{x}\\{y}");
            var opensubkey = typeof(RegistryKey).GetMethod("OpenSubKey", new[] { typeof(string) });
            var member = Expression.Call(param, opensubkey, Expression.Constant(subkeyname));
            return member;
        }

        public static Expression BuildDisposeExpr(this Type src, Func<BinaryExpression> func) 
        {
            return null;
        }

        public static MethodCallExpression GetAllSubKeysExprs(this Expression src) 
        {
            var opensubkeys_expr = Expression.Call(typeof(RegQueryEx).GetMethod("GetAllSubKeys"), src);

            var opensubkey = typeof(RegistryKey).GetMethod("GetAllSubKeys", new[] { typeof(string) });
            return null; 
        }    
    }
}
