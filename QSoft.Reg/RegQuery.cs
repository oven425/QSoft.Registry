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

    public class EqualityComparerAll<T> : IEqualityComparer<T>
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
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;
            List<int> hashcodes = new List<int>();
            var pps = typeof(T).GetProperties().Where(p => p.CanRead == true);
            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.PropertyType);

                var ss1 = pp.GetValue(obj, null);
                if(ss1 == null)
                {
                    hashcodes.Add(0);
                }
                else
                {
                    hashcodes.Add(ss1.GetHashCode());
                }
            }

            int hashcode = hashcodes.Aggregate((x, y) => x ^ y);
            System.Diagnostics.Trace.WriteLine($"hashcode:{hashcode}");
            return hashcode;
            //Get hash code for the Name field if it is not null.
            //int hashProductName = product.Name == null ? 0 : product.Name.GetHashCode();

            ////Get hash code for the Code field.
            //int hashProductCode = product.Code.GetHashCode();

            ////Calculate the hash code for the product.
            //return hashProductName ^ hashProductCode;
        }
    }
}
