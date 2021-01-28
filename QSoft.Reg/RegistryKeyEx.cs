using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace QSoft.Registry
{
    public static class RegistryKeyEx
    {
        public static T GetValue<T>(this RegistryKey src, string name)
        {
            T t = default(T);
            TypeCode typecode = Type.GetTypeCode(typeof(T));

            if (typecode == TypeCode.String)
            {
                object obj = src.GetValue(name);
                t = (T)Convert.ChangeType(obj, typeof(T));
                if(t==null)
                {
                    object str_empty = string.Empty;
                    t = (T)str_empty;
                }
            }
            else
            {
                object obj = src.GetValue(name);
                t = (T)obj;
            }
            return t;
        }

        public static T GetValueSafe<T>(this RegistryKey src, string name)
        {
            T t = default(T);
            TypeCode typecode = Type.GetTypeCode(typeof(T));
            string[] sss = src.GetValueNames();

            //bool vv = false;
            //for(int i=0; i<sss.Length; i++)
            //{
            //    if(sss[i] == name)
            //    {
            //        vv = true;
            //        break;
            //    }
            //}
            ////if (src.GetValueNames().Count(x => x == name) >0)
            ////if (src.GetValueNames().FirstOrDefault(x => x == name) != null)
            //if(vv == true)
            //{
            //    if (typecode == TypeCode.String)
            //    {
            //        object obj = src.GetValue(name);
            //        t = (T)Convert.ChangeType(obj, typeof(T));
            //    }
            //    else
            //    {
            //        object obj = src.GetValue(name);
            //        t = (T)obj;
            //    }
            //}
            //else
            //{
            //    //if (Type.GetTypeCode(typeof(T)) == TypeCode.String)
            //    //{
            //    //    object obj = string.Empty;
            //    //    t = (T)obj;
            //    //}
            //}
            return t;
        }
    }
}

namespace QSoft.Registry.Linq
{
    public static class RegistryKeyLinq
    {

        //public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector);
        public static IEnumerable<TResult> Join<TInner, TResult>(this RegistryKey src, IEnumerable<TInner> inner, Func<RegistryKey, TInner, bool> check, Func<RegistryKey, TInner, TResult> resultSelector)
        {
            string[] subkeynames = src.GetSubKeyNames();
            Dictionary<RegistryKey, TInner> dic = new Dictionary<RegistryKey, TInner>();
            foreach (var subkeyname in subkeynames)
            {

                RegistryKey reg = src.OpenSubKey(subkeyname);

                // TKey key_reg = outerKeySelector.Invoke(reg);
                foreach (var oo in inner)
                {
                    if (check.Invoke(reg, oo) == true)
                    {
                        yield return resultSelector.Invoke(reg, oo);
                    }
                    //TKey key = innerKeySelector.Invoke(oo);
                    //if (key.Equals(key_reg) == true)
                    //{
                    //    dic.Add(reg, oo);

                    //    yield return resultSelector.Invoke(reg, oo);
                    //    //yield break;
                    //}
                }

                //bool has = inner.Any(x=> innerKeySelector.Invoke(x))
                //yield return reg;
            }
        }
        public static IEnumerable<TResult> Join<TInner, TKey, TResult>(this RegistryKey src, IEnumerable<TInner> inner, Func<RegistryKey, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<RegistryKey, TInner, TResult> resultSelector)
        {
            string[] subkeynames = src.GetSubKeyNames();
            Dictionary<RegistryKey, TInner> dic = new Dictionary<RegistryKey, TInner>();
            foreach (var subkeyname in subkeynames)
            {

                RegistryKey reg = src.OpenSubKey(subkeyname);
                TKey key_reg = outerKeySelector.Invoke(reg);
                foreach (var oo in inner)
                {
                    TKey key = innerKeySelector.Invoke(oo);
                    if (key.Equals(key_reg) == true)
                    {
                        dic.Add(reg, oo);

                        yield return resultSelector.Invoke(reg, oo);
                        //yield break;
                    }
                }

                //bool has = inner.Any(x=> innerKeySelector.Invoke(x))
                //yield return reg;
            }
        }

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
                    reg.Close();
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

                reg.Close();
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
                    reg.Close();
                    reg = null;
                }
            }
            return count;
        }

        public static IEnumerable<TResult> Select<TResult>(this RegistryKey src, Func<RegistryKey, TResult> select)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TResult obj = select.Invoke(reg);
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
                    reg.Close();
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
                    reg.Close();
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
                if (dic.ContainsKey(key) == true)
                {
                    dic[key].Add(reg);
                }
                else
                {
                    dic.Add(key, new Grouping<TKey, RegistryKey>(key, reg));

                }
                
            }
            for(int i=0; i<dic.Count; i++)
            {
                yield return dic.ElementAt(i).Value;
            }
        }

        public static ILookup<TKey, RegistryKey> ToLookup<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func)
        {

            Lookup<TKey, RegistryKey> ll = new Lookup<TKey, RegistryKey>();
            

            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TKey key = func.Invoke(reg);
                ll.Add(key, reg);
            }
            return ll;
        }

        public static IEnumerable<RegistryKey> Take(this RegistryKey src, int count)
        {
            var takes = src.GetSubKeyNames().Take(count);
            foreach(var take in takes)
            {
                RegistryKey reg = src.OpenSubKey(take);
                yield return reg;
            }
        }
    }


    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        Dictionary<TKey, Grouping<TKey, TElement>> dic = new Dictionary<TKey, Grouping<TKey, TElement>>();
        public IEnumerable<TElement> this[TKey key] => dic[key];
        public void Add(TKey key, TElement value)
        {
            if(this.dic.ContainsKey(key) == false)
            {
                this.dic.Add(key, new Grouping<TKey, TElement>(key, value));
            }
            else
            {
                this.dic[key].Add(value);
            }
        }
        public int Count => throw new NotImplementedException();

        public bool Contains(TKey key)
        {
            return dic.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.Values.GetEnumerator();
        }
    }

    internal class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
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
