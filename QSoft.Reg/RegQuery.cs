using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QSoft.Registry
{

    public class RegQuery<T> : IOrderedQueryable<T>
    {
        public RegQuery()
        {
            this.Expression = Expression.Constant(this);
        }

        Setting m_Setting = null;
        public RegQuery<T> useSetting(Action<Setting> data)
        {
            this.m_Setting = new Setting();
            data(this.m_Setting);
            this.Provider = new RegProvider(this.m_Setting.Hive, this.m_Setting.SubKey, typeof(T));
            return this;
        }

        public RegQuery(RegProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
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

    public class Setting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
    }

    public static class RegQueryEx
    {

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
        {
            return f.Method;
        }

        public static IQueryable<T> Update<T>(this IQueryable<T> source, Expression<Func<T, T>> selector)
        {
            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    GetMethodInfo(RegQueryEx.Update, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));

        }

        public static int Update1<TSource>(this IQueryable<TSource> source)
        {
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Count, source),
                    new Expression[] { source.Expression }
                    ));
        }

        public static int Update1<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.Provider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Count, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        //public static int Update1<TSource>(this IQueryable<TSource> source, Expression<Action<TSource>> predicate)
        //{
        //    return source.Provider.Execute<int>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfo(RegQueryEx.Update1, source, predicate),
        //            new Expression[] { source.Expression, Expression.Quote(predicate) }
        //            ));
        //}
    }
}
