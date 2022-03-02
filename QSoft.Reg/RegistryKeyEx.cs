using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace QSoft.Registry
{
    public static class RegistryKeyEx
    {
        public interface IHeiratey<T> :IEnumerable<T>
        {
            IEnumerable<T> Childs { set; get; }

        }

        public static IEnumerable<RegistryKey> FindAll(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            var subkeys = src.GetSubKeyNames();
            foreach(var subkey in subkeys)
            {
                System.Diagnostics.Trace.WriteLine(subkey);
                var reg = src.OpenSubKey(subkey);
                if(func(reg) == true)
                {
                    yield return reg;
                }
                else
                {
                    
                    var rr = reg.FindAll(func);
                    foreach(var oo in rr)
                    {
                        yield return oo;
                    }
                    reg.Close();
                    
                }
            }
        }

        public static T GetValue<T>(this RegistryKey src, string name)
        {
            T t = default(T);
            Type type = typeof(T);
           
            TypeCode typecode = Type.GetTypeCode(type);

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
            else if(typecode == TypeCode.Object && typeof(T) == typeof(Version))
            {
                var obj = src.GetValue(name);
                Version ver = null;
                Version.TryParse(obj as string, out ver);
                t = (T)Convert.ChangeType(ver, typeof(T));
            }
            else
            {
                object obj = src.GetValue(name);
                if(obj is string)
                {
                    if(typecode == TypeCode.Object)
                    {
                        if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var typecode_null = Type.GetTypeCode(type.GetGenericArguments().FirstOrDefault());
                            object obj_null = null;
                            switch (typecode_null)
                            {
                                case TypeCode.Boolean:
                                    {
                                        obj_null = Convert.ToBoolean(obj);
                                    }
                                    break;
                                case TypeCode.Byte:
                                    {
                                        obj_null = Convert.ToByte(obj);
                                    }
                                    break;
                                case TypeCode.Char:
                                    {
                                        obj_null = Convert.ToChar(obj);
                                    }
                                    break;
                                case TypeCode.DateTime:
                                    {
                                        obj_null = Convert.ToDateTime(obj);
                                    }
                                    break;
                                case TypeCode.Decimal:
                                    {
                                        obj_null = Convert.ToDecimal(obj);
                                    }
                                    break;
                                case TypeCode.Double:
                                    {
                                        obj_null = Convert.ToDouble(obj);
                                    }
                                    break;
                                case TypeCode.Int16:
                                    {
                                        obj_null = Convert.ToInt16(obj);
                                    }
                                    break;
                                case TypeCode.Int32:
                                    {
                                        obj_null = Convert.ToInt32(obj);
                                    }
                                    break;
                                case TypeCode.Int64:
                                    {
                                        obj_null = Convert.ToInt64(obj);
                                    }
                                    break;
                                case TypeCode.SByte:
                                    {
                                        obj_null = Convert.ToSByte(obj);
                                    }
                                    break;
                                case TypeCode.Single:
                                    {
                                        obj_null = Convert.ToSingle(obj);
                                    }
                                    break;
                                case TypeCode.String:
                                    {
                                        obj_null = obj;
                                    }
                                    break;
                                case TypeCode.UInt16:
                                    {
                                        obj_null = Convert.ToUInt16(obj);
                                    }
                                    break;
                                case TypeCode.UInt32:
                                    {
                                        obj_null = Convert.ToUInt32(obj);
                                    }
                                    break;
                                case TypeCode.UInt64:
                                    {
                                        obj_null = Convert.ToUInt64(obj);
                                    }
                                    break;
                            }
                            t = (T)obj_null;
                        }
                    }
                    else
                    {
                        t = (T)Convert.ChangeType(obj, typeof(T));
                    }
                }
                else
                {
                    
                    switch(typecode)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            {
                                return (T)Convert.ChangeType(obj, typeof(T));
                            }
                            break;
                        case TypeCode.DateTime:
                            {

                            }
                            break;
                        default:
                            {
                                t = (T)obj;
                            }
                            break;
                    }
                    
                }
            }
            return t;
        }

        //public static T Converter<T>(this object src)
        //{
        //    var typecode = Type.GetTypeCode(typeof(T));
        //    switch(typecode)
        //    {
        //        case TypeCode.UInt32:
        //            {
        //                return Convert.ToUInt32(src);
        //            }
        //            break;
        //    }
        //    return default(T);
        //}

        public static T GetValue<T>(this RegistryKey src, string subkey, string name)
        {
            T t = default(T);
            RegistryKey reg = null;
            if (string.IsNullOrEmpty(subkey) == false)
            {
                reg = src.OpenSubKey(subkey);
            }
            else
            {
                reg = src;
            }
            TypeCode typecode = Type.GetTypeCode(typeof(T));
            if (reg == null)
            {
                if (typecode == TypeCode.String)
                {
                    object str_empty = string.Empty;
                    t = (T)str_empty;
                }
            }
            else
            {
                if (typecode == TypeCode.String)
                {
                    object obj = reg.GetValue(name);
                    t = (T)Convert.ChangeType(obj, typeof(T));
                    if (t == null)
                    {
                        object str_empty = string.Empty;
                        t = (T)str_empty;
                    }
                }
                else
                {
                    object obj = reg.GetValue(name);
                    t = (T)obj;
                }
            }
            if (reg != src)
            {
                reg.Dispose();
            }
            return t;
        }

        public static IEnumerable<RegistryKey> OpenView(this RegistryHive src, string subkey, bool view32 = true, bool view64 = true, bool writable = false)
        {
            if (view64 == true)
            {
                yield return src.OpenView64(subkey, writable);
            }
            if (view32 == true)
            {
                yield return src.OpenView32(subkey, writable);
            }
        }

        public static RegistryKey OpenView64(this RegistryHive src, string subkey, bool writable = false)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(src, RegistryView.Registry64);
            if (string.IsNullOrEmpty(subkey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(subkey, writable);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }

        public static RegistryKey OpenView32(this RegistryHive src, string subkey, bool writable = false)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(src, RegistryView.Registry32);
            if (string.IsNullOrEmpty(subkey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(subkey, writable);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }
    }
}

