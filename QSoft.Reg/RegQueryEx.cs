﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace QSoft.Registry.Linq
{
    public static class RegQueryEx
    {
        public static int Insert<TSource, TData>(this RegQuery<TSource> source, IEnumerable<TData> datas)
        {
            var updates = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Insert");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TData)), source.Expression, Expression.Constant(datas, typeof(IEnumerable<TData>)));
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int Insert<TSource, TData>(this IEnumerable<TSource> source, IEnumerable<TData> datas)
        {
            var regs = source as IEnumerable<RegistryKey>;
            if (regs == null)
            {
                throw new Exception("Source must be RegistryKey");
            }
            var reg = regs.FirstOrDefault()?.GetParent();
            var pps = typeof(TData).GetProperties().Where(x => x.CanRead == true);
            foreach (var data in datas)
            {
                var child = reg.CreateSubKey($"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                foreach (var pp in pps)
                {
                    var vv = pp.GetValue(data, null);
                    if(vv != null)
                    {
                        child.SetValue(pp.Name, vv);
                    }
                }
                child.Close();
            }
            return datas.Count();
        }

        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) where TResult : class
        {
            var updates = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Update");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector);
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int Update<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> data)
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
        //    var removeall = typeof(RegQueryEx).GetMethods().FirstOrDefault(x =>
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

        //public static int RemoveAll<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
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
            if (parent != null)
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