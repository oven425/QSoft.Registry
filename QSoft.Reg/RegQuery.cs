using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        RegSetting m_Setting;
        
        public RegQuery<T> useSetting(Action<RegSetting> data)
        {
            var provider = new RegProvider<T>();
            
            data(provider.Setting);
            m_Setting = provider.Setting;
            this.Provider = provider;
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
            var fail = (this.Provider as RegProvider<T>)?.CheckFail();
            if(fail != null)
            {
                throw fail;
            }
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

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

        public T Get()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Open();
            return reg.ToDataFunc<T>()(reg);
        }

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
