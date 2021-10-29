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

        //Setting m_Setting = null;
        public RegQuery<T> useSetting(Action<Setting> data)
        {
            //this.m_Setting = new Setting();
            //data(this.m_Setting);
            //this.Provider = new RegProvider(this.m_Setting.Hive, this.m_Setting.SubKey, typeof(T));

            var provider = new RegProvider(typeof(T));
            data(provider.Setting);
            this.Provider = provider;
            return this;
        }

        public RegQuery(RegProvider provider, Expression expression, bool isfirst)
        {
            this.Provider = provider;
#if CreateQuery
            if(isfirst == true)
            {
                List<RegistryKey> regs = new List<RegistryKey>();
                RegistryKey registry = provider.Setting;
                var subkeynames = registry.GetSubKeyNames();

                foreach (var subkeyname in subkeynames)
                {
                    regs.Add(registry.OpenSubKey(subkeyname));
                }
                var tte = regs.AsQueryable();
                RegExpressionVisitor reg = new RegExpressionVisitor();
                this.Expression = reg.Visit(expression, typeof(T), tte);
            }
            else
            {
                this.Expression = expression;
            }
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

    public class Setting
    {
        public string SubKey { set; get; }
        public RegistryHive Hive { set; get; }
        public RegistryView View { set; get; }

        //public static RegistryKey operator =(Setting a)
        //{
        //    RegistryKey reg = null;
        //    return reg;
        //}

        public static implicit operator RegistryKey(Setting data)
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
        //public static int Update<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, TSource>> selector)
        //{
        //    var methods = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Update");
        //    var pps = methods.ElementAt(0).GetParameters();
        //    var first = typeof(RegQueryEx).GetMethods().Where(x => x.Name == "Update");
        //    var methdodcall = Expression.Call(first.First().MakeGenericMethod(typeof(TSource)), source.Expression, selector);
        //    return source.Provider.Execute<int>(methdodcall);
        //}

        //public static int Update<TSource>(this IEnumerable<TSource> src) where TSource: struct
        //{
        //    return src.Count();
        //}
    }

    //public static class TestEx
    //{
    //    public static int RemoveAll<TSource>(this IQueryable<TSource> source)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x => x.Name == "RemoveAll");
    //        var methdodcall = Expression.Call(first.First().MakeGenericMethod(typeof(TSource)), source.Expression);
    //        return source.Provider.Execute<int>(methdodcall);
    //    }

    //    public static int RemoveAll<TSource>(this IEnumerable<RegistryKey> src)
    //    {
    //        foreach(var oo in src)
    //        {
    //            var name = oo.Name;
    //            int index = name.LastIndexOf("\\");

    //            Uri uri = new Uri(name);
    //            //Uri uri = new Uri(oo.)
    //        }

    //        return src.Count();
    //    }


    //    public static bool Update<TSource>(this TSource src, string value)
    //    {
    //        return true;
    //    }

    //    public static int Update<TSource>(this IQueryable<TSource> source)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x=>x.Name == "Update");
    //        var methdodcall = Expression.Call(first.Last().MakeGenericMethod(typeof(TSource)), source.Expression);
    //        return source.Provider.Execute<int>(methdodcall);
    //    }

    //    public static int Update<TSource>(this IEnumerable<TSource> src)
    //    {
    //        return src.Count();
    //    }

    //    public static int Update<TSource>(this IQueryable<TSource> source, Expression<Action<TSource>> expression)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x => x.Name == "Update");
    //        var methdodcall = Expression.Call(first.Last().MakeGenericMethod(typeof(TSource)), source.Expression, expression);
    //        return source.Provider.Execute<int>(methdodcall);
    //    }

    //    public static int Update<TSource>(this IQueryable<TSource> source, Expression<Func<string>> expression)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x => x.Name == "Update");
    //        var methdodcall = Expression.Call(first.Last().MakeGenericMethod(typeof(TSource)), source.Expression, expression);
    //        return source.Provider.Execute<int>(methdodcall);
    //    }

    //    public static int Update<TSource>(this IEnumerable<TSource> src, Action<TSource> action)
    //    {
    //        foreach(var oo in src)
    //        {
    //            action(oo);
    //        }
    //        return src.Count();
    //    }

    //    public static IQueryable<TSource> Change<TSource>(this IQueryable<TSource> source, Expression<Action<TSource>> expression)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x => x.Name == "Change");
    //        var methdodcall = Expression.Call(null, first.First().MakeGenericMethod(typeof(TSource)), source.Expression, expression);
    //        return source.Provider.CreateQuery<TSource>(methdodcall);
    //    }

    //    public static IEnumerable<TSource> Change<TSource>(this IEnumerable<TSource> src, Action<TSource> action)
    //    {
    //        foreach (var oo in src)
    //        {
    //            action(oo);
    //            yield return oo;
    //        }
    //        //return default(TSource);
    //    }

    //    public static int SaveChanges<TSource>(this IQueryable<TSource> source)
    //    {
    //        var first = typeof(TestEx).GetMethods().Where(x => x.Name == "SaveChanges");
    //        var methdodcall = Expression.Call(first.First().MakeGenericMethod(typeof(TSource)), source.Expression);
    //        return source.Provider.Execute<int>(methdodcall);
    //    }

    //    public static int SaveChanges<TSource>(this IEnumerable<TSource> src)
    //    {
    //        return src.Count();
    //    }

    //}

    //public static class RegQueryEx
    //{

    //    private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
    //    {
    //        return f.Method;
    //    }

    //    private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
    //    {
    //        return f.Method;
    //    }

    //    private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
    //    {
    //        return f.Method;
    //    }

    //    private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
    //    {
    //        return f.Method;
    //    }

    //    private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
    //    {
    //        return f.Method;
    //    }

    //    private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
    //    {
    //        return f.Method;
    //    }

    //    //public static TSource Update<TSource>(this IQueryable<TSource> source)
    //    //{
    //    //    var first = typeof(RegQueryEx).GetMethod("Update");
    //    //    var methdodcall = Expression.Call(first.MakeGenericMethod(typeof(TSource)), source.Expression);

    //    //    return source.Provider.Execute<TSource>(methdodcall);


    //    //    //return source.Provider.Execute<TSource>(
    //    //    //    Expression.Call(
    //    //    //        null,
    //    //    //        GetMethodInfo(RegQueryEx.FirstOrDefault1, source),
    //    //    //        new Expression[] { source.Expression }
    //    //    //        ));
    //    //}

    //    public static TSource FirstOrDefault11<TSource>(IQueryable<TSource> source)
    //    {
    //        return default(TSource);
    //    }

    //    public static IQueryable<T> Update<T>(this IQueryable<T> source, Expression<Func<T, T>> selector)
    //    {
    //        return source.Provider.CreateQuery<T>(
    //            Expression.Call(
    //                null,
    //                GetMethodInfo(RegQueryEx.Update, source, selector),
    //                new Expression[] { source.Expression, Expression.Quote(selector) }
    //                ));

    //    }

    //    public static int Update1<TSource>(this IQueryable<TSource> source)
    //    {
    //        return source.Provider.Execute<int>(
    //            Expression.Call(
    //                null,
    //                GetMethodInfo(Queryable.Count, source),
    //                new Expression[] { source.Expression }
    //                ));
    //    }

    //    public static int Update1<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
    //    {
    //        return source.Provider.Execute<int>(
    //            Expression.Call(
    //                null,
    //                GetMethodInfo(Queryable.Count, source, predicate),
    //                new Expression[] { source.Expression, Expression.Quote(predicate) }
    //                ));
    //    }

    //    //public static int Update1<TSource>(this IQueryable<TSource> source, Expression<Action<TSource>> predicate)
    //    //{
    //    //    return source.Provider.Execute<int>(
    //    //        Expression.Call(
    //    //            null,
    //    //            GetMethodInfo(RegQueryEx.Update1, source, predicate),
    //    //            new Expression[] { source.Expression, Expression.Quote(predicate) }
    //    //            ));
    //    //}
    //}
}
