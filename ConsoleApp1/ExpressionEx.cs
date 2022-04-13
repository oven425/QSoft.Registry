﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class ExpressionEx
    {
        
        public static MethodCallExpression Where(Expression src, LambdaExpression lambda)
        {
            var ggs = lambda.Type.GetGenericArguments();
            var where_method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where" && x.GetParameters()[1].ParameterType.GetGenericArguments().Length>= ggs.Length);
            var tty = src.Type;
            if (src.Type.IsArray == true)
            {
                tty = src.Type.GetElementType();
            }
            else if(src.Type.IsGenericType == true&& src.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                tty = src.Type.GetGenericArguments()[0];
            }
            where_method = where_method.MakeGenericMethod(tty);
            var methodcall = Expression.Call(where_method, src, lambda);
            return methodcall;
        }

        public static Type GetType1(this Expression src)
        {
            var tty = src.Type;
            if (src.Type.IsArray == true)
            {
                tty = src.Type.GetElementType();
            }
            else if (src.Type.IsGenericType == true)
            {
                var typedef = src.Type.GetGenericTypeDefinition();
                if(typedef == typeof(IEnumerable<>))
                {
                    tty = src.Type.GetGenericArguments()[0];
                }
                else if(typedef == typeof(Func<>)|| typedef == typeof(Func<,>)|| typedef == typeof(Func<,,>)|| typedef == typeof(Func<,,,>))
                {
                    tty = src.Type.GetGenericArguments().Last();
                }
            }


            return tty;
        }

        public static MethodCallExpression Select(Expression src, LambdaExpression lambda)
        {
            var ggs = lambda.Type.GetGenericArguments();
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Select" && x.GetParameters()[1].ParameterType.GetGenericArguments().Length >= ggs.Length);
            method = method.MakeGenericMethod(src.GetType1(), lambda.GetType1());
            var methodcall = Expression.Call(method, src, lambda);
            return methodcall;
        }


        public static MethodCallExpression FirstOrDefault(Expression src, LambdaExpression lambda)
        {
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "FirstOrDefault" && x.GetParameters().Length>1);
            method = method.MakeGenericMethod(src.GetType1());
            var methodcall = Expression.Call(method, src, lambda);
            return methodcall;
        }

        public static MethodCallExpression ToStringExpr(this Expression src)
        {
            var method = src.Type.GetMethod("ToString", new Type[] { });
            var methodcall = Expression.Call(src, method);
            return methodcall;
        }

        public static MethodCallExpression WriteLineExpr(this Expression src)
        {
            var expr = src.ToStringExpr();
            var method = typeof(System.Diagnostics.Trace).GetMethod("WriteLine", new Type[] { src.Type});
            var methodcall = Expression.Call(method, src);
            return methodcall;
        }

        public static Expression Foreach(this Expression src)
        {
            if(src.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var getenumerator = src.Type.GetMethod("GetEnumerator");
                
                var tt = src.Type.GetGenericArguments().First();
                typeof(IEnumerator<>).MakeGenericType(tt);
            }
            return null;
        }

    }
}
