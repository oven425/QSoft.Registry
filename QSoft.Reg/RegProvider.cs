using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

//[assembly: InternalsVisibleTo("AssemblyB")]
namespace System.Linq
{
    public class RegProvider : IQueryProvider
    {
        RegistryKey m_Reg;
        Type m_DataType;
        public RegProvider(RegistryHive hive, string path, Type datatype)
        {
            this.m_DataType = datatype;
            this.m_Reg = hive.OpenView64(path);
        }

        public IQueryable CreateQuery(Expression expression)
        {

            throw new NotImplementedException();
        }

        Queue<(MethodInfo method, Expression expression)> CreateQuertys = new Queue<(MethodInfo method, Expression expression)>();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {         
            return new RegQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Enumerable<T>(IEnumerable<RegistryKey> query)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true);
            foreach (var oo in query)
            {
                var inst = Activator.CreateInstance(typeof(T));
                foreach (var pp in pps)
                {
                    pp.SetValue(inst, oo.GetValue(pp.Name));
                }
                yield return (T)inst;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            List<RegistryKey> regs = new List<RegistryKey>();
            var subkeynames = this.m_Reg.GetSubKeyNames();

            foreach (var subkeyname in subkeynames)
            {
                regs.Add(this.m_Reg.OpenSubKey(subkeyname));
            }
            var tte = regs.AsQueryable();
            
            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            if (expression is ConstantExpression)
            {
                if (tts[0] == this.m_DataType)
                {
                    var mi = typeof(RegProvider).GetMethod("Enumerable");
                    var fooRef = mi.MakeGenericMethod(tts[0]);
                    return (TResult)fooRef.Invoke(this, new object[] { tte });
                }
            }



            RegExpressionVisitor regvisitor = new RegExpressionVisitor();

            var expr = regvisitor.Visit(expression, this.m_DataType, tte);

            var methodcall_param_0 = Expression.Constant(tte);

            TResult return_hr;
            if (type.Name == "IEnumerable`1")
            {
                //var pppo = tte.Provider.CreateQuery(expr);
                //foreach(var oo in pppo)
                //{

                //}
                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x=>x.Name== "CreateQuery" && x.IsGenericMethod==true);
                var tts1 = new Type[tts.Length];
                for(int i=0; i<tts.Length; i++)
                {
                    if(tts[i]==this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else if(tts[i].GenericTypeArguments.Length > 1)
                    {
                        if(tts[i].Name.Contains("IGrouping"))
                        {
                            tts1[i] = typeof(IGrouping<,>).MakeGenericType(typeof(string), typeof(RegistryKey));

                        }
                        else if(tts[i].Name.Contains("AnonymousType"))
                        {
                            tts1[i] = tts[i];
                        }
                    }
                    else
                    {
                        tts1[i] = tts[i];
                    }
                }
                tts1[0] = tts[0];
                var creatquery = creatquerys.First().MakeGenericMethod(tts1);
                var excute = creatquery.Invoke(tte.Provider, new object[] { expr });

                if(tts[0].Name.Contains("IGrouping"))
                {
                    var groupby = excute as IEnumerable<IGrouping<string, RegistryKey>>;
                    foreach(var group in groupby)
                    {
                        var mi = typeof(RegProvider).GetMethod("Enumerable");
                        var fooRef = mi.MakeGenericMethod(this.m_DataType);
                        var ooo = group as IEnumerable<RegistryKey>;
                        return (TResult)fooRef.Invoke(this, new object[] { ooo });
                    }
                    
                }
                //else if(tts[0] == this.m_DataType)
                //{
                //    var mi = typeof(RegProvider).GetMethod("Enumerable");
                //    var fooRef = mi.MakeGenericMethod(tts[0]);
                //    return (TResult)fooRef.Invoke(this, new object[] { excute });
                //}
                return_hr = (TResult)excute;
            }
            else
            {
                object inst = null;
                
                var excute = tte.Provider.Execute(expr);
                var excute_reg = excute as RegistryKey;
                if (excute_reg !=null)
                {
                    var pps = typeof(TResult).GetProperties().Where(x => x.CanWrite == true);
                    inst = Activator.CreateInstance(typeof(TResult));
                    foreach (var pp in pps)
                    {
                        pp.SetValue(inst, excute_reg.GetValue(pp.Name));
                    }

                }
                else
                {
                    inst = excute;
                }

                foreach(var oo in regs)
                {
                    oo.Close();
                    oo.Dispose();
                }
                //this.m_Reg.Close();
                //this.m_Reg.Dispose();
                return_hr = (TResult)inst;
            }

            return return_hr;
        }
    }

    //class GroupedEnumerable<TSource, TKey, TElement, TResult> : IEnumerable<TResult>
    //{
    //    IEnumerable<TSource> source;
    //    Func<TSource, TKey> keySelector;
    //    Func<TSource, TElement> elementSelector;
    //    IEqualityComparer<TKey> comparer;
    //    Func<TKey, IEnumerable<TElement>, TResult> resultSelector;

    //    public GroupedEnumerable(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
    //    {
    //        this.source = source;
    //        this.keySelector = keySelector;
    //        this.elementSelector = elementSelector;
    //        this.comparer = comparer;
    //        this.resultSelector = resultSelector;
    //    }

    //    public IEnumerator<TResult> GetEnumerator()
    //    {
    //        Lookup<TKey, TElement> lookup = Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
    //        return lookup.ApplyResultSelector(resultSelector).GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}


}
