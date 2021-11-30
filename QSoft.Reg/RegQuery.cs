using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QSoft.Registry.Linq
{
    public class RegQuery<T> : IOrderedQueryable<T>
    {
        public RegQuery()
        {
            this.Expression = Expression.Constant(this);
        }

        
        public RegQuery<T> useSetting(Action<RegSetting> data)
        {
            var provider = new RegProvider(typeof(T));
            
            data(provider.Setting);
            this.Provider = provider;
            return this;
        }

        public RegQuery(RegProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        public RegQuery(RegProvider provider, Expression expression, bool isfirst, Expression regsource)
        {
            this.Provider = provider;

            ////if(isfirst == true)
            //MethodCallExpression method1 = expression as MethodCallExpression;
            ////if(method1.Arguments[0].NodeType == ExpressionType.Constant)
            //{
            //    //List<RegistryKey> regs = new List<RegistryKey>();
            //    //RegistryKey registry = provider.Setting;
            //    //var subkeynames = registry.GetSubKeyNames();

            //    //foreach (var subkeyname in subkeynames)
            //    //{
            //    //    regs.Add(registry.OpenSubKey(subkeyname));
            //    //}
            //    //var tte = regs.AsQueryable();

            //    RegExpressionVisitor reg = new RegExpressionVisitor();
            //    this.Expression = reg.Visit(expression, typeof(T), null, regsource);
            //}
            ////else
            ////{
            ////    this.Expression = expression;
            ////}
            this.Expression = expression;

        }


        public Expression Expression { private set; get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider { private set; get; }
        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class RegSetting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
        public RegistryView View { set; get; }


        public static implicit operator RegistryKey(RegSetting data)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(data.Hive, RegistryView.Registry64);
            if (string.IsNullOrEmpty(data.SubKey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(data.SubKey);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }
    }

    public class Typeee : IEqualityComparer<ParameterInfo>
    {
        public bool Equals(ParameterInfo x, ParameterInfo y)
        {
            bool bb = x.ParameterType.GetGenericTypeDefinition() == y.ParameterType.GetGenericTypeDefinition();
            return bb;
        }

        public int GetHashCode(ParameterInfo obj)
        {
            return obj.ParameterType.GetHashCode();
            //if (object.ReferenceEquals(obj, null))
            //{
            //    return 0;
            //}
            //return obj.ParameterType == null ? 0 : obj.ParameterType.GetHashCode();
        }
    }

    public static class RegQueryEx
    {
        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector)
        {
            var updates = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Update");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector);
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int Update<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> data)
        {
            var regs = source as IEnumerable<RegistryKey>;
            if(regs == null)
            {
                throw new Exception("source must be RegistryKey");
            }
            var pps = data.GetType().GetGenericArguments()[1].GetProperties().Where(x => x.CanRead == true);
            foreach (var oo in source)
            {
                RegistryKey reg = oo as RegistryKey;
                Type ddd = data.GetType();
                var obj = data(oo);
                foreach(var pp in pps)
                {
                    object vv = pp.GetValue(obj);
                    if(vv != null)
                    {
                        reg.SetValue(pp.Name, vv);
                    }
                }
            }
            
            return source.Count();
        }

        static public int Replace(this Type[] datas, Type src, Type dst)
        {
            int count = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == src)
                {
                    datas[i] = dst;
                }
            }

            return count;
        }

        static public MethodInfo SelectMethod(this Type datatype)
        {
            var select_method = typeof(Queryable).GetMethods().FirstOrDefault(x =>
            {
                bool hr = false;
                if (x.Name == "Select")
                {
                    var pps = x.GetParameters();
                    if (pps.Length == 2)
                    {
                        var ssss = pps[1].ParameterType.GenericTypeArguments[0].GenericTypeArguments.Length;
                        if (ssss == 2)
                        {
                            hr = true;
                        }
                    }
                }
                return hr;
            });
            return select_method?.MakeGenericMethod(typeof(RegistryKey), datatype);
        }


        static public Expression ToSelectData(this Type datatype)
        {
            var pp = Expression.Parameter(typeof(RegistryKey), "x");
            var todata = datatype.ToData(pp);
            var lambda = Expression.Lambda(todata, pp);
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            return unary;
        }

        static public Expression ToData(this Type datatype, ParameterExpression param)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
            var pps = datatype.GetProperties().Where(x => x.CanWrite == true);
            var ccs = datatype.GetConstructors();
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                Expression name = null;
                if (pp.PropertyType.IsGenericTypeDefinition == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                    UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                    var binding = Expression.Bind(pp, unary1);
                    bindings.Add(binding);
                }
                else
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                    var binding = Expression.Bind(pp, method);
                    bindings.Add(binding);
                }
            }

            var memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);

            return memberinit;
        }

        public static Type[] GetTypes(this IEnumerable<Expression> src, MethodInfo method)
        {
            List<Type> types = new List<Type>();
            var args = method.GetGenericArguments();
            var args1 = method.GetGenericMethodDefinition().GetGenericArguments().Select(x=>x.Name);
            var args2 = method.GetGenericMethodDefinition().GetParameters().ToList();
            
            var aaa = args2.Distinct(new Typeee());

            Dictionary<string, ParameterInfo> infos = new Dictionary<string, ParameterInfo>();
            foreach(var oo in args2)
            {
                var paramtype = oo.ParameterType.GetGenericTypeDefinition();
                if(paramtype == typeof(IQueryable<>) || paramtype == typeof(IEnumerable<>))
                {
                    infos[oo.ParameterType.GetGenericArguments()[0].Name] = oo;
                }
                else
                {
                    var func = oo.ParameterType.GetGenericArguments()[0].GetGenericArguments().Last().Name;
                    infos[func] = oo;
                }

            }

            foreach(var oo in infos.Select(x => x.Value.Position))
            {
                if (src.ElementAt(oo).Type.IsGenericType == true)
                {
                    if (src.ElementAt(oo).Type.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0]);
                    }
                    else if (src.ElementAt(oo).Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0]);
                    }
                    else
                    {
                        var uu = src.ElementAt(oo).Type.GetGenericArguments()[0].GetGenericTypeDefinition();
                        if (uu == typeof(Func<>) || uu == typeof(Func<,>) || uu == typeof(Func<,,>))
                        {
                            types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0].GetGenericArguments().Last());
                        }
                    }
                }
            }


            //foreach (var oo in src)
            //{
            //    if(oo.Type.IsGenericType == true)
            //    {
            //        if (oo.Type.GetGenericTypeDefinition() == typeof(IQueryable<>))
            //        {
            //            types.Add(oo.Type.GetGenericArguments()[0]);
            //        }
            //        else if(oo.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            //        {
            //            types.Add(oo.Type.GetGenericArguments()[0]);
            //        }
            //        else
            //        {
            //            var uu = oo.Type.GetGenericArguments()[0].GetGenericTypeDefinition();
            //            if (uu == typeof(Func<>)||uu == typeof(Func<,>) || uu == typeof(Func<,,>))
            //            {
            //                types.Add(oo.Type.GetGenericArguments()[0].GetGenericArguments().Last());
            //            }
            //        }
            //    }
                
                
            //}
            
            return types.Take(args.Length).ToArray();
        }

        public static string ToString1(this string src, string str)
        {
            return str;
        }
    }

    
}
