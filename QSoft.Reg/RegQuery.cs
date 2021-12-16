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

        public T Dump()
        {
            RegistryKey reg = (this.Provider as RegProvider<T>)?.Setting;
            return reg.ToFunc<T>()(reg);
        }
    }

    public class RegSetting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
        public RegistryView View { set; get; }


        public static implicit operator RegistryKey(RegSetting data)
        {
            RegistryKey reg_base = RegistryKey.OpenBaseKey(data.Hive, RegistryView.Registry64);
            if (string.IsNullOrEmpty(data.SubKey) == false)
            {
                RegistryKey reg = reg_base.OpenSubKey(data.SubKey);
                reg_base.Dispose();
                return reg;
            }
            return reg_base;
        }
    }

    public class EqualityComparer1<T> : IEqualityComparer<T>
    {
        
        public bool Equals(T x, T y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            Type type1 = x.GetType();
            Type type2 = y.GetType();
            if (type1 != type2)
            {
                return false;
            }

            var pps = type1.GetProperties().Where(p => p.CanRead == true);
            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.PropertyType);

                dynamic ss1 = pp.GetValue(x, null);
                dynamic ss2 = pp.GetValue(y, null);
                if (ss1 != ss2)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(T obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}
