using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class UUU
    {
        public static MethodCallExpression DisposeExpr(this Expression src)
        {
            var method = src.Type.GetMethod("Dispose");
            var methodcall = Expression.Call(src, method);
            return methodcall;
        }
        static public Expression ToData(this Type dst, Expression param)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
            var getvaluenames = typeof(RegistryKey).GetMethod("GetValueNames");

            var pps = dst.GetProperties().Where(x => x.CanWrite == true)
                .Select(x => new
                {
                    x,
                    attr = x.GetCustomAttributes(true).FirstOrDefault(y => y is RegSubKeyName || y is RegIgnore || y is RegPropertyName)
                }).Where(x => !(x.attr is RegIgnore));

            var ccs = dst.GetConstructors();
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.x.PropertyType);
                var property = pp.x.PropertyType;
                if (pp.x.PropertyType.IsGenericType == true && pp.x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    property = pp.x.PropertyType.GetGenericArguments()[0];
                    typecode = Type.GetTypeCode(property);
                }
                Expression name = null;
                if (pp.attr != null && pp.attr is RegSubKeyName)
                {
                    RegSubKeyName subkeyname = pp.attr as RegSubKeyName;
                    var expr = subkeyname.ToExpression(pp.x.PropertyType, param);
                    var binding = Expression.Bind(pp.x, expr);
                    bindings.Add(binding);
                }
                else if (pp.attr != null && pp.attr is RegPropertyName)
                {
                    if (typecode == TypeCode.Object)
                    {
                        var subkeyname = (pp.attr as RegPropertyName)?.Name;
                        var opensubkey = typeof(RegistryKey).GetMethod("OpenSubKey", new[] { typeof(string) });
                        var getsubkeyexpr = Expression.Call(param, opensubkey, Expression.Constant(subkeyname));
                        var reg_p = Expression.Parameter(typeof(RegistryKey), "reg");
                        var new_p = Expression.Parameter(pp.x.PropertyType, "dst");
                        var objexpr = pp.x.PropertyType.ToData(reg_p);
                        var block = Expression.Block(new[] { reg_p, new_p },
                            Expression.Assign(reg_p, getsubkeyexpr),
                            Expression.Condition(Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                                Expression.Block(
                                    "456".WriteLineExpr(),
                                    Expression.Assign(new_p, objexpr),
                                    new_p
                                    ),
                                Expression.Block(
                                    "456".WriteLineExpr(),
                                    Expression.Constant(null, pp.x.PropertyType)
                                    )
                                )
                            );
                        var binding = Expression.Bind(pp.x, block);
                        bindings.Add(binding);
                    }
                    else
                    {
                        var reganme = pp.attr as RegPropertyName;
                        name = Expression.Constant(reganme.Name, typeof(string));
                    }
                }
                else
                {
                    if (typecode == TypeCode.Object)
                    {
                        var subkeyname = pp.x.Name;
                        var opensubkey = typeof(RegistryKey).GetMethod("OpenSubKey", new[] { typeof(string) });
                        var getsubkeyexpr = Expression.Call(param, opensubkey, Expression.Constant(subkeyname));
                        var reg_p = Expression.Parameter(typeof(RegistryKey), "reg");
                        var new_p = Expression.Parameter(pp.x.PropertyType, "dst");
                        var objexpr = pp.x.PropertyType.ToData(reg_p);
                        var block = Expression.Block(new[] { reg_p, new_p },
                            Expression.Assign(reg_p, getsubkeyexpr),
                            Expression.Condition(Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                                Expression.Block(
                                    Expression.Assign(new_p, objexpr),
                                    new_p
                                    ),
                                Expression.Constant(null, pp.x.PropertyType)
                                )
                            );
                        var binding = Expression.Bind(pp.x, block);
                        bindings.Add(binding);
                    }
                    else
                    {
                        name = Expression.Constant(pp.x.Name, typeof(string));
                    }
                }
                if (name != null)
                {
                    if (pp.x.PropertyType.IsGenericTypeDefinition == true && pp.x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.x.PropertyType), param, name);
                        UnaryExpression unary1 = Expression.Convert(method, pp.x.PropertyType);
                        var binding = Expression.Bind(pp.x, unary1);
                        bindings.Add(binding);
                    }
                    else
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.x.PropertyType), param, name);
                        var binding = Expression.Bind(pp.x, method);
                        bindings.Add(binding);
                    }
                }

            }
            var memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);

            return memberinit;
        }

    }

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
            MethodCallExpression methodcall = null;
            TypeCode typecode = Type.GetTypeCode(src.Type);
            if(typecode == TypeCode.String)
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

        public static MethodCallExpression WriteLineExpr(this string src)
        {
            return Expression.Constant(src).WriteLineExpr();
        }

        public static Expression Foreach(this Expression src, Expression block)
        {
            LabelTarget label = Expression.Label();
            ParameterExpression enumerator = Expression.Variable(typeof(IEnumerator<PropertyInfo>), "enumerator");
            var method = Expression.Call(src, src.Type.GetMethod("GetEnumerator"));
            Expression.Assign(enumerator, Expression.Call(src, src.Type.GetMethod("GetEnumerator")));
            Expression.Loop(
                Expression.Block(
                        Expression.IfThenElse(Expression.MakeBinary(ExpressionType.Equal, Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")), Expression.Constant(true)),
                            "123".WriteLineExpr(),
                            Expression.Break(label))
                    ),
                label
                );
            //if (src.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            //{
            //    var getenumerator = src.Type.GetMethod("GetEnumerator");
            //    LabelTarget label = Expression.Label("foreach_break");
            //    var tt = src.Type.GetGenericArguments().First();
            //    typeof(IEnumerator<>).MakeGenericType(tt);
            //}
            return null;
        }

        public static MemberExpression PropertyExpr(this Expression src, string name)
        {
            return Expression.Property(src, name);
        }

        public static BinaryExpression MakeBinary(this ExpressionType src, params Expression[] exprs)
        {
            if(exprs.Length<2)
            {
                return null;
            }
            BinaryExpression binary = Expression.MakeBinary(src, exprs[0], exprs[1]);
            for (int i=2; i<exprs.Length; i++)
            {
                binary = Expression.MakeBinary(src, binary, exprs[i]);
            }

            return binary;
        }

        public static UnaryExpression Convert(this Expression src, Type type)
        {
            return Expression.Convert(src, type);
        }
    }
}
