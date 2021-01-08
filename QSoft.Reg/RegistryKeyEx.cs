using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace QSoft.Reg.Linq
{
    public static class RegistryKeyEx
    {
        public static IEnumerable<RegistryKey> Where(this RegistryKey src, Func<RegistryKey, bool> func)
        {
            List<RegistryKey> ll = new List<RegistryKey>();
            string[] subkeynames = src.GetSubKeyNames();
            foreach(var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                if(func.Invoke(reg) == true)
                {
                    ll.Add(reg);
                }
                else
                {
                    reg.Dispose();
                    reg = null;
                }
            }
            return ll;
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


        public static void Select(this RegistryKey src, Func<RegistryKey, object> select)
        {
            string[] subkeynames = src.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                RegistryKey reg = src.OpenSubKey(subkeyname);
                object obj = select.Invoke(reg);
                obj = null;
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
