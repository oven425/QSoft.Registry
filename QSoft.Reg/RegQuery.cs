using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QSoft.Registry.Linq
{
    public class RegQuery<T> : IOrderedQueryable<T>
    {
        public RegQuery()
        {
            this.Expression = Expression.Constant(this);
        }

        public RegQuery<T> useSetting(Action<RegSetting> data)
        {
            var provider = new RegProvider(typeof(T));
            data(provider.Setting);
            this.Provider = provider;
            return this;
        }

        public RegQuery(RegProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        public RegQuery(RegProvider provider, Expression expression, bool isfirst, Expression regsource)
        {
            this.Provider = provider;
#if CreateQuery
            ////if(isfirst == true)
            //MethodCallExpression method1 = expression as MethodCallExpression;
            ////if(method1.Arguments[0].NodeType == ExpressionType.Constant)
            //{
            //    //List<RegistryKey> regs = new List<RegistryKey>();
            //    //RegistryKey registry = provider.Setting;
            //    //var subkeynames = registry.GetSubKeyNames();

            //    //foreach (var subkeyname in subkeynames)
            //    //{
            //    //    regs.Add(registry.OpenSubKey(subkeyname));
            //    //}
            //    //var tte = regs.AsQueryable();

            //    RegExpressionVisitor reg = new RegExpressionVisitor();
            //    this.Expression = reg.Visit(expression, typeof(T), null, regsource);
            //}
            ////else
            ////{
            ////    this.Expression = expression;
            ////}
            this.Expression = expression;
#else
            
            this.Expression = expression;
#endif
        }


        public Expression Expression { private set; get; }
        public Type ElementType => typeof(T);
        public IQueryProvider Provider { private set; get; }
        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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

    public static class RegQueryEx
    {
        public static int Update<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource,TResult>> selector)
        {
            var updates = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Update");
            var methdodcall = Expression.Call(updates.Last().MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector);
            return source.Provider.Execute<int>(methdodcall);
        }

        public static int Update<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> data)
        {
            var regs = source as IEnumerable<RegistryKey>;
            if(regs == null)
            {
                throw new Exception("source must be RegistryKey");
            }
            var pps = data.GetType().GetGenericArguments()[1].GetProperties().Where(x => x.CanRead == true);
            foreach (var oo in source)
            {
                RegistryKey reg = oo as RegistryKey;
                var obj = data(oo);
                foreach(var pp in pps)
                {
                    object vv = pp.GetValue(obj);
                    if(vv != null)
                    {
                        reg.SetValue(pp.Name, vv);
                    }
                }
            }
            
            return source.Count();
        }

        static public int Replace(this Type[] datas, Type src, Type dst)
        {
            int count = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == src)
                {
                    datas[i] = dst;
                }
            }

            return count;
        }
    }
}
