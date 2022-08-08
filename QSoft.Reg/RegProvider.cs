//#define TestProvider
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace QSoft.Registry.Linq
{
    public class RegProvider<TData> : IQueryProvider
    {
        public List<RegQueryConvert> Converts { set; get; }
        public Action<TData> DefaultValue {internal set; get; }
        public RegSetting Setting { internal set; get; } = new RegSetting();
        //int CountName;
        public MethodCallExpression m_RegSource;
        public RegProvider()
        {

            var method = typeof(RegProvider<TData>).GetMethod("CreateRegs");
            this.m_RegSource = Expression.Call(Expression.Constant(this), method);
        }
#if TestProvider
        //public Expression Expression_Src { private set; get; }
        //public Expression Expression_Dst { private set; get; }
        public RegProvider(Expression src, Expression dst)
        {
            //this.Expression_Src = src;
            //this.Expression_Dst = dst;
            this.m_Exprs[src] = dst;
            this.m_RegMethod = dst;
        }
#endif
        
        Expression m_RegMethod = null;
        List<Tuple<Expression, Expression, string>> m_Errors = new List<Tuple<Expression, Expression, string>>();
        Dictionary<Expression, Expression> m_Exprs = new Dictionary<Expression, Expression>();
        HashSet<Expression> m_CreateQuerys = new HashSet<Expression>();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {

            var type1 = typeof(TElement);
            var type2 = typeof(TData);
#if TestProvider
            RegProvider<TElement> provid = null;
            RegExpressionVisitor<TData> reg = new RegExpressionVisitor<TData>();
            MethodCallExpression method1 = expression as MethodCallExpression;

            //this.m_RegMethod = expression;
            if (method1.Arguments[0].NodeType == ExpressionType.Constant)
            {
                this.m_Exprs.Clear();
                this.m_RegMethod = reg.VisitA(expression, this.m_RegSource, this.m_Exprs);
                provid = new RegProvider<TElement>(expression, this.m_RegMethod);
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
            //var provid = new RegProvider<TElement>(expression, this.m_RegMethod);
            if(provid == null)
            {
                return new RegQuery<TElement>(this, expression);
            }
            return new RegQuery<TElement>(provid, expression);
#else
            this.m_CreateQuerys.Add(expression);
            return new RegQuery<TElement>(this, expression);

            
#endif
        }



        Expression ProcessExpr(Expression excuteexpr)
        {
            Stack<Expression> exprs = new Stack<Expression>();
            if(this.m_CreateQuerys.Contains(excuteexpr) == true)
            {
                exprs.Push(excuteexpr);
                this.m_CreateQuerys.Remove(excuteexpr);
            }
            MethodCallExpression methodcall = excuteexpr as MethodCallExpression;
            while (true)
            {
                if (methodcall == null)
                {
                    break;
                }
                else
                {
                    var ll = methodcall.Arguments.Where(x => this.m_CreateQuerys.Contains(x) == true);
                    int findcount = ll.Count();
                    if(findcount > 1)
                    {
                        System.Diagnostics.Debug.WriteLine("");
                    }
                    else if(findcount == 1)
                    {
                        exprs.Push(ll.ElementAt(0));
                        
                        methodcall = ll.ElementAt(0) as MethodCallExpression;
                        this.m_CreateQuerys.Remove(ll.ElementAt(0));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            this.m_Exprs.Clear();

            foreach (var expression in exprs)
            {
                RegExpressionVisitor<TData> reg = new RegExpressionVisitor<TData>();
                reg.Converts = this.Converts;
                MethodCallExpression method1 = expression as MethodCallExpression;


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
            }
            //this.m_ProcessExprs[excuteexpr] = this.m_RegMethod;
            return this.m_RegMethod;
        }

        Dictionary<Expression, Expression> m_ProcessExprs = new Dictionary<Expression, Expression>();
        IQueryable<RegistryKey> m_RegsQuery;
        MethodInfo m_CreateQuery = typeof(IQueryProvider).GetMethods().FirstOrDefault(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
        public TResult Execute<TResult>(Expression expression)
        {
            var type1 = typeof(TData);
            var type2 = typeof(TResult);
            this.m_RegsQuery = this.m_RegsQuery ?? new List<RegistryKey>().AsQueryable();
            TResult return_hr = default(TResult);
            var type = typeof(TResult);
            var expr_org = expression as MethodCallExpression;
            string methodname = (expression as MethodCallExpression)?.Method?.Name;
            Type[] tts = type.GetGenericArguments();
#if TestProvider
            //var expr_org = expression as MethodCallExpression;
            //this.m_RegMethod = this.Expression_Dst;
            var updatemethod = this.m_RegMethod as MethodCallExpression;

            if (expression is ConstantExpression && tts[0] == typeof(TData))
            {
                var expr = expression;
                var sd = typeof(TData).ToSelectData();
                var select = typeof(TData).SelectMethod();
                this.m_RegMethod = Expression.Call(select, this.m_RegSource, sd);
                this.m_ProcessExprs[expression] = this.m_RegMethod;
                //var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                var creatquery = this.m_CreateQuery.MakeGenericMethod(tts);
                var excute = creatquery.Invoke(this.m_RegsQuery.Provider, new object[] { this.m_RegMethod });
                return (TResult)excute;
            }

            if (type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                if (updatemethod.Type == typeof(IQueryable<RegistryKey>) || updatemethod.Type == typeof(IOrderedQueryable<RegistryKey>))
                {
                    var t = typeof(TResult).GetGenericArguments()[0];
                    var sd = t.ToSelectData();
                    var select = t.SelectMethod();
                    updatemethod = Expression.Call(select, updatemethod, sd);
                }
                else if (updatemethod.Type.GetGenericTypeDefinition() == typeof(IQueryable<>) && updatemethod.Type.GetGenericArguments()[0].IsGenericType == true && updatemethod.Type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    //var oiou = updatemethod.Type.GetGenericTypeDefinition() == typeof(IQueryable<>);
                    //var popo = updatemethod.Type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IGrouping<,>);
                    var groupby = updatemethod.Type.GetGenericArguments()[0].GetGenericArguments();
                    if (groupby.Length == 2 && groupby[1] == typeof(RegistryKey))
                    {
                        var methods = updatemethod.Method.ReflectedType.GetMethods().Where(x => x.Name == updatemethod.Method.Name);
                        methods = methods.Where(x => x.GetGenericArguments().Length == 3);
                        Type[] type3 = new Type[3];
                        var vvv = updatemethod.Method.GetGenericArguments();
                        Array.Copy(updatemethod.Method.GetGenericArguments(), type3, 2);
                        Expression arg2 = updatemethod.Arguments[1];
                        if (type3[1] == typeof(RegistryKey))
                        {
                            type3[1] = typeof(TData).GetGenericArguments()[1];
                            arg2 = type3[1].ToSelectData();
                        }

                        type3[2] = typeof(TData).GetGenericArguments()[1];
                        var oo = methods.ElementAt(0).MakeGenericMethod(type3);
                        //Expression arg2 = updatemethod.Arguments[1];
                        //if (type3[1] == typeof(TData))
                        //{
                        //    arg2 = typeof(TData).ToSelectData();
                        //}
                        updatemethod = Expression.Call(oo, updatemethod.Arguments[0], arg2, type3[2].ToSelectData());

                    }
                }
                else
                {
                    var ttype1 = updatemethod.Type;
                    if (ttype1.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        var ty = ttype1.GetGenericArguments()[0].GetProperties();
                    }
                    var sd = type.GetGenericArguments()[0].ToSelectData(null, updatemethod.Type.GetGenericArguments()[0]);
                    if (sd != null)
                    {
                        var select = typeof(TResult).GetGenericArguments()[0].SelectMethod(updatemethod.Type.GetGenericArguments()[0]);
                        updatemethod = Expression.Call(select, updatemethod, sd);
                    }
                }

                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                var creatquery = creatquerys.First().MakeGenericMethod(tts);
                this.m_RegMethod = updatemethod;
                var excute = creatquery.Invoke(this.m_RegsQuery.Provider, new object[] { updatemethod });
                return_hr = (TResult)excute;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Execute<TResult> {methodname}");

                object inst = null;
                Expression expr = expression;

                switch (methodname)
                {
                    case "Insert":
                    case "InsertTo":
                    case "RemoveAll":
                    case "Update":
                        {
                            if (this.m_IsWritable == false)
                            {
                                foreach (var oo in this.m_Regs)
                                {
                                    oo.Close();
                                    oo.Dispose();
                                }
                                this.m_Regs.Clear();
                            }
                            this.m_IsWritable = true;
                        }
                        break;
                    default:
                        {
                            this.m_IsWritable = false;
                        }
                        break;
                }

                if (methodname == "Insert" || methodname == "InsertTo")
                {

                }
                else if (expr_org.Arguments[0].Type.IsGenericType == true && expr_org.Arguments[0].Type.GetGenericTypeDefinition() == typeof(RegQuery<>))
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
                    //List<Expression> args = new List<Expression>();
                    //saves[updatemethod.Arguments[0]] = this.m_RegMethod;
                    //args.Add(this.m_RegMethod);

                    //for (int i = 1; i < updatemethod.Arguments.Count; i++)
                    //{
                    //    RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                    //    arg1 = regvisitor.VisitA(updatemethod, this.m_RegSource, saves);

                    //    //arg1 = regvisitor.Visit(updatemethod.Method, updatemethod.Arguments[i-1], updatemethod.Arguments[i]);
                    //    if (regvisitor.Fail != null)
                    //    {
                    //        this.m_Errors.Add(Tuple.Create(expression, arg1, regvisitor.Fail));
                    //    }
                    //    args.Add(arg1);
                    //    saves[updatemethod.Arguments[i]] = arg1;
                    //}

                    //var oioi = args.GetTypes(updatemethod.Method);

                    //expr = arg1;


                    Expression arg1 = null;
                    updatemethod = expr as MethodCallExpression;
                    if (updatemethod.Arguments.Count == 1)
                    {
                        var ggs = updatemethod.Method.GetGenericArguments();
                        //var ggs = (this.m_RegMethod as MethodCallExpression)?.Method.GetGenericArguments();
                        if (ggs[0] == typeof(TData))
                        {
                            //ggs[0] = typeof(RegistryKey);
                        }
                        var mmethod = updatemethod.Method.GetGenericMethodDefinition().MakeGenericMethod(this.m_RegMethod.Type.GetGenericArguments()[0]);
                        expr = MethodCallExpression.Call(mmethod, this.m_RegMethod);
                    }
                    else
                    {
                        RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                        Dictionary<Expression, Expression> saves = new Dictionary<Expression, Expression>();
                        saves[updatemethod.Arguments[0]] = this.m_RegMethod;
                        expr = regvisitor.VisitA(updatemethod, this.m_RegSource, saves);
                    }
                    //List<Expression> args = new List<Expression>();
                    //args.Add(this.m_RegMethod);
                    //for (int i=1; i< updatemethod.Arguments.Count; i++)
                    //{
                    //    RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                    //    arg1 = regvisitor.Visit(updatemethod.Method, updatemethod.Arguments[i-1], updatemethod.Arguments[i]);
                    //    if (regvisitor.Fail != null)
                    //    {
                    //        this.m_Errors.Add(Tuple.Create(expression, arg1, regvisitor.Fail));
                    //    }
                    //    args.Add(arg1);
                    //}

                    //var ggs = updatemethod.Method.GetGenericArguments();
                    //if(ggs[0] == typeof(TData))
                    //{
                    //    ggs[0] = typeof(RegistryKey);
                    //}
                    //var oioi = args.GetTypes(updatemethod.Method);
                    //var mmethod = updatemethod.Method.GetGenericMethodDefinition().MakeGenericMethod(oioi);
                    //expr = MethodCallExpression.Call(mmethod, args);

                }

                var fail = this.CheckFail();
                if (fail != null)
                {
                    throw fail;
                }
                object excute = null;
                this.m_RegMethod = expr;
                excute = this.m_RegsQuery.Provider.Execute(expr);

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
#else
            if (expression is ConstantExpression && tts[0] == typeof(TData) && this.m_ProcessExprs.ContainsKey(expression) == false)
            {
                //var sd = typeof(TData).ToSelectData();
                //var select = typeof(TData).SelectMethod();
                //var updatemethod1 = Expression.Call(select, this.m_RegSource, sd);
                //this.m_ProcessExprs[expression] = updatemethod1;

                this.m_ProcessExprs[expression] = this.BuildSelect(this.m_RegSource);

                ////var creatquerys = typeof(IQueryProvider).GetMethods().Where(x => x.Name == "CreateQuery" && x.IsGenericMethod == true);
                //var creatquery = this.m_CreateQuery.MakeGenericMethod(tts);
                //var excute = creatquery.Invoke(this.m_RegsQuery.Provider, new object[] { updatemethod1 });
                //return (TResult)excute;
            }

            if (this.m_ProcessExprs.ContainsKey(expression) == false)
            {
                var updatemethod1 = ProcessExpr(expression) as MethodCallExpression;
                if (type.IsIEnumerable())
                {
                    if (updatemethod1.Type == typeof(IQueryable<RegistryKey>) || updatemethod1.Type == typeof(IOrderedQueryable<RegistryKey>))
                    {
                        //var sd = typeof(TData).ToSelectData();
                        //var select = typeof(TData).SelectMethod();
                        //updatemethod1 = Expression.Call(select, updatemethod1, sd);
                        
                        this.m_ProcessExprs[expression] = this.BuildSelect(updatemethod1, expression.Type.GetGenericArguments()[0]);
                    }
                    else if (updatemethod1.Type.GetGenericTypeDefinition() == typeof(IQueryable<>) && updatemethod1.Type.GetGenericArguments()[0].IsGenericType == true && updatemethod1.Type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IGrouping<,>))
                    {
                        //var oiou = updatemethod.Type.GetGenericTypeDefinition() == typeof(IQueryable<>);
                        //var popo = updatemethod.Type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IGrouping<,>);
                        var groupby = updatemethod1.Type.GetGenericArguments()[0].GetGenericArguments();
                        if (groupby.Length == 2 && groupby[1] == typeof(RegistryKey))
                        {
                            var methods = updatemethod1.Method.ReflectedType.GetMethods().Where(x => x.Name == updatemethod1.Method.Name);
                            methods = methods.Where(x => x.GetGenericArguments().Length == 3);
                            Type[] type3 = new Type[3];
                            var vvv = updatemethod1.Method.GetGenericArguments();
                            Array.Copy(updatemethod1.Method.GetGenericArguments(), type3, 2);
                            if (type3[1] == typeof(RegistryKey))
                            {
                                type3[1] = typeof(TData);
                            }
                            type3[2] = typeof(TData);
                            var oo = methods.ElementAt(0).MakeGenericMethod(type3);
                            Expression arg2 = updatemethod1.Arguments[1];
                            if (type3[1] == typeof(TData))
                            {
                                arg2 = typeof(TData).ToSelectData(this.Converts);
                            }
                            updatemethod1 = Expression.Call(oo, updatemethod1.Arguments[0], arg2, typeof(TData).ToSelectData(this.Converts));
                            
                        }
                        this.m_ProcessExprs[expression] = updatemethod1;
                    }
                    else
                    {
                        var ttype1 = updatemethod1.Type;
                        if (ttype1.GetGenericTypeDefinition() == typeof(IQueryable<>))
                        {
                            var ty = ttype1.GetGenericArguments()[0].GetProperties();
                        }
                        var sd = type.GetGenericArguments()[0].ToSelectData(this.Converts, null, updatemethod1.Type.GetGenericArguments()[0]);
                        if (sd != null)
                        {
                            var select = typeof(TResult).GetGenericArguments()[0].SelectMethod(updatemethod1.Type.GetGenericArguments()[0]);
                            updatemethod1 = Expression.Call(select, updatemethod1, sd);
                        }
                        this.m_ProcessExprs[expression] = updatemethod1;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Execute<TResult> {methodname}");

                    Expression expr = expression;

                    switch (methodname)
                    {
                        case "Insert":
                        case "InsertTo":
                        case "RemoveAll":
                        case "Update":
                            {
                                if (this.m_IsWritable == false)
                                {
                                    foreach (var oo in this.m_Regs)
                                    {
                                        oo.Close();
                                        oo.Dispose();
                                    }
                                    this.m_Regs.Clear();
                                }
                                this.m_IsWritable = true;
                            }
                            break;
                        default:
                            {
                                this.m_IsWritable = false;
                            }
                            break;
                    }

                    if (methodname == "Insert" || methodname == "InsertTo")
                    {
                        if (this.m_ProcessExprs.ContainsKey(expression) == false)
                        {
                            this.m_ProcessExprs[expression] = expression;
                        }
                    }
                    else if (expr_org.Arguments[0].Type.IsGenericType == true && expr_org.Arguments[0].Type.GetGenericTypeDefinition() == typeof(RegQuery<>))
                    {
                        RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                        regvisitor.Converts = this.Converts;
                        expr = regvisitor.VisitA(expr_org, this.m_RegSource, this.m_Exprs);
                        this.m_ProcessExprs[expression] = expr;
                        if (regvisitor.Fail != null)
                        {
                            this.m_Errors.Add(Tuple.Create(expression, expr, regvisitor.Fail));
                        }
                    }
                    else
                    {
                        Expression arg1 = null;
                        var updatemethod2 = expr as MethodCallExpression;
                        if (updatemethod2.Arguments.Count == 1)
                        {
                            var ggs = updatemethod2.Method.GetGenericArguments();
                            //var ggs = (this.m_RegMethod as MethodCallExpression)?.Method.GetGenericArguments();
                            if (ggs[0] == typeof(TData))
                            {
                                ggs[0] = typeof(RegistryKey);
                            }
                            var mmethod = updatemethod2.Method.GetGenericMethodDefinition().MakeGenericMethod(ggs);
                            expr = MethodCallExpression.Call(mmethod, updatemethod1);
                            this.m_ProcessExprs[expression] = expr;
                        }
                        else
                        {
                            RegExpressionVisitor<TData> regvisitor = new RegExpressionVisitor<TData>();
                            regvisitor.Converts = this.Converts;
                            Dictionary<Expression, Expression> saves = new Dictionary<Expression, Expression>();
                            saves[updatemethod2.Arguments[0]] = updatemethod1;
                            expr = regvisitor.VisitA(updatemethod2, this.m_RegSource, saves);
                            this.m_ProcessExprs[expression] = expr;
                        }


                    }
                }
            }



            this.m_RegMethod = this.m_ProcessExprs[expression];
            if (type.IsNullable()==true)
            {
                var excute = this.m_RegsQuery.Provider.Execute(this.m_RegMethod);
                //var creatquery = this.m_CreateQuery.MakeGenericMethod(tts);
                //var excute = creatquery.Invoke(this.m_RegsQuery.Provider, new object[] { this.m_RegMethod });
                return_hr = (TResult)excute;
                //return (TResult)excute;
            }
            else
            {
                object inst = null;
                var fail = this.CheckFail();
                if (fail != null)
                {
                    throw fail;
                }
                object excute = null;
                excute = this.m_RegsQuery.Provider.Execute(this.m_RegMethod);

                var excute_reg = excute as RegistryKey;
                if (excute_reg != null)
                {
                    var func_todata = excute_reg.ToDataFunc<TResult>(this.Converts);
                    inst = func_todata(excute_reg);
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


            
#endif
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

        //Expression m_SelectData;
        //MethodInfo m_SelectMethod; 
        //Expression BuildSelect(Expression src)
        //{
        //    if(this.m_SelectData == null)
        //    {
        //        this.m_SelectData = typeof(TData).ToSelectData();
        //    }
        //    if(this.m_SelectMethod == null)
        //    {
        //        this.m_SelectMethod = typeof(TData).SelectMethod();
        //    }
        //    var dst = Expression.Call(this.m_SelectMethod, src, this.m_SelectData);
        //    return dst;
        //}

        //Expression m_SelectData;
        //MethodInfo m_SelectMethod;
        Expression BuildSelect(Expression src, Type dst = null)
        {
            //if (this.m_SelectData == null)
            //{
            //    this.m_SelectData = typeof(TData).ToSelectData();
            //}
            //if (this.m_SelectMethod == null)
            //{
            //    this.m_SelectMethod = typeof(TData).SelectMethod();
            //}
            Type ttype = dst ?? typeof(TData);
            var sd = ttype.ToSelectData(this.Converts);
            var sd_method = ttype.SelectMethod();
            var expr_dst = Expression.Call(sd_method, src, sd);
            return expr_dst;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
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
    }
}
