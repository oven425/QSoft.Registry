﻿using Microsoft.Win32;
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
        public static int Insert<TSource, TData>(this RegQuery<TSource> source, IEnumerable<TData> datas) where TData : class
        {
            var updates = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic|BindingFlags.Static).Where(x => x.Name == "Insert");
            var reg = source.ToRegistryKey();
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TData)), Expression.Constant(reg, typeof(RegistryKey)), Expression.Constant(datas, typeof(IEnumerable<TData>)));
            int hr = source.Provider.Execute<int>(methdodcall);
            reg.Close();
            return hr;
        }

        static int Insert<TData>(this RegistryKey source, IEnumerable<TData> datas)
        {
            int count = 0;
            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            //var pps = typeof(TData).GetProperties().Where(x => x.CanRead == true && x.GetCustomAttributes(true).Any(y => y is RegIgnore || y is RegSubKeyName) == false);

            var group = typeof(TData).GetProperties().Where(x => x.CanRead == true)
                .Select(x => new { x, attr = x.GetCustomAttributes(true).Where(y => y is RegIgnore || y is RegSubKeyName || y is RegPropertyName).FirstOrDefault() })
                .GroupBy(x => x.attr);

            PropertyInfo subkey = null;
            foreach (var items in group)
            {
                if (items.Key is RegPropertyName)
                {
                    foreach (var oo in items)
                    {
                        dicpps[oo.x] = (oo.attr as RegPropertyName)?.Name ?? oo.x.Name;
                    }
                }
                else if (items.Key == null)
                {
                    foreach (var oo in items)
                    {
                        dicpps[oo.x] = oo.x.Name;
                    }
                }
                else if (items.Key is RegSubKeyName)
                {
                    foreach (var oo in items)
                    {
                        subkey = oo.x;
                    }
                }
            }



            //foreach (var pp in pps)
            //{
            //    dicpps[pp] = pp.Name;
            //    var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
            //    if (regnames.Length > 0)
            //    {
            //        dicpps[pp] = regnames[0].Name;
            //    }
            //}
            //int index = 0;
            //var names = source.GetSubKeyNames();
            //if(names.Length > 0)
            //{
            //    index = int.Parse(names.Last());
            //}
            foreach (var data in datas)
            {
                //var child = source.CreateSubKey($"{index++:0000000000}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                //var child = source.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                RegistryKey child = null;
                if (subkey == null)
                {
                    child = source.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                else
                {
                    var vv = subkey.GetValue(data, null);
                    child = source.CreateSubKey($"{vv.ToString()}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                foreach (var pp in dicpps.Select(x=>x.Key))
                {
                    var vv = pp.GetValue(data, null);
                    if(vv != null)
                    {
                        child.SetValue(dicpps[pp], vv);
                    }
                }
                child.Close();
                count = count + 1;
            }
            return count;
        }

        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) where TResult : class
        {
            var updates = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic|BindingFlags.Static).Where(x => x.Name == "Update");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector);
            return source.Provider.Execute<int>(methdodcall);
        }

        static int Update<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> data)
        {
            var regs = source as IEnumerable<RegistryKey>;
            if (regs == null)
            {
                throw new Exception("Source must be RegistryKey");
            }
            var pps = data.GetType().GetGenericArguments()[1].GetProperties().Where(x => x.CanRead == true && x.GetCustomAttributes(true).Any(y => y is RegIgnore || y is RegSubKeyName) == false);
            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            foreach (var pp in pps)
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

                foreach (var pp in pps)
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


        public static int InsertOrUpdate<TFirst, TSecond, TResult>(this IQueryable<TFirst> source, RegQuery<TSecond> reg, Expression<Func<TFirst, TResult>> selector)
        {
            var inserorupdate = typeof(RegQueryEx).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.Name == "InsertOrUpdate");
            var methdodcall = Expression.Call(inserorupdate.First().MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TResult)), source.Expression, Expression.Constant(reg, reg.GetType()), selector);
            return source.Provider.Execute<int>(methdodcall);
            return 1;
        }

        static int InsertOrUpdate<TFirst, TSecond, TResult>(this IEnumerable<TFirst> source, IEnumerable<TSecond> reg, Func<TFirst, TResult> data)
        {
            return 0;
        }




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

        

        
    }

}
