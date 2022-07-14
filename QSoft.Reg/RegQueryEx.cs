using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QSoft.Registry.Linq
{
    public static class RegQueryEx
    {
        public static int Insert<TSource>(this RegQuery<TSource> source, IEnumerable<TSource> datas) where TSource : class
        {
            var updates = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "Insert");
            var reg = source.ToRegistryKey();
            var provide = source.Provider as RegProvider<TSource>;
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource)), Expression.Constant(reg, typeof(RegistryKey)), Expression.Constant(datas, typeof(IEnumerable<TSource>)), Expression.Constant(provide.DefaultValue, typeof(Action<TSource>)), Expression.Constant(provide.Converts));
            
            int hr = source.Provider.Execute<int>(methdodcall);
            reg.Close();
            return hr;
        }

        static void Insert(this RegistryKey source, object data, object defaulvalue, bool isinsert, IEnumerable<RegQueryConvert> converts)
        {
            if (data == null) { return; }

            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            PropertyInfo subkey = null;

            var p = data.GetType().PropertyName();
            dicpps = p.Item2;
            subkey = p.Item1;
            RegistryKey child = source;
            if(isinsert == true)
            {
                if (subkey == null)
                {
                    child = source.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}_regquery", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                else
                {
                    var vv = subkey.GetValue(data, null);
                    child = source.CreateSubKey($"{vv.ToString()}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
            }
            isinsert = false;

            foreach (var pp in dicpps)
            {
                var typecode = Type.GetTypeCode(pp.Key.PropertyType);
                if (pp.Key.PropertyType.IsGenericType == true && pp.Key.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    typecode = Type.GetTypeCode(pp.Key.PropertyType.GetGenericArguments()[0]);
                }
                if (pp.Key.PropertyType == typeof(Version))
                {
                    System.Diagnostics.Debug.WriteLine("");
                    var con = converts.FirstOrDefault(x => x.CanConvert(pp.Key.PropertyType));
                    var methods = con.GetType().GetMethod("ConvertTo");
                    var vv = pp.Key.GetValue(data, null);
                    var dst = methods.Invoke(con, new[] { vv });
                    child.SetValue(dicpps[pp.Key], dst);

                }
                //if(typecode == TypeCode.Object && pp.Key.PropertyType!= typeof(Version))
                else if (typecode == TypeCode.Object)
                {
                    
                    var obj = pp.Key.GetValue(data, null);
                    if (obj != null)
                    {
                        var cc = child.CreateSubKey(pp.Value, RegistryKeyPermissionCheck.ReadWriteSubTree);
                        cc.Insert(pp.Key.GetValue(data, null), null, isinsert, converts);
                    }
                }
                else
                {
                    var vv = pp.Key.GetValue(data, null);
                    if (vv != null)
                    {
                        child.SetValue(dicpps[pp.Key], vv);
                    }
                }
                //switch (typecode)
                //{
                //    case TypeCode.Object:
                //        {
                //            if(pp.Key.PropertyType == typeof(Version))
                //            {

                //            }
                //            else
                //            {
                                
                //            }
                //        }
                //        break;
                //    default:
                //        {
                            
                //        }
                //        break;
                //}
            }
            child.Close();
        }

        static int Insert<TData>(this RegistryKey source, IEnumerable<TData> datas, Action<TData> defaultaction, IEnumerable<RegQueryConvert> converts)
        {
            TData hasdefault = default(TData);
            if(defaultaction != null)
            {
                hasdefault = Activator.CreateInstance<TData>();
                defaultaction(hasdefault);
            }
            
            int count = 0;
            foreach (var data in datas)
            {
                source.Insert(data, null, true, converts);
                count = count + 1;
            }
            return count;
        }

        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) where TResult : class
        {
            var t1 = typeof(TSource);
            var t2 = typeof(TResult);
            var updates = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic|BindingFlags.Static).Where(x => x.Name == "Update");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector);
            return source.Provider.Execute<int>(methdodcall);
        }

        static int Update<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> data_func)
        {
            var regs = source as IEnumerable<RegistryKey>;
            if (regs == null)
            {
                throw new Exception("Source must be RegistryKey");
            }

            //var pps = data_func.GetType().GetGenericArguments()[1].GetProperties().Where(x => x.CanRead == true && x.GetCustomAttributes(true).Any(y => y is RegIgnore || y is RegSubKeyName) == false);
            //Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            //foreach (var pp in pps)
            //{
            //    dicpps[pp] = pp.Name;
            //    var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
            //    if (regnames.Length > 0)
            //    {
            //        dicpps[pp] = regnames[0].Name;
            //    }
            //}

            foreach (var oo in source)
            {
                RegistryKey reg = oo as RegistryKey;
                var obj = data_func(oo);
                reg.Insert(obj, null, false, null);
                //foreach (var pp in pps)
                //{
                //    var vv = pp.GetValue(obj, null);

                //    if (vv != null)
                //    {
                //        reg.SetValue(dicpps[pp], vv);
                //    }

                //}
            }

            return source.Count();
        }

        //static void Update(this RegistryKey source, object data)
        //{
        //    if (data == null) { return; }
        //    Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
        //    PropertyInfo subkey = null;

        //    var p = data.GetType().PropertyName();
        //    dicpps = p.Item2;
        //    subkey = p.Item1;
        //    RegistryKey child = null;
        //    if (subkey == null)
        //    {
        //        child = source.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}_regquery", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //    }
        //    else
        //    {
        //        var vv = subkey.GetValue(data, null);
        //        child = source.CreateSubKey($"{vv.ToString()}", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //    }
        //    foreach (var pp in dicpps.Select(x => x.Key))
        //    {
        //        var typecode = Type.GetTypeCode(pp.PropertyType);
        //        if (pp.PropertyType.IsGenericType == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        //        {
        //            typecode = Type.GetTypeCode(pp.PropertyType.GetGenericArguments()[0]);
        //        }
        //        switch (typecode)
        //        {
        //            case TypeCode.Object:
        //                {
        //                    child.Insert(pp.GetValue(data, null), null);
        //                }
        //                break;
        //            default:
        //                {
        //                    var vv = pp.GetValue(data, null);
        //                    if (vv != null)
        //                    {
        //                        child.SetValue(dicpps[pp], vv);
        //                    }
        //                }
        //                break;
        //        }

        //    }
        //    child.Close();
        //}


        //public static int InsertTo<TFirst, TSecond, TResult>(this IQueryable<TFirst> src, RegQuery<TSecond> dst, Expression<Func<TFirst, TResult>> selector)
        //{
        //    var inserorupdate = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "InsertAnotherReg");
        //    var reg = dst.ToRegistryKey();
        //    var methdodcall = Expression.Call(inserorupdate.First().MakeGenericMethod(typeof(TFirst), typeof(TResult)), src.Expression, Expression.Constant(reg, typeof(RegistryKey)), selector);
        //    int hr = src.Provider.Execute<int>(methdodcall);
        //    reg.Close();
        //    return hr;
        //}

        //static int InsertAnotherReg<TFirst, TResult>(this IEnumerable<TFirst> source, RegistryKey dst, Func<TFirst, TResult> func)
        //{
        //    int count = 0;
        //    var p = typeof(TResult).PropertyName();
        //    var subkey = p.Item1;
        //    var dicpps = p.Item2;

        //    foreach (var oo in source)
        //    {
        //        var obj = func(oo);
        //        RegistryKey child = null;
        //        if (subkey == null)
        //        {
        //            child = dst.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}_regquery", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        }
        //        else
        //        {
        //            var vv = subkey.GetValue(obj, null);
        //            child = dst.CreateSubKey($"{vv.ToString()}", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        }
        //        foreach (var pp in dicpps.Select(x => x.Key))
        //        {
        //            var vv = pp.GetValue(obj, null);
        //            if (vv != null)
        //            {
        //                child.SetValue(dicpps[pp], vv);
        //            }
        //        }
        //        child.Close();
        //        count = count + 1;

        //    }
        //    return count;
        //}

        public static int RemoveAll<TSource>(this IQueryable<TSource> source)
        {
            var removeall = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(x =>
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

        static int RemoveAll<TSource>(this IEnumerable<TSource> source)
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
                var rds = regs.ToList();
                foreach (var oo in rds)
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

        public static IQueryable<TSource> Hierarchy<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            var method = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "Hierarchy");
            var methdodcall = Expression.Call(method.ElementAt(0).MakeGenericMethod(typeof(TSource)), source.Expression, predicate);
            var hr = source.Provider.Execute<IQueryable<TSource>>(methdodcall);
            return hr;
        }


        static IEnumerable<TSource> Hierarchy<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if(typeof(TSource) != typeof(RegistryKey))
            {
                throw new Exception("Source must be RegistryKey");
            }
            return null;
        }



        //public static IQueryable<RegistryKey> Intersect_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2)
        //{
        //    if (typeof(TSource2) == typeof(RegistryKey))
        //    {
        //        var src_regs_2 = source2 as IQueryable<RegistryKey>;
        //        var groupby = source1.GroupBy(x => x.Name);
        //        var except = source1.Select(x => x.Name).Intersect(src_regs_2.Select(x => x.Name));
        //        var ss = source1.Join(except, x => x.Name, y => y, (x, y) => x);
        //        return ss;
        //    }
        //    else
        //    {
        //        var func = source1.First().ToFunc<TSource2>();
        //        var aa = source1.ToDictionary(x => func(x), x => x);
        //        var compare = aa.Keys.Intersect(source2, new EqualityComparerForce<TSource2>());
        //        var dst = compare.Select(x => aa[x]).AsQueryable();
        //        return dst;

        //    }
        //}

        //public static IQueryable<RegistryKey> Intersect_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2, IEqualityComparer<TSource2> comparer)
        //{
        //    if (typeof(TSource2) == typeof(RegistryKey))
        //    {
        //        var src_regs_2 = source2 as IQueryable<RegistryKey>;
        //        var except = source1.Select(x => x.Name).Intersect(src_regs_2.Select(x => x.Name));
        //        var ss = source1.Join(except, x => x.Name, y => y, (x, y) => x);
        //        return ss;
        //    }
        //    else
        //    {
        //        var func = source1.First().ToFunc<TSource2>();
        //        var aa = source1.ToDictionary(x => func(x), x => x);
        //        var compare = aa.Keys.Intersect(source2, comparer);
        //        var dst = compare.Select(x => aa[x]).AsQueryable();
        //        return dst;
        //    }

        //}

        //public static IQueryable<RegistryKey> Except_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2)
        //{
        //    if (typeof(TSource2) == typeof(RegistryKey))
        //    {
        //        var src_regs_2 = source2 as IQueryable<RegistryKey>;
        //        var groupby = source1.GroupBy(x => x.Name);
        //        var except = source1.Select(x => x.Name).Except(src_regs_2.Select(x => x.Name));
        //        var ss = source1.Join(except, x => x.Name, y => y, (x, y) => x);
        //        return ss;
        //    }
        //    else
        //    {
        //        var func = source1.First().ToFunc<TSource2>();
        //        var aa = source1.ToDictionary(x => func(x), x => x);
        //        var compare = aa.Keys.Except(source2, new EqualityComparerForce<TSource2>());
        //        var dst = compare.Select(x => aa[x]).AsQueryable();
        //        return dst;

        //    }
        //}

        //public static IQueryable<RegistryKey> Except_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2, IEqualityComparer<TSource2> comparer)
        //{
        //    if (typeof(TSource2) == typeof(RegistryKey))
        //    {
        //        var src_regs_2 = source2 as IQueryable<RegistryKey>;
        //        var except = source1.Select(x => x.Name).Except(src_regs_2.Select(x => x.Name));
        //        var ss = source1.Join(except, x => x.Name, y => y, (x, y) => x);
        //        return ss;
        //    }
        //    else
        //    {
        //        var func = source1.First().ToFunc<TSource2>();
        //        var aa = source1.ToDictionary(x => func(x), x => x);
        //        var compare = aa.Keys.Except(source2, comparer);
        //        var dst = compare.Select(x => aa[x]).AsQueryable();
        //        return dst;
        //    }

        //}

        //public static int RemoveAll<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        //{
        //    var removeall = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic|BindingFlags.Static).FirstOrDefault(x =>
        //    {
        //        Type[] types = new Type[] { typeof(IEnumerable<TSource>).GetGenericTypeDefinition(), typeof(Func<TSource, bool>).GetGenericTypeDefinition() };
        //        bool result = false;
        //        if (x.Name == "RemoveAll" && x.IsGenericMethod == true)
        //        {
        //            var pps = x.GetParameters().Select(y => y.ParameterType.GetGenericTypeDefinition());
        //            result = pps.Zip(types, (a, b) => new { a, b }).All(c => c.a == c.b);
        //        }
        //        return result;
        //    });
        //    var methdodcall = Expression.Call(removeall.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);
        //    return source.Provider.Execute<int>(methdodcall);
        //}

        //static int RemoveAll<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        //{
        //    Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
        //    var regs = source as IEnumerable<RegistryKey>;
        //    if (regs == null)
        //    {
        //        throw new Exception("Source must be RegistryKey");
        //    }
        //    int count = 0;
        //    var parent = regs.FirstOrDefault()?.GetParent();
        //    if (parent != null)
        //    {
        //        var zip = source.Zip(regs, (src, reg) => new { src, reg });
        //        foreach (var oo in zip)
        //        {
        //            if (predicate(oo.src) == true)
        //            {
        //                var match = regex1.Match(oo.reg.Name);
        //                if (match.Success)
        //                {
        //                    parent.DeleteSubKeyTree(match.Groups["path"].Value);
        //                    count++;
        //                }
        //            }


        //        }
        //        parent.Close();
        //        parent.Dispose();
        //    }

        //    return count;
        //}


    }

}
