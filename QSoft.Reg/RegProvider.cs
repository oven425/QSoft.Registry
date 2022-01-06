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
    public class RegProvider<TData> : IQueryProvider
    {
        public RegSetting Setting { set; get; } = new RegSetting();

        public MethodCallExpression m_RegSource;
        public RegProvider()
        {
            var method = typeof(RegProvider<TData>).GetMethod("CreateRegs");
            this.m_RegSource = Expression.Call(Expression.Constant(this), method);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        Expression m_RegMethod = null;
        List<Tuple<Expression, Expression, string>> m_Errors = new List<Tuple<Expression, Expression, string>>();
        Dictionary<Expression, Expression> m_Exprs = new Dictionary<Expression, Expression>();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            System.Diagnostics.Debug.WriteLine($"CreateQuery {(expression as MethodCallExpression).Method?.Name}");

            IQueryable<TElement> hr = default(IQueryable<TElement>);
            hr = new RegQuery<TElement>(this, expression);

            RegExpressionVisitor<TData> reg = new RegExpressionVisitor<TData>();
            MethodCallExpression method1 = expression as MethodCallExpression;
            
            if (method1.Arguments[0].NodeType == ExpressionType.Constant)
            {
                this.m_Exprs.Clear();
            }
            //this.m_RegMethod = reg.VisitA(expression, this.m_RegSource, this.m_Exprs);
            //this.m_Exprs[expression] = this.m_RegMethod;

            if (this.m_RegMethod == null || method1.Arguments[0].NodeType == ExpressionType.Constant)
            {
                this.m_RegMethod = reg.VisitA(expression, this.m_RegSource, this.m_Exprs);
                this.m_Exprs[expression] = this.m_RegMethod;
            }
            else if (this.m_RegMethod.Type.IsGenericType == true)
            {
                var ttype = this.m_RegMethod.Type;
                var ttype_def = ttype.GetGenericTypeDefinition();
                if (ttype == typeof(IQueryable<RegistryKey>) || ttype == typeof(IEnumerable<RegistryKey>) || ttype == typeof(IOrderedQueryable<RegistryKey>))
                {
                    this.m_RegMethod = reg.VisitA(expression, this.m_RegSource, this.m_Exprs);
                    this.m_Exprs[expression] = this.m_RegMethod;
                }
                else if (ttype_def == typeof(IQueryable<>) || ttype_def == typeof(IEnumerable<>) || ttype_def == typeof(IOrderedQueryable<>))
                {
                    var group = ttype.GetGenericArguments()[0];
                    
                    bool has = group.GetGenericArguments().Any(x => x == typeof(RegistryKey));
                    has = group.HaseRegistryKey();
                    if (has == true)
                    {
                        this.m_RegMethod = reg.VisitA(expression, this.m_RegSource, this.m_Exprs);
                        this.m_Exprs[expression] = this.m_RegMethod;
                    }
                    else
                    {
                        var args = method1.Arguments.ToArray();
                        args[0] = this.m_RegMethod;
                        this.m_RegMethod = Expression.Call(method1.Method, args);
                    }
                }
                else
                {
                    var args = method1.Arguments.ToArray();
                    args[0] = this.m_RegMethod;
                    this.m_RegMethod = Expression.Call(method1.Method, args);
                }
            }
            else
            {
                var args = method1.Arguments.ToArray();
                args[0] = this.m_RegMethod;
                this.m_RegMethod = Expression.Call(method1.Method, args);
            }




            //var ttype = typeof(TElement);
            //RegExpressionVisitor<TData> reg = new RegExpressionVisitor<TData>();

            //MethodCallExpression method1 = expression as MethodCallExpression;
            //if (method1.Arguments[0].NodeType == ExpressionType.Constant)
            //{
            //    this.m_Errors.Clear();
            //    this.m_RegMethod = reg.Visit(expression, this.m_RegSource);
            //    if (reg.Fail != null)
            //    {
            //        this.m_Errors.Add(Tuple.Create(expression, this.m_RegMethod, reg.Fail));
            //    }
            //}
            //else
            //{
            //    var expr = expression;
            //    bool bb = typeof(IQueryable<RegistryKey>) == this.m_RegMethod.Type;
            //    if(bb == false)
            //    {
            //        bb = typeof(IOrderedQueryable<RegistryKey>) == this.m_RegMethod.Type;
            //    }
            //    if (bb == false)
            //    {
            //        var aaaa = this.m_RegMethod.Type.GetGenericArguments();
            //        var bbbb = aaaa.ElementAt(0).GetGenericArguments();
            //        if(bbbb.Length>0)
            //        {
            //            bb = typeof(RegistryKey) == bbbb.Last();
            //        }

            //    }
            //    if (bb == true)
            //    {
            //        expr = reg.Visit(method1.Method, method1.Arguments[0], method1.Arguments[1]);
            //        if (reg.Fail != null)
            //        {
            //            this.m_Errors.Add(Tuple.Create(expression, expr, reg.Fail));
            //        }
            //        var methods = method1.Method.ReflectedType.GetMethods().Where(x => x.Name == method1.Method.Name);

            //        methods = methods.Where(x => x.IsGenericMethod == method1.Method.IsGenericMethod);
            //        methods = methods.Where(x => x.GetParameters().Length == method1.Method.GetParameters().Length);
            //        var pps = method1.Method.GetGenericArguments();
            //        if (pps[0] == typeof(TData))
            //        {
            //            pps[0] = typeof(RegistryKey);
            //            pps[pps.Length-1] = reg.ReturnType;
            //        }
            //        if (pps[0].Name.Contains("IGrouping"))
            //        {
            //            var gpps = pps[0].GetGenericArguments();
            //            gpps.Replace(typeof(TData), typeof(RegistryKey));

            //            pps[0] = typeof(IGrouping<,>).MakeGenericType(gpps);
            //            if (pps[1] == typeof(TData))
            //            {
            //                pps[1] = typeof(RegistryKey);
            //            }
            //        }
            //        if (pps.Length>1&&pps[1].Name.Contains("IGrouping"))
            //        {
            //            var gpps = pps[1].GetGenericArguments();
            //            gpps.Replace(typeof(TData), typeof(RegistryKey));

            //            pps[1] = typeof(IGrouping<,>).MakeGenericType(gpps);

            //        }

            //        var ooi = methods.ElementAt(0).MakeGenericMethod(pps);
            //        var ppps = method1.Method.GetGenericMethodDefinition().GetParameters();
            //        ooi = method1.Method.GetGenericMethodDefinition().MakeGenericMethod(pps);
            //        //if(this.m_RegMethod.Type != ooi.ReturnType)
            //        {
            //            this.m_RegMethod = Expression.Call(ooi, this.m_RegMethod, expr);
            //        }

            //    }
            //    else
            //    {
            //        this.m_RegMethod = Expression.Call(method1.Method, this.m_RegMethod, method1.Arguments[1]);
            //    }

            //    expr = null;
            //}

            return hr;
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

        bool m_IsWritable = false;


        List<RegistryKey> m_Regs = new List<RegistryKey>();
        public IQueryable<RegistryKey> CreateRegs()
        {
            if (this.m_Regs.Count == 0)
            {
                RegistryKey reg = this.Setting.Create();
                var subkeynames = reg.GetSubKeyNames();
                foreach (var subkeyname in subkeynames)
                {
                    this.m_Regs.Add(reg.OpenSubKey(subkeyname, m_IsWritable));
                }
            }
            return m_Regs.AsQueryable();
        }


        public TResult Execute<TResult>(Expression expression)
        {
            var tte = new List<RegistryKey>().AsQueryable();
            TResult return_hr = default(TResult);


            var expr_org = expression as MethodCallExpression;
            var updatemethod = this.m_RegMethod as MethodCallExpression;
            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            string methodname = (expression as MethodCallExpression)?.Method?.Name;

            if (expression is ConstantExpression && tts[0] == typeof(TData))
            {
                var expr = expression;
                var sd = typeof(TData).ToSelectData();
                var select = typeof(TData).SelectMethod();
                updatemethod = Expression.Call(select, this.m_RegSource, sd);
                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                var creatquery = creatquerys.First().MakeGenericMethod(tts);
                var excute = creatquery.Invoke(tte.Provider, new object[] { updatemethod });
                return (TResult)excute;
            }

            if (type.IsGenericType == true&&type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                if (updatemethod.Type == typeof(IQueryable<RegistryKey>) || updatemethod.Type == typeof(IOrderedQueryable<RegistryKey>))
                {
                    var sd = typeof(TData).ToSelectData();
                    var select = typeof(TData).SelectMethod();
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
                            type3[1] = typeof(TData);
                        }
                        type3[2] = typeof(TData);
                        var oo = methods.ElementAt(0).MakeGenericMethod(type3);
                        Expression arg2 = updatemethod.Arguments[1];
                        if(type3[1] == typeof(TData))
                        {
                            arg2 = typeof(TData).ToSelectData();
                        }
                        updatemethod = Expression.Call(oo, updatemethod.Arguments[0], arg2, typeof(TData).ToSelectData());

                    }
                }
                //else if (updatemethod?.Method.Name == "Join")
                //{
                //    var pps = updatemethod.Method.GetParameters();
                //    var join = updatemethod.Type.GetGenericArguments()[0].GetGenericArguments();
                    
                //}
                else
                {
                    var ttype1 = updatemethod.Type;
                    if(ttype1.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        var ty = ttype1.GetGenericArguments()[0].GetProperties();
                    }
                    var sd = type.GetGenericArguments()[0].ToSelectData(null, updatemethod.Type.GetGenericArguments()[0]);
                    if(sd != null)
                    {
                        var select = typeof(TResult).GetGenericArguments()[0].SelectMethod(updatemethod.Type.GetGenericArguments()[0]);
                        updatemethod = Expression.Call(select, updatemethod, sd);
                    }
                    

                    //var args = exprs1.Select(x => x.Value).ToList();
                    //args[0] = selectexpr;
                    //methodcall = Expression.Call(expr.Method, args);
                }

                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                var creatquery = creatquerys.First().MakeGenericMethod(tts);
                var excute = creatquery.Invoke(tte.Provider, new object[] { updatemethod });
                return_hr = (TResult)excute;
            }
            else
            {
                
                System.Diagnostics.Debug.WriteLine($"Execute<TResult> {methodname}");
               
                object inst = null;
                Expression expr = expression;

                if (methodname == "Insert" || methodname == "RemoveAll"|| methodname=="Update")
                {
                    if(this.m_IsWritable == false)
                    {
                        foreach (var oo in this.m_Regs)
                        {
                            oo.Close();
                            oo.Dispose();
                        }
                        this.m_Regs.Clear();
                    }
                }
                this.m_IsWritable = methodname == "Update" || methodname == "Insert" || methodname == "RemoveAll";
                if (methodname == "Insert")
                {

                }
                else if (expr_org.Arguments[0].Type.IsGenericType==true&&expr_org.Arguments[0].Type.GetGenericTypeDefinition() == typeof(RegQuery<>))
                {
                    RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                    expr = regvisitor.VisitA(expr_org, this.m_RegSource, this.m_Exprs);
                    if (regvisitor.Fail != null)
                    {
                        this.m_Errors.Add(Tuple.Create(expression, expr, regvisitor.Fail));
                    }
                }
                else
                {
                    Expression arg1 = null;
                    updatemethod = expr as MethodCallExpression;
                    List<Expression> args = new List<Expression>();
                    args.Add(this.m_RegMethod);
                    for (int i=1; i< updatemethod.Arguments.Count; i++)
                    {
                        RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                        arg1 = regvisitor.Visit(updatemethod.Method, updatemethod.Arguments[i-1], updatemethod.Arguments[i]);
                        if (regvisitor.Fail != null)
                        {
                            this.m_Errors.Add(Tuple.Create(expression, arg1, regvisitor.Fail));
                        }
                        args.Add(arg1);
                    }

                    var ggs = updatemethod.Method.GetGenericArguments();
                    if(ggs[0] == typeof(TData))
                    {
                        ggs[0] = typeof(RegistryKey);
                    }
                    
                    var mmethod = updatemethod.Method.GetGenericMethodDefinition().MakeGenericMethod(ggs);
                    expr = MethodCallExpression.Call(mmethod, args);

                }
                
                var fail = this.CheckFail();
                if (fail != null)
                {
                    throw fail;
                }
                object excute = null;
                excute = tte.Provider.Execute(expr);

                var excute_reg = excute as RegistryKey;
                if (excute_reg != null)
                {
                    inst = excute_reg.ToDataFunc<TResult>()(excute_reg);
                }
                else
                {
                    inst = excute;
                }

                return_hr = (TResult)inst;
            }

            if (methodname == "Insert" || methodname == "RemoveAll")
            {
                foreach (var oo in this.m_Regs)
                {
                    oo.Close();
                    oo.Dispose();
                }
                this.m_Regs.Clear();
            }

            return return_hr;
        }

        public Exception CheckFail()
        {
            Exception excpt = null;
            var first = this.m_Errors.FirstOrDefault();
            if (first != null)
            {
                excpt = new Exception(first.Item3);
            }
            return excpt;
        }
    }
}
