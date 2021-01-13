using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace QSoft.Registry.Linq
{
    public static class RegistryKeyEx
    {
        public static IEnumerable<RegistryKey> Where(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach(var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                if(func.Invoke(reg) == true)
                {
                    yield return reg;
                }
                else
                {
                    reg.Dispose();
                    reg = null;
                }
            }
        }

        public static bool Any(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            bool bb = false;
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                if (func.Invoke(reg) == true)
                {
                    bb = true;
                }

                reg.Dispose();
                reg = null;
                if(bb==true)
                {
                    break;
                }
            }
            return bb;
        }

        public static int Count(this RegistryKey src)
        {
            return src.GetSubKeyNames().Length;
        }

        public static int Count(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            int count = 0;
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                if (func.Invoke(reg) == true)
                {
                    count++;
                }
                else
                {
                    reg.Dispose();
                    reg = null;
                }
            }
            return count;
        }

        public static IEnumerable<object> Select(this RegistryKey src, Func<RegistryKey, object> select)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                object obj = select.Invoke(reg);
                yield return obj;
            }
        }

        public static RegistryKey FirstOrDefault(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            RegistryKey hr = null;
            foreach(var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                if (func.Invoke(reg) == true)
                {
                    hr = reg;
                    break;
                }
                else
                {
                    reg.Dispose();
                    reg = null;
                }
            }
            return hr;
        }

        public static RegistryKey FirstOrDefault(this RegistryKey src)
        {
            RegistryKey hr = null;
            if(src.GetSubKeyNames().Length >0)
            {
                hr = src.OpenSubKey(src.GetSubKeyNames()[0]);
            }
            return hr;
        }

        public static RegistryKey LastOrDefault(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            RegistryKey hr = null;
            string[] subkeynames = src.GetSubKeyNames();
            for(int i= subkeynames.Length-1; i>=0; i--)
            {
                RegistryKey reg = src.OpenSubKey(subkeynames[i]);
                if (func.Invoke(reg) == true)
                {
                    hr = reg;
                    break;
                }
                else
                {
                    reg.Dispose();
                    reg = null;
                }
            }
            return hr;
        }

        public static RegistryKey LastOrDefault(this RegistryKey src)
        {
            RegistryKey hr = null;
            if (src.GetSubKeyNames().Length > 0)
            {
                hr = src.OpenSubKey(src.GetSubKeyNames()[src.GetSubKeyNames().Length-1]);
            }
            return hr;
        }

        public static Dictionary<TKey, RegistryKey> ToDictionary<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func)
        {
            Dictionary<TKey, RegistryKey> dic = new Dictionary<TKey, RegistryKey>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TKey tt = func.Invoke(reg);
                dic.Add(tt, reg);
            }
            return dic;
        }

        public static List<RegistryKey> ToList(this RegistryKey src)
        {
            List<RegistryKey> ll = new List<RegistryKey>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                ll.Add(reg);
            }
            return ll;
        }


        public static IEnumerable<IGrouping<TKey, RegistryKey>> GroupBy<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func)
        {
            Dictionary<TKey, Grouping<TKey, RegistryKey>> dic = new Dictionary<TKey, Grouping<TKey, RegistryKey>>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TKey key = func.Invoke(reg);
                if(dic.ContainsKey(key) == true)
                {
                    dic[key].Add(reg);
                }
                else
                {
                    dic.Add(key, new Grouping<TKey, RegistryKey>(key, reg));
                    yield return dic[key];
                }
                
            }
        }

        public static ILookup<TKey, RegistryKey> ToLookup<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func)
        {
            Lookup<TKey, RegistryKey> ll = null;
            
            return ll;
            //System.Linq.Lookup<TKey, RegistryKey> lookup = new Lookup<TKey, RegistryKey>();
            //foreach (var subkeyname in src.GetSubKeyNames())
            //{
            //    RegistryKey reg = src.OpenSubKey(subkeyname);
            //    TKey key = func.Invoke(reg);
            //    eld return key;
            //    reg.Dispose();
            //    reg = null;
            //}

            return null;
        }


        public static T GetValue<T>(this RegistryKey src, string name)
        {
            T t =  default(T);
            if(src.GetValueNames().Any(x=>x==name) == true)
            {
                object obj = src.GetValue(name);
                t = (T)obj;
            }
            else
            {
                if(Type.GetTypeCode(typeof(T)) == TypeCode.String)
                {
                    object obj = string.Empty;
                    t = (T)obj;
                }
            }
            return t;
        }
    }

    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        readonly List<TElement> elements = new List<TElement>();

        public void Add(TElement data)
        {
            this.elements.Add(data);
        }
        public Grouping(TKey key, TElement value)
        {
            Key = key;
            elements.Add(value);
        }

        public TKey Key { get; private set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    }
}
