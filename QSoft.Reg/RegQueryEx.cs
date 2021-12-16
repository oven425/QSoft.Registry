﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace QSoft.Registry.Linq
{
    public static class RegQueryEx
    {
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

        public static int RemoveAll<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            var removeall = typeof(RegQueryEx).GetMethods().FirstOrDefault(x =>
            {
                Type[] types = new Type[] { typeof(IEnumerable<TSource>).GetGenericTypeDefinition(), typeof(Func<TSource, bool>).GetGenericTypeDefinition() };
                bool result = false;
                if (x.Name == "RemoveAll" && x.IsGenericMethod == true)
                {
                    var pps = x.GetParameters().Select(y => y.ParameterType.GetGenericTypeDefinition());
                    result = pps.Zip(types, (a, b) => new { a, b }).All(c => c.a == c.b);
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
                    if (predicate(oo.src) == true)
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

        static string ToXml(object obj)
        {
            string str = "";
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            
            using (MemoryStream mm = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(mm, new XmlWriterSettings { Indent = false, Encoding = Encoding.UTF8 }))
                {
                    xml.Serialize(xmlWriter, obj);
                }
               
                str = Encoding.UTF8.GetString(mm.ToArray());
            }
            return str;
        }

        public static IQueryable<RegistryKey> Except_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2)
        {
            if (typeof(TSource2) == typeof(RegistryKey))
            {
                var src_regs_2 = source2 as IQueryable<RegistryKey>;
                var except = source1.Select(x => x.Name).Except(src_regs_2.Select(x => x.Name));
                var ss = source1.Join(except, x => x.Name, y => y, (x, y) => x);
                return ss;
            }
            else
            {
                //var func = source1.First().ToFunc<TSource2>();
                //var xml1 = source1.ToDictionary(x => ToXml(func(x)));
                //var xml2 = source2.Select(x => ToXml(x));
                //var compare = xml1.Select(x=>x.Key).Except(xml2);
                //var dst = compare.Select(x => xml1[x]).AsQueryable();

                //return dst;

            }
            return null;
        }

        public static IQueryable<RegistryKey> Except_RegistryKey<TSource2>(this IQueryable<RegistryKey> source1, IEnumerable<TSource2> source2, IEqualityComparer<TSource2> comparer)
        {
            var src_regs_1 = source1 as IQueryable<RegistryKey>;
            if (src_regs_1 == null)
            {
                throw new Exception("Source must be RegistryKey");
            }
            
            if (typeof(TSource2) == typeof(RegistryKey))
            {
                var src_regs_2 = source2 as IQueryable<RegistryKey>;
                var except = src_regs_1.Select(x => x.Name).Except(src_regs_2.Select(x => x.Name));
                var ss = src_regs_1.Join(except, x => x.Name, y => y, (x, y) => x);
                return ss;
            }
            else
            {
                var func = src_regs_1.First().ToFunc<TSource2>();
                var aa = src_regs_1.ToDictionary(x => func(x), x => x);
                var compare = aa.Keys.Except(source2, comparer);
                var dst = compare.Select(x => aa[x]).AsQueryable();
                return dst;
            }

        }

        //public static IQueryable<TSource> Except_RegistryKey<TSource>(this IQueryable<TSource> source1, IQueryable<TSource> source2)
        //{
        //    if (typeof(TSource) != typeof(RegistryKey))
        //    {
        //        throw new Exception("Source must be RegistryKey");
        //    }
        //    var src_regs_1 = source1 as IQueryable<RegistryKey>;
        //    var src_regs_2 = source2 as IQueryable<RegistryKey>;


        //    var src_regs_3 = src_regs_1 as IQueryable<TSource>;

        //    var except = src_regs_1.Select(x => x.Name).Except(src_regs_2.Select(x => x.Name));
        //    var dic = src_regs_1.ToDictionary(x => x.Name);
        //    return except.Select(x => dic[x]) as IQueryable<TSource>;
        //}

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

        
    }

}
