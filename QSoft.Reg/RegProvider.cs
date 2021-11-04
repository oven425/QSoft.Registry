﻿using Microsoft.Win32;
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

namespace QSoft.Registry.Linq
{
    public class RegProvider : IQueryProvider
    {
        public RegSetting Setting { set; get; } = new RegSetting();
        Type m_DataType;

        MethodCallExpression m_RegSource;
        public RegProvider(Type datatype)
        {
            var method = typeof(RegProvider).GetMethod("CreateRegs");
            this.m_RegSource = Expression.Call(Expression.Constant(this), method);
            this.m_DataType = datatype;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            
            throw new NotImplementedException();
        }

        bool m_IsFirst = true;
        
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            System.Diagnostics.Trace.WriteLine($"CreateQuery {(expression as MethodCallExpression).Method?.Name}");
            IQueryable <TElement> hr = default(IQueryable<TElement>);
            hr = new RegQuery<TElement>(this, expression, this.m_IsFirst, this.m_RegSource);
            this.m_IsFirst = false;
            return hr;
            //return new RegQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Enumerable<T>(IQueryable<RegistryKey> query)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true);
            foreach (var oo in query)
            {
                var inst = Activator.CreateInstance(typeof(T));
                foreach (var pp in pps)
                {
                    pp.SetValue(inst, oo.GetValue(pp.Name), null);
                }
                yield return (T)inst;
            }

        }
#if CreateQuery
        public int ToUpdate<T>(IEnumerable<T> datas)
        {
            if (datas.Count() > 0 && this.m_Regs.Count() > 0)
            {
                var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true);
                Dictionary<string, object> values = new Dictionary<string, object>();
                foreach (var data in datas)
                {
                    if(values.Count == 0)
                    {
                        foreach(var pp in pps)
                        {
                            values[pp.Name] = pp.GetValue(data);
                        }
                    }
                    foreach (var reg in this.m_Regs)
                    {
                        foreach(var value in values)
                        {
                            reg.SetValue(value.Key, value.Value);
                        }
                    }
                    values.Clear();
                }
                
            }
            return datas.Count();
        }
#endif

        bool m_IsWritable = false;
        IQueryable<RegistryKey> m_Regs;
        public IQueryable<RegistryKey> CreateRegs()
        {
            if(this.m_Regs?.Count() > 0)
            {
                foreach (var oo in this.m_Regs)
                {
                    oo.Close();
                    oo.Dispose();
                }
            }
            
            
            List<RegistryKey> regs = new List<RegistryKey>();
            RegistryKey reg = this.Setting;
            var subkeynames = reg.GetSubKeyNames();
            foreach (var subkeyname in subkeynames)
            {
                regs.Add(reg.OpenSubKey(subkeyname, m_IsWritable));
            }
            return m_Regs = regs.AsQueryable();
        }

        public TResult Execute<TResult>(Expression expression)
        {
#if CreateQuery


            var tte = new List<RegistryKey>().AsQueryable();
            TResult return_hr = default(TResult);
            //List<RegistryKey> regs = new List<RegistryKey>();
            //RegistryKey reg = this.Setting;
            //var subkeynames = reg.GetSubKeyNames();

            var updatemethod = expression as MethodCallExpression;
            this.m_IsWritable = updatemethod?.Method?.Name == "Update";
            //foreach (var subkeyname in subkeynames)
            //{
            //    regs.Add(reg.OpenSubKey(subkeyname, updatemethod?.Method.Name == "Update"));
            //}
            //var tte = regs.AsQueryable();
            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            if (type.Name == "IEnumerable`1")
            {
                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                var tts1 = new Type[tts.Length];
                for (int i = 0; i < tts.Length; i++)
                {
                    if (tts[i] == this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else if (tts[i].GenericTypeArguments.Length > 1)
                    {
                        if (tts[i].Name.Contains("IGrouping"))
                        {
                            tts1[i] = typeof(IGrouping<,>).MakeGenericType(tts[i].GenericTypeArguments[0], typeof(RegistryKey));

                        }
                        else if (tts[i].Name.Contains("AnonymousType"))
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
                var excute = creatquery.Invoke(tte.Provider, new object[] { expression });

                return_hr = (TResult)excute;
            }
            else
            {
                object inst = null;
                RegExpressionVisitor regvisitor = new RegExpressionVisitor();
                //var expr = regvisitor.Visit(expression, this.m_DataType, null, this.m_RegSource);
                TResult excute = default(TResult);
                //if (this.m_IsWritable == true)
                //{
                //    var yutu = updatemethod.Arguments[0] as MethodCallExpression;
                //    var gg = (yutu.Arguments[0] as MethodCallExpression).Method.ReturnType.GetGenericArguments();
                //    var toupdate = this.GetType().GetMethod("ToUpdate");
                //    var update_expr = MethodCallExpression.Call(Expression.Constant(this), toupdate.MakeGenericMethod(gg), yutu.Arguments[0]);

                //    excute = tte.Provider.Execute<TResult>(update_expr);
                //}
                //else
                {
                    excute = tte.Provider.Execute<TResult>(expression);
                }
                //var excute = tte.Provider.Execute<TResult>(expression);
                var excute_reg = excute as RegistryKey;
                if (excute_reg != null)
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
                if(this.m_Regs != null)
                {
                    foreach (var oo in this.m_Regs)
                    {
                        oo.Close();
                        oo.Dispose();
                    }
                }
                
                
                return_hr = (TResult)inst;
            }

            return return_hr;
#else
            List<RegistryKey> regs = new List<RegistryKey>();
            RegistryKey reg = this.Setting;
            var subkeynames = reg.GetSubKeyNames();

            var updatemethod = expression as MethodCallExpression;
            foreach (var subkeyname in subkeynames)
            {
                regs.Add(reg.OpenSubKey(subkeyname, updatemethod?.Method.Name=="Update"));
            }
            var tte = regs.AsQueryable();
            
            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            if (expression is ConstantExpression && tts[0] == this.m_DataType)
            {
                var mi = typeof(RegProvider).GetMethod("Enumerable");
                var fooRef = mi.MakeGenericMethod(tts[0]);
                return (TResult)fooRef.Invoke(this, new object[] { tte });
            }



            RegExpressionVisitor regvisitor = new RegExpressionVisitor();
            var methods = typeof(RegProvider).GetMethods();
            var method = typeof(RegProvider).GetMethod("CreateRegs");
            var createregs = Expression.Call(Expression.Constant(this), method);
            var expr = regvisitor.Visit(expression, this.m_DataType, reg, tte, createregs);

            var methodcall_param_0 = Expression.Constant(tte);

            TResult return_hr;
            if (type.Name == "IEnumerable`1")
            {
                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x=>x.Name== "CreateQuery" && x.IsGenericMethod==true);
                var tts1 = new Type[tts.Length];
                for(int i=0; i<tts.Length; i++)
                {
                    if(tts[i]==this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else if(tts[i].GetGenericArguments().Length > 1)
                    {
                        if(tts[i].Name.Contains("IGrouping"))
                        {
                            tts1[i] = typeof(IGrouping<,>).MakeGenericType(tts[i].GetGenericArguments()[0], typeof(RegistryKey));

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
                        pp.SetValue(inst, excute_reg.GetValue(pp.Name),null);
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

                return_hr = (TResult)inst;
            }

            return return_hr;
#endif
        }
    }
}
