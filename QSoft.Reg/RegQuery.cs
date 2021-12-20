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

        //public void CreateOrUpdate(T data)
        //{
        //    var setting = (this.Provider as RegProvider<T>)?.Setting;
        //    RegistryKey reg = setting?.Create(true);
        //    if (reg == null)
        //    {
        //        Regex regex1 = new Regex(@"^(?<parent>.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
        //        string subkey = setting.SubKey;
        //        var match = regex1.Match(subkey);
        //        if(match.Success)
        //        {
        //            var basekey = RegistryKey.OpenBaseKey(setting.Hive, setting.View);
        //            var parentreg = basekey.OpenSubKey(match.Groups["parent"].Value, true);
        //            reg = parentreg.CreateSubKey(match.Groups["path"].Value, RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        }
        //    }


        //    if(reg != null)
        //    {
        //        var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
        //        foreach(var pp in pps)
        //        {
        //            reg.SetValue(pp.Name, pp.GetValue(data, null));
        //        }
        //    }
        //}

        //public T Get()
        //{
        //    RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Create();
        //    return reg.ToFunc<T>()(reg);
        //}

        //public void Delete()
        //{
        //    RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting?.Create();
        //    var parent = reg.GetParent();
        //    Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
        //    var match = regex1.Match(reg.Name);
        //    if (match.Success)
        //    {
        //        parent.DeleteSubKeyTree(match.Groups["path"].Value);
        //    }
        //}
    }

    public class RegSetting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
        public RegistryView View { set; get; }

        public RegistryKey Create(bool write=false)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(this.Hive, this.View);
            if (string.IsNullOrEmpty(this.SubKey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(this.SubKey, write);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }
        //public static implicit operator RegistryKey(RegSetting data)
        //{
        //    RegistryKey reg_base = RegistryKey.OpenBaseKey(data.Hive, RegistryView.Registry64);
        //    if (string.IsNullOrEmpty(data.SubKey) == false)
        //    {
        //        RegistryKey reg = reg_base.OpenSubKey(data.SubKey);
        //        reg_base.Dispose();
        //        return reg;
        //    }
        //    return reg_base;
        //}
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