namespace QSoft.Registry.Linq
{
    [Obsolete("No supoort, please use RegQuery<T>")]
    public static class RegistryKeyLinq
    {
        public static IEnumerable<TResult> Join<TInner, TResult>(this RegistryKey src, IEnumerable<TInner> inner, Func<RegistryKey, TInner, bool> check, Func<RegistryKey, TInner, TResult> resultSelector)
        {
            string[] subkeynames = src.GetSubKeyNames();
            Dictionary<RegistryKey, TInner> dic = new Dictionary<RegistryKey, TInner>();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                bool hastrue = false;
                foreach (var oo in inner)
                {
                    if (check.Invoke(reg, oo) == true)
                    {
                        hastrue = true;
                        yield return resultSelector.Invoke(reg, oo);
                    }
                }
                if (hastrue == false)
                {
                    reg.Dispose();
                }
            }
        }

        public static IEnumerable<TResult> Join<TInner, TKey, TResult>(this RegistryKey src, IEnumerable<TInner> inner, Func<RegistryKey, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<RegistryKey, TInner, TResult> resultSelector, bool writable = false)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {

                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
                TKey key_reg = outerKeySelector.Invoke(reg);
                bool hastrue = false;
                foreach (var oo in inner)
                {
                    TKey key = innerKeySelector.Invoke(oo);
                    if (key.Equals(key_reg) == true)
                    {
                        hastrue = true;
                        yield return resultSelector.Invoke(reg, oo);
                    }
                }
                if (hastrue == false)
                {
                    reg.Dispose();
                }
            }
        }

        public static IEnumerable<RegistryKey> Where(this RegistryKey src, Func<RegistryKey, bool> func, bool writable = false)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
                if (func.Invoke(reg) == true)
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
                if (bb == true)
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
                reg.Close();
                reg = null;
            }
            return count;
        }

        public static IEnumerable<TResult> Select<TResult>(this RegistryKey src, Func<RegistryKey, TResult> select)
        {
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TResult obj = select.Invoke(reg);
                yield return obj;
            }
        }

        public static RegistryKey FirstOrDefault(this RegistryKey src, Func<RegistryKey, bool> func, bool writable = false)
        {
            RegistryKey hr = null;
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
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

        public static RegistryKey FirstOrDefault(this RegistryKey src, bool writable = false)
        {
            RegistryKey hr = null;
            if (src.GetSubKeyNames().Length > 0)
            {
                hr = src.OpenSubKey(src.GetSubKeyNames()[0], writable);
            }
            return hr;
        }

        public static RegistryKey LastOrDefault(this RegistryKey src, Func<RegistryKey, bool> func, bool writable = false)
        {
            RegistryKey hr = null;
            string[] subkeynames = src.GetSubKeyNames();
            for (int i = subkeynames.Length - 1; i >= 0; i--)
            {
                RegistryKey reg = src.OpenSubKey(subkeynames[i], writable);
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

        public static RegistryKey LastOrDefault(this RegistryKey src, bool writable = false)
        {
            RegistryKey hr = null;
            if (src.GetSubKeyNames().Length > 0)
            {
                hr = src.OpenSubKey(src.GetSubKeyNames()[src.GetSubKeyNames().Length - 1], writable);
            }
            return hr;
        }

        public static Dictionary<TKey, RegistryKey> ToDictionary<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func, bool writable = false)
        {
            Dictionary<TKey, RegistryKey> dic = new Dictionary<TKey, RegistryKey>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
                TKey tt = func.Invoke(reg);
                dic.Add(tt, reg);
            }
            return dic;
        }

        public static List<RegistryKey> ToList(this RegistryKey src, bool writable = false)
        {
            List<RegistryKey> ll = new List<RegistryKey>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
                ll.Add(reg);
            }
            return ll;
        }


        public static IEnumerable<IGrouping<TKey, RegistryKey>> GroupBy<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func, bool writable = false)
        {
            Dictionary<TKey, Grouping<TKey, RegistryKey>> dic = new Dictionary<TKey, Grouping<TKey, RegistryKey>>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
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
            for (int i = 0; i < dic.Count; i++)
            {
                yield return dic.ElementAt(i).Value;
            }
        }

        public static ILookup<TKey, RegistryKey> ToLookup<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func, bool writable = false)
        {

            Lookup<TKey, RegistryKey> ll = new Lookup<TKey, RegistryKey>();


            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname, writable);
                TKey key = func.Invoke(reg);
                ll.Add(key, reg);
            }
            return ll;
        }

        public static IEnumerable<RegistryKey> Take(this RegistryKey src, int count, bool writable = false)
        {

            var takes = src.GetSubKeyNames().Take(count);
            foreach (var take in takes)
            {
                RegistryKey reg = src.OpenSubKey(take, writable);
                yield return reg;
            }
        }

        public static IEnumerable<RegistryKey> Take(this RegistryKey src, Func<string, bool> func, bool writable = false)
        {
            var takes = src.GetSubKeyNames().Where(x => func(x));
            foreach (var take in takes)
            {
                RegistryKey reg = src.OpenSubKey(take, writable);
                yield return reg;
            }
        }
    }

    [Obsolete("No supoort, please use RegQuery<T>")]
    internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        Dictionary<TKey, Grouping<TKey, TElement>> dic = new Dictionary<TKey, Grouping<TKey, TElement>>();
        public IEnumerable<TElement> this[TKey key] => dic[key];
        public void Add(TKey key, TElement value)
        {
            if (this.dic.ContainsKey(key) == false)
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

    [Obsolete("No supoort, please use RegQuery<T>")]
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
