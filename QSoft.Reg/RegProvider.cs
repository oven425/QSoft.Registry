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

        Expression m_RegMethod = null;
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            System.Diagnostics.Trace.WriteLine($"CreateQuery {(expression as MethodCallExpression).Method?.Name}");
            IQueryable<TElement> hr = default(IQueryable<TElement>);
            hr = new RegQuery<TElement>(this, expression, this.m_IsFirst, this.m_RegSource);
            var ttype = typeof(TElement);
            RegExpressionVisitor reg = new RegExpressionVisitor();

            MethodCallExpression method1 = expression as MethodCallExpression;
            if (method1.Arguments[0].NodeType == ExpressionType.Constant)
            {
                this.m_RegMethod = reg.Visit(expression, typeof(TElement), null, this.m_RegSource);
            }
            else
            {
                var expr = expression;
                bool bb = typeof(IQueryable<RegistryKey>) == this.m_RegMethod.Type;
                if (bb == false)
                {
                    var aaaa = this.m_RegMethod.Type.GetGenericArguments();
                    var bbbb = aaaa.ElementAt(0).GetGenericArguments();
                    bb = typeof(RegistryKey) == bbbb.Last();
                }
                if (bb == true)
                {
                    expr = reg.Visit(method1.Arguments[1], this.m_DataType, null, this.m_RegSource);
                    var methods = method1.Method.ReflectedType.GetMethods().Where(x => x.Name == method1.Method.Name);

                    methods = methods.Where(x => x.IsGenericMethod == method1.Method.IsGenericMethod);
                    methods = methods.Where(x => x.GetParameters().Length == method1.Method.GetParameters().Length);
                    var pps = method1.Method.GetGenericArguments();
                    if (pps[0] == this.m_DataType)
                    {
                        pps[0] = typeof(RegistryKey);
                    }
                    if (pps[0].Name.Contains("IGrouping"))
                    {
                        var gpps = pps[0].GetGenericArguments();
                        gpps.Replace(this.m_DataType, typeof(RegistryKey));

                        pps[0] = typeof(IGrouping<,>).MakeGenericType(gpps);
                        if (pps[1] == this.m_DataType)
                        {
                            pps[1] = typeof(RegistryKey);
                        }
                    }
                    if (pps[1].Name.Contains("IGrouping"))
                    {
                        var gpps = pps[1].GetGenericArguments();
                        gpps.Replace(this.m_DataType, typeof(RegistryKey));

                        pps[1] = typeof(IGrouping<,>).MakeGenericType(gpps);

                    }

                    var ooi = methods.ElementAt(0).MakeGenericMethod(pps);
                    var ppps = method1.Method.GetGenericMethodDefinition().GetParameters();
                    ooi = method1.Method.GetGenericMethodDefinition().MakeGenericMethod(pps);
                    if(this.m_RegMethod.Type != ooi.ReturnType)
                    {
                        this.m_RegMethod = Expression.Call(ooi, this.m_RegMethod, expr);
                    }
                    
                }
                else
                {
                    this.m_RegMethod = Expression.Call(method1.Method, this.m_RegMethod, method1.Arguments[1]);
                }
               
                expr = null;
            }
               






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
        //public int ToUpdate<T>(IEnumerable<T> datas)
        //{
        //    if (datas.Count() > 0 && this.m_Regs.Count() > 0)
        //    {
        //        var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true);
        //        Dictionary<string, object> values = new Dictionary<string, object>();
        //        foreach (var data in datas)
        //        {
        //            if(values.Count == 0)
        //            {
        //                foreach(var pp in pps)
        //                {
        //                    values[pp.Name] = pp.GetValue(data);
        //                }
        //            }
        //            foreach (var reg in this.m_Regs)
        //            {
        //                foreach(var value in values)
        //                {
        //                    reg.SetValue(value.Key, value.Value);
        //                }
        //            }
        //            values.Clear();
        //        }

        //    }
        //    return datas.Count();
        //}
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



            var updatemethod = this.m_RegMethod as MethodCallExpression;

            this.m_IsWritable = updatemethod?.Method?.Name == "Update";

            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            if (type.Name == "IEnumerable`1")
            {
               
                if (updatemethod.Type == typeof(IQueryable<RegistryKey>) || updatemethod.Type == typeof(IOrderedQueryable<RegistryKey>))
                {
                    var expr = expression;
                    var sd = this.ToSelectData();
                    var selects = typeof(Queryable).GetMethods().Where(x => x.Name == "Select");
                    var select = selects.ElementAt(0).MakeGenericMethod(typeof(RegistryKey), this.m_DataType);
                    updatemethod = Expression.Call(select, updatemethod, sd);

                }
                else if (updatemethod?.Method.Name == "GroupBy")
                {
                    var groupby = updatemethod.Type.GetGenericArguments()[0].GetGenericArguments();
                    if (groupby.Length == 2 && groupby[1] == typeof(RegistryKey))
                    {
                        var methods = updatemethod.Method.ReflectedType.GetMethods().Where(x => x.Name == updatemethod.Method.Name);
                        methods = methods.Where(x => x.GetGenericArguments().Length == 3);
                        Type[] type3 = new Type[3];
                        Array.Copy(updatemethod.Method.GetGenericArguments(), type3, 2);
                        if(type3[1] == typeof(RegistryKey))
                        {
                            type3[1] = this.m_DataType;
                        }
                        type3[2] = this.m_DataType;
                        var oo = methods.ElementAt(0).MakeGenericMethod(type3);
                        Expression arg2 = updatemethod.Arguments[1];
                        if(type3[1] == this.m_DataType)
                        {
                            arg2 = this.ToSelectData();
                        }
                        updatemethod = Expression.Call(oo, updatemethod.Arguments[0], arg2, this.ToSelectData());

                    }
                }

                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                //var tts1 = new Type[tts.Length];
                //for (int i = 0; i < tts.Length; i++)
                //{
                //    if (tts[i] == this.m_DataType)
                //    {
                //        tts1[i] = typeof(RegistryKey);
                //    }
                //    else if (tts[i].GenericTypeArguments.Length > 1)
                //    {
                //        if (tts[i].Name.Contains("IGrouping"))
                //        {
                //            tts1[i] = typeof(IGrouping<,>).MakeGenericType(tts[i].GenericTypeArguments[0], typeof(RegistryKey));

                //        }
                //        else if (tts[i].Name.Contains("AnonymousType"))
                //        {
                //            tts1[i] = tts[i];
                //        }
                //    }
                //    else
                //    {
                //        tts1[i] = tts[i];
                //    }
                //}
                //tts1[0] = tts[0];
                var creatquery = creatquerys.First().MakeGenericMethod(tts);
                var excute = creatquery.Invoke(tte.Provider, new object[] { updatemethod });
                return_hr = (TResult)excute;
            }
            else
            {
                object inst = null;
                RegExpressionVisitor regvisitor = new RegExpressionVisitor();
                Expression expr = expression;
                updatemethod = expr as MethodCallExpression;
                var opop = updatemethod.Arguments[0].Type;
                if(opop.Name.Contains("RegQuery`1") == true)
                {
                    expr = regvisitor.Visit(expression, this.m_DataType, null, this.m_RegSource);
                }

                object excute = null;


                excute = tte.Provider.Execute(expr);
                //var excute = tte.Provider.Execute<TResult>(expression);
                var excute_reg = excute as RegistryKey;
                var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                if (excute_reg != null)
                {
                    var pps = typeof(TResult).GetProperties().Where(x => x.CanWrite == true);
                    inst = Activator.CreateInstance(typeof(TResult));
                    foreach (var pp in pps)
                    {
                        var yyy = regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType).Invoke(excute_reg, new object[] { excute_reg, pp.Name });
                        pp.SetValue(inst, yyy);
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

        public Expression ToSelectData()
        {
            var selects = typeof(Queryable).GetMethods().Where(x => x.Name == "Select");
            var pp = Expression.Parameter(typeof(RegistryKey), "x");
            var todata = ToData(pp);
            var lambda = Expression.Lambda(todata, pp);
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            return unary;
        }

        public Expression ToData(ParameterExpression param)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
            var pps = m_DataType.GetProperties().Where(x => x.CanWrite == true);
            var ccs = m_DataType.GetConstructors();
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                Expression name = null;
                if (pp.PropertyType.Name.Contains("Nullable"))
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                    UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                    var binding = Expression.Bind(pp, unary1);
                    bindings.Add(binding);
                }
                else
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                    var binding = Expression.Bind(pp, method);
                    bindings.Add(binding);
                }
            }

            var memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);

            return memberinit;
        }
    }


}
