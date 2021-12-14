﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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
            var provider = new RegProvider<T>(typeof(T));
            
            data(provider.Setting);
            this.Provider = provider;
            return this;
        }

        //public RegQuery(RegProvider provider, Expression expression)
        //{
        //    this.Provider = provider;
        //    this.Expression = expression;
        //}

        public RegQuery(IQueryProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        //public RegQuery(RegProvider provider, Expression expression, bool isfirst, Expression regsource)
        //{
        //    this.Provider = provider;


        //    this.Expression = expression;

        //}


        public Expression Expression { private set; get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider { private set; get; }
        public IEnumerator<T> GetEnumerator()
        {
            var fail = (this.Provider as RegProvider<T>)?.CheckFail();
            if(fail != null)
            {
                throw fail;
            }
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

    public static class RegQueryEx
    {
        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector) where TResult:class
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
                throw new Exception("Source must be RegistryKey");
            }
            var pps = data.GetType().GetGenericArguments()[1].GetProperties().Where(x => x.CanRead == true&&x.GetCustomAttributes(true).Any(y=>y is RegIgnore||y is RegSubKeyName)==false);
            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            foreach(var pp in pps)
            {
                dicpps[pp] = pp.Name;
                var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
                if (regnames.Length > 0)
                {
                    dicpps[pp] = regnames[0].Name;
                }

            }

            foreach (var oo in source)
            {
                RegistryKey reg = oo as RegistryKey;
                var obj = data(oo);
                
                foreach(var pp in pps)
                {
                    var vv = pp.GetValue(obj, null);

                    if (vv != null)
                    {
                        reg.SetValue(dicpps[pp], vv);
                    }

                }
            }
            
            return source.Count();
        }

        public static int RemoveAll<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            var removeall = typeof(RegQueryEx).GetMethods().FirstOrDefault(x =>
            {
                Type[] types = new Type[] { typeof(IEnumerable<TSource>).GetGenericTypeDefinition(), typeof(Func<TSource, bool>).GetGenericTypeDefinition() };
                bool result = false;
                if (x.Name == "RemoveAll" && x.IsGenericMethod==true)
                {
                    var pps = x.GetParameters().Select(y=>y.ParameterType.GetGenericTypeDefinition());
                    result = pps.Zip(types, (a,b)=> new {a,b }).All(c=>c.a==c.b);
                }
                return result;
            });
            var methdodcall = Expression.Call(removeall.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int RemoveAll<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
            var regs = source as IEnumerable<RegistryKey>;
            if (regs == null)
            {
                throw new Exception("Source must be RegistryKey");
            }
            int count = 0;
            var parent = regs.FirstOrDefault()?.GetParent();
            if (parent != null)
            {
                var zip = source.Zip(regs, (src, reg) => new { src, reg });
                foreach (var oo in zip)
                {
                    if(predicate(oo.src) == true)
                    {
                        var match = regex1.Match(oo.reg.Name);
                        if (match.Success)
                        {
                            parent.DeleteSubKeyTree(match.Groups["path"].Value);
                            count++;
                        }
                    }
                    

                }
                parent.Close();
                parent.Dispose();
            }

            return count;
        }


        public static int RemoveAll<TSource>(this IQueryable<TSource> source)
        {
            var removeall = typeof(RegQueryEx).GetMethods().FirstOrDefault(x =>
            {
                bool result = false;
                if (x.Name == "RemoveAll" && x.IsGenericMethod == true)
                {
                    var pps = x.GetParameters();
                    if (pps.Length == 1)
                    {
                        result = (pps[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<TSource>).GetGenericTypeDefinition());
                    }
                }
                return result;
            });
            var methdodcall = Expression.Call(removeall.MakeGenericMethod(typeof(TSource)), source.Expression);
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int RemoveAll<TSource>(this IEnumerable<TSource> source)
        {
            Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
            var regs = source as IEnumerable<RegistryKey>;
            if (regs == null)
            {
                throw new Exception("Source must be RegistryKey");
            }
            int count = 0;
            var parent = regs.FirstOrDefault()?.GetParent();
            if(parent != null)
            {
                foreach (var oo in regs)
                {
                    var match = regex1.Match(oo.Name);
                    if (match.Success)
                    {
                        parent.DeleteSubKeyTree(match.Groups["path"].Value);
                        count++;
                    }

                }
                parent.Close();
                parent.Dispose();
            }
            
            return count;
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
                        var ssss = pps[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length;
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

        static public MethodInfo SelectMethod_Enumerable(this Type datatype)
        {
            var select_method = typeof(Enumerable).GetMethods().FirstOrDefault(x =>
            {
                bool hr = false;
                if (x.Name == "Select")
                {
                    var pps = x.GetParameters();
                    if (pps.Length == 2)
                    {
                        var ssss = pps[1].ParameterType.GetGenericArguments().Length;
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

        static public Expression ToSelectData(this Type datatype, ParameterExpression pp=null)
        {
            if(pp==null)
            {
                pp = Expression.Parameter(typeof(RegistryKey), "x");
            }
            var todata = datatype.ToData(pp);
            var lambda = Expression.Lambda(todata, pp);
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            return unary;
        }

        public static Func<RegistryKey, TData> ToFunc<TData>(this RegistryKey reg)
        {
            var o_pp = Expression.Parameter(typeof(RegistryKey), "x");
            var o1 = typeof(TData).ToData(o_pp);
            var o_func = Expression.Lambda<Func<RegistryKey, TData>>(o1, o_pp).Compile();
            return o_func;
        }

        public static LambdaExpression ToLambdaData(this Type datatype, ParameterExpression pp = null)
        {
            if (pp == null)
            {
                pp = Expression.Parameter(typeof(RegistryKey), "x");
            }
            var todata = datatype.ToData(pp);
            var lambda = Expression.Lambda(todata, pp);
            return lambda;
        }

        static public Expression ToData(this Type datatype, ParameterExpression param)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
            var pps = datatype.GetProperties().Where(x => x.CanWrite == true&&x.GetCustomAttributes(typeof(RegIgnore), true).Length==0);
            var ccs = datatype.GetConstructors();
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                var regattr = pp.GetCustomAttributes(true).FirstOrDefault();
                Expression name = null;
                if (regattr != null && regattr is RegSubKeyName)
                {
                    if (pp.PropertyType.IsGenericTypeDefinition == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var method = Expression.Property(param, "Name");
                        UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                        var binding = Expression.Bind(pp, unary1);
                        bindings.Add(binding);
                    }
                    else
                    {
                        var method = Expression.Property(param, "Name");
                        var binding = Expression.Bind(pp, method);
                        bindings.Add(binding);
                        
                    }
                }
                else if (regattr != null && regattr is RegPropertyName)
                {
                    var reganme = regattr as RegPropertyName;
                    name = Expression.Constant(reganme.Name, typeof(string));
                }
                else
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                }
                if(name != null)
                {
                    if (pp.PropertyType.IsGenericTypeDefinition == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                        UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                        var binding = Expression.Bind(pp, unary1);
                        bindings.Add(binding);
                    }
                    else
                    {
                        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                        var binding = Expression.Bind(pp, method);
                        bindings.Add(binding);
                    }
                }
                
            }

            var memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);

            return memberinit;
        }

        public static Type[] GetTypes(this IEnumerable<Expression> src, MethodInfo method)
        {
            List<Type> types = new List<Type>();
            var args = method.GetGenericArguments();
            if (args.Length == 0)
            {
                types.AddRange(src.Select(x => x.Type));
            }
            else
            {
                var args1 = method.GetGenericMethodDefinition().GetGenericArguments().Select(x => x.Name);
                var args2 = method.GetGenericMethodDefinition().GetParameters().ToList();


                Dictionary<string, ParameterInfo> infos = new Dictionary<string, ParameterInfo>();
                foreach (var oo in args2)
                {
                    if (oo.ParameterType.IsGenericType == true)
                    {
                        var paramtype = oo.ParameterType.GetGenericTypeDefinition();
                        if (paramtype == typeof(IQueryable<>) || paramtype == typeof(IEnumerable<>))
                        {
                            infos[oo.ParameterType.GetGenericArguments()[0].Name] = oo;
                        }
                        else if (paramtype == typeof(Func<>) || paramtype == typeof(Func<,>))
                        {
                            var func = oo.ParameterType.GetGenericArguments().Last().Name;
                            infos[func] = oo;
                        }
                        else
                        {
                            var func = oo.ParameterType.GetGenericArguments()[0].GetGenericArguments().Last().Name;
                            infos[func] = oo;
                        }
                    }
                    else if (oo.ParameterType.IsGenericParameter == true)
                    {
                        var aaaaa = oo.ParameterType;
                        infos[aaaaa.Name] = oo;
                    }

                    if (infos.Count >= args.Length)
                    {
                        break;
                    }
                }

                foreach (var oo in infos.Select(x => x.Value.Position))
                {
                    if (src.ElementAt(oo).Type.IsGenericType == true)
                    {
                        var paramtype = src.ElementAt(oo).Type.GetGenericTypeDefinition();
                        if (paramtype == typeof(IQueryable<>))
                        {
                            types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0]);
                        }
                        else if (paramtype == typeof(IEnumerable<>))
                        {
                            types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0]);
                        }
                        else if (paramtype == typeof(Func<>) || paramtype == typeof(Func<,>))
                        {
                            types.Add(src.ElementAt(oo).Type.GetGenericArguments().Last());
                        }
                        else
                        {
                            var type = src.ElementAt(oo).Type.GetGenericArguments()[0];
                            if(type.IsGenericType == true)
                            {
                                var uu = src.ElementAt(oo).Type.GetGenericArguments()[0].GetGenericTypeDefinition();
                                if (uu == typeof(Func<>) || uu == typeof(Func<,>) || uu == typeof(Func<,,>))
                                {
                                    types.Add(src.ElementAt(oo).Type.GetGenericArguments()[0].GetGenericArguments().Last());
                                }
                            }
                            else
                            {
                                types.Add(src.ElementAt(oo).Type);
                            }
                        }
                    }
                    else
                    {
                        types.Add(src.ElementAt(oo).Type);
                    }
                }
            }
           
            return types.Take(args.Length).ToArray();
        }

        static public RegistryKey GetParent(this RegistryKey src)
        {
            string basekeyname = "";
            string path = "";
            Regex regex1 = new Regex(@"^(?<base>\w+)[\\](?<path>.+)[\\]", RegexOptions.Compiled);
            Regex regex2 = new Regex(@"^(?<base>\w+)[\\]", RegexOptions.Compiled);
            var match = regex1.Match(src.Name);
            if (match.Success == true)
            {
                basekeyname = match.Groups["base"].Value;
                path = match.Groups["path"].Value;
            }
            else
            {
                basekeyname = src.Name;
            }
            RegistryHive hive;
            switch (basekeyname)
            {
                case "HKEY_CURRENT_USER":
                    {
                        hive = RegistryHive.CurrentUser;
                    }
                    break;
                case "HKEY_CLASSES_ROOT":
                    {
                        hive = RegistryHive.ClassesRoot;
                    }
                    break;
                case "HKEY_LOCAL_MACHINE":
                    {
                        hive = RegistryHive.LocalMachine;
                    }
                    break;
                case "HKEY_CURRENT_CONFIG":
                    {
                        hive = RegistryHive.CurrentConfig;
                    }
                    break;
                case "HKEY_DYN_DATA":
                    {
                        hive = RegistryHive.DynData;
                    }
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    {
                        hive = RegistryHive.PerformanceData;
                    }
                    break;
                case "HKEY_USERS":
                    {
                        hive = RegistryHive.Users;
                    }
                    break;
                default:
                    {
                        throw new Exception("RegistryHive not find");
                    }
                    break;
            }
            var basekey = RegistryKey.OpenBaseKey(hive,src.View);
            var reg = basekey.OpenSubKey(path, true);
            basekey.Close();
            basekey.Dispose();
            return reg;
        }
    }

    
}
