using System;
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

        public static Dictionary<TKey, RegistryKey> ToDictionary<TKey>(this RegistryKey src, Func<RegistryKey, TKey> func)
        {
            Dictionary<TKey, RegistryKey> dic = new Dictionary<TKey, RegistryKey>();
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                TKey tt = func.Invoke(reg);
                dic.Add(tt, reg);
                reg.Dispose();
                reg = null;
            }
            return dic;
        }

        public static void GroupBy(this RegistryKey src)
        {
            foreach (var subkeyname in src.GetSubKeyNames())
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                //TKey tt = func.Invoke(reg);
                //dic.Add(tt, reg);
                reg.Dispose();
                reg = null;
            }
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
}
