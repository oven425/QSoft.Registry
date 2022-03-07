using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
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

        //RegSetting m_Setting;
        RegProvider<T> m_Provider = new RegProvider<T>();
        public RegQuery<T> useSetting(Action<RegSetting> data)
        {
            
            data(this.m_Provider.Setting);
            //m_Setting = provider.Setting;
            this.Provider = this.m_Provider;
            return this;
        }


        public RegQuery<T> HasDefault1(Action<T> data)
        {
            //var obj = Activator.CreateInstance<T>();
            //data(obj);
            //System.Threading.Thread.Sleep(2000);
            //data(obj);
            this.m_Provider.DefaultValue = data;

            return this;
        }

        private Expression<Action<T>> FuncToExpression(Action<T> f)
        {
            return x => f(x);
        }

        public RegQuery<T> HasDefault(Expression<Func<T>> data)
        {
            //Expression.Lambda<T>()

            return this;
        }

        public RegQuery(IQueryProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        public Expression Expression { private set; get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider { private set; get; }
        public IEnumerator<T> GetEnumerator()
        {
            var provider = this.Provider as RegProvider<T>;
            if(provider != null)
            {
                var fail = provider.CheckFail();
                if (fail != null)
                {
                    throw fail;
                }
                //return provider.Execute<IEnumerable<T>>(provider.Expr_Dst).GetEnumerator();
            }
            
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }





        [Obsolete("Testing", true)]
        public void Create(T data, bool isoverwrite=false)
        {
            var setting = (this.Provider as RegProvider<T>)?.Setting;
            RegistryKey reg = setting?.Open(true);
            if(reg != null && isoverwrite==false)
            {
                throw new Exception("Exist RegsistryKey");
            }
            else if(reg != null)
            {
                var names = reg.GetValueNames();
                foreach(var oo in names)
                {
                    reg.DeleteValue(oo);
                }
            }
            else
            {
                reg = setting?.Create(true);
            }
            var pps = this.DumpPropertys(data);
            foreach(var pp in pps)
            {
                var oo = pp.Key.GetValue(data, null);
                reg.SetValue(pp.Value, oo);
            }

        }

        Dictionary<PropertyInfo, string> DumpPropertys(T data)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
            Dictionary<PropertyInfo, string> dicpps = new Dictionary<PropertyInfo, string>();
            foreach (var pp in pps)
            {
                var attr = pp.GetCustomAttributes(true).FirstOrDefault();
                string ppname = "";
                if (attr is RegIgnore)
                {

                }
                else if (attr is RegPropertyName)
                {
                    ppname = (attr as RegPropertyName)?.Name;
                }
                else if (attr is RegSubKeyName)
                {

                }
                else
                {
                    ppname = pp.Name;
                }
                if (string.IsNullOrEmpty(ppname) == false)
                {
                    dicpps[pp] = ppname;
                    var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
                    if (regnames.Length > 0)
                    {
                        dicpps[pp] = regnames[0].Name;
                    }
                }

            }
            return dicpps;
        }
        [Obsolete("Testing", true)]
        public void Update(T data)
        {
            var setting = (this.Provider as RegProvider<T>)?.Setting;
            RegistryKey reg = setting?.Open(true);
            var pps = this.DumpPropertys(data);
            foreach (var pp in pps)
            {
                var oo = pp.Key.GetValue(data, null);
                reg.SetValue(pp.Value, oo);
            }
        }
        [Obsolete("Testing", true)]
        public T Get()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Open();
            return reg.ToDataFunc<T>()(reg);
        }
        [Obsolete("Testing", true)]
        public void Delete()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Open();
            var parent = reg.GetParent();
            Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
            var match = regex1.Match(reg.Name);
            if (match.Success)
            {
                parent.DeleteSubKeyTree(match.Groups["path"].Value);
            }
        }

        void Admin(string se)
        {
            IntPtr token;
            NativeMethod.OpenProcessToken(NativeMethod.GetCurrentProcess(), NativeMethod.TOKEN_ALL_ACCESS, out token);
            NativeMethod.LUID PrivilegeRequired = new NativeMethod.LUID();
            bool bRes = false;
            bRes = NativeMethod.LookupPrivilegeValue(null, se, ref PrivilegeRequired);
            
            NativeMethod.TOKEN_PRIVILEGES tp = new NativeMethod.TOKEN_PRIVILEGES();
            tp.PrivilegeCount = 1;
            tp.Privileges[0].Luid = PrivilegeRequired;
            tp.Privileges[0].Attributes = NativeMethod.SE_PRIVILEGE_ENABLED;
            NativeMethod.TOKEN_PRIVILEGES tp1 = new NativeMethod.TOKEN_PRIVILEGES();
            uint hr;
            bRes = NativeMethod.AdjustTokenPrivileges(token, false, ref tp, (uint)Marshal.SizeOf(tp), ref tp1, out hr);

            NativeMethod.CloseHandle(token);
        }

        [Obsolete("Testing", true)]
        public void Backup(string filename, bool overwrite=true)
        {
            if(File.Exists(filename) == true&& overwrite==false)
            {
                throw new Exception("File is Existed");
            }

        }

        [Obsolete("Testing", true)]
        public void Restore(string filename)
        {
            if (File.Exists(filename) == true)
            {

            }
        }

        public RegistryKey ToRegistryKey()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Create(true);
            return reg;
        }
    }

    public class EqualityComparerForce<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            var pps = typeof(T).GetProperties().Where(p => p.CanRead == true);
            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.PropertyType);
                dynamic s1 = pp.GetValue(x, null);
                dynamic s2 = pp.GetValue(y, null);
                if(s1 != s2)
                {
                    return false;
                }
                
            }
            return true;
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
