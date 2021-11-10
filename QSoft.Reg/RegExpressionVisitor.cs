﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QSoft.Registry.Linq
{
    class RegExpressionVisitor: ExpressionVisitor
    {
        Type m_DataType;
        Expression m_New = null;
        //IQueryable<RegistryKey> m_RegKeys = null;
        Expression m_RegSource;
        public Expression Visit(Expression node, Type datatype, IQueryable<RegistryKey> regkeys, Expression regfunc)
        {
            this.m_DataType = datatype;
            this.m_RegSource = regfunc;
            Type ttupe = regfunc.GetType();
            //this.m_RegKeys = regkeys;
            Expression expr = this.Visit(node);

            if (expr != null)
            {
                if(this.m_New!=null)
                {
                    expr = this.m_New;
                }
                else if (this.m_Unary != null)
                {
                    expr = this.m_Unary;
                }
                
            }
#if CreateQuery
            //var methodcall = expr as MethodCallExpression;
            //if (methodcall != null && methodcall.Method != null)
            //{
            //    if (methodcall.Method.ReturnType == typeof(IQueryable<RegistryKey>) || methodcall.Method.ReturnType == typeof(IOrderedQueryable<RegistryKey>))
            //    {
            //        var sd = this.ToSelectData();
            //        var selects = typeof(Queryable).GetMethods().Where(x => x.Name == "Select");
            //        var select = selects.ElementAt(0).MakeGenericMethod(typeof(RegistryKey), this.m_DataType);
            //        expr = Expression.Call(select, expr, sd);
            //    }
            //}
#else
            var methodcall = expr as MethodCallExpression;
            if(methodcall != null&& methodcall.Method!=null)
            {
                if(methodcall.Method.ReturnType == typeof(IQueryable<RegistryKey>) || methodcall.Method.ReturnType == typeof(IOrderedQueryable<RegistryKey>))
                {
                    var sd = this.ToSelectData();
                    var selects = typeof(Queryable).GetMethods().Where(x => x.Name == "Select");
                    var select = selects.ElementAt(0).MakeGenericMethod(typeof(RegistryKey), this.m_DataType);
                    expr = Expression.Call(select, expr, sd);
                }
            }
#endif
            return expr;
        }

        List<Expression> m_Binarys = new List<Expression>();
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Type left = node.Left.GetType();
            Type right = node.Right.GetType();
            System.Diagnostics.Trace.WriteLine($"VisitBinary");
            

            var expr = base.VisitBinary(node) as BinaryExpression;
            if(expr.Method != null)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Member2Regs[0], node.Right, expr.IsLiftedToNull, expr.Method);
                m_Binarys.Add(binary);
                this.m_Member2Regs.Clear();
                this.m_ParamList.Clear();
            }
            else if (this.m_Member2Regs.Count >0)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Member2Regs[0], node.Right);
                m_Binarys.Add(binary);
                this.m_Member2Regs.Clear();
                this.m_ParamList.Clear();
            }
            else if(m_MethodCall_Member != null)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_MethodCall_Member, node.Right);
                m_Binarys.Add(binary);
                m_MethodCall_Member = null;
                this.m_ParamList.Clear();
            }
            else if(this.m_Binarys.Count==2)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Binarys[0], m_Binarys[1]);
                m_Binarys.Clear();
                m_Binarys.Add(binary);
            }
            else
            {
                m_Binarys.Add(expr);
            }

            return expr;
        }


        NewExpression m_NewExpression = null;
        protected override Expression VisitNew(NewExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitNew");
            var expr = base.VisitNew(node) as NewExpression;
            if (this.m_Member2Regs.Count > 0)
            {
                if(node.Members == null)
                {
                    m_NewExpression = Expression.New(node.Constructor, this.m_Member2Regs);
                }
                else
                {
                    m_NewExpression = Expression.New(node.Constructor, this.m_Member2Regs, node.Members);
                }
                
                this.m_Member2Regs.Clear();
            }
            else if(expr.Members?.Count > 0)
            {
                var args = expr.Arguments.ToList();
                for(int i=0; i<args.Count; i++)
                {
                    ParameterExpression parameter = args[i] as ParameterExpression;
                    if(parameter != null)
                    {
                        if(parameter.Type == this.m_DataType)
                        {
                            var param = this.m_Parameters[parameter.Name];
                            var todata = this.ToData(param);
                            args[i] = todata;
                        }
                    }
                }
                m_NewExpression = Expression.New(node.Constructor, args, expr.Members);
            }

            return expr;
        }

        List<MemberBinding> m_UpdateBidings = null;
        MemberInitExpression m_MemberInit = null;
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var expr = base.VisitMemberInit(node);
            //if(this.m_IsUpdate==true)
            {
                this.m_UpdateBidings = node.Bindings.ToList();
            }
            if(this.m_Binarys.Count > 0)
            {
                List<MemberBinding> bindings = new List<MemberBinding>();
                var zip = this.m_Binarys.Zip(node.Bindings, (member, binding) => new { member, binding });
                foreach (var oo in zip)
                {
                    var binding = Expression.Bind(oo.binding.Member, oo.member);
                    bindings.Add(binding);
                }
                if (this.m_NewExpression != null)
                {
                    m_MemberInit = Expression.MemberInit(m_NewExpression, bindings);
                    m_NewExpression = null;
                }
                else
                {
                    m_MemberInit = Expression.MemberInit(node.NewExpression, bindings);
                }
                this.m_Binarys.Clear();
            }
            else if (this.m_Member2Regs.Count > 0)
            {
                List<MemberBinding> bindings = new List<MemberBinding>();
                var zip = this.m_Member2Regs.Zip(node.Bindings, (member, binding) => new { member, binding });
                foreach(var oo in zip)
                {
                    var binding = Expression.Bind(oo.binding.Member, oo.member);
                    bindings.Add(binding);
                }
                if(this.m_NewExpression != null)
                {
                    m_MemberInit = Expression.MemberInit(m_NewExpression, bindings);
                    m_NewExpression = null;
                }
                else
                {
                    m_MemberInit = Expression.MemberInit(node.NewExpression, bindings);
                }
                
                this.m_Member2Regs.Clear();
            }
            this.m_ParamList.Clear();
            return expr;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            //System.Diagnostics.Trace.WriteLine($"VisitMemberAssignment");
            return base.VisitMemberAssignment(node);
        }

        LambdaExpression m_Lambda = null;
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitLambda T:{typeof(T)}");
            var expr = base.VisitLambda(node);

            var lambda = expr as LambdaExpression;
            Type type = lambda.Body.GetType();

            if(this.m_Binarys.Count > 0)
            {
                if(this.m_Parameters.Count == 0)
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], lambda.Parameters);
                }
                else
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], this.m_Parameters.Values);
                }
            }
            else if(m_Member2Regs.Count>0)
            {
                m_Lambda = Expression.Lambda(this.m_Member2Regs[0], this.m_Parameters.Values);
                m_Member2Regs.Clear();
                //this.m_Parameters.Clear();
            }
            else if(this.m_MethodCall_Member != null)
            {
                m_Lambda = Expression.Lambda(this.m_MethodCall_Member, this.m_Parameters.Values);
                this.m_MethodCall_Member = null;
            }
            else if(this.m_NewExpression != null)
            {
                m_Lambda = Expression.Lambda(this.m_NewExpression, this.m_Parameters.Values);
                this.m_NewExpression = null;
            }
            else if (m_MemberInit != null)
            {
                m_Lambda = Expression.Lambda(this.m_MemberInit, this.m_Parameters.Values);
                this.m_MemberInit = null;
            }
            else if (this.m_Parameters.Count > 0 && this.m_Parameters.Values.Any(x=>x.Type == typeof(RegistryKey)))
            {
                if(lambda.ReturnType == this.m_DataType)
                {
                    
                    var pp = lambda.Body as ParameterExpression;
                    if(pp== null)
                    {
                        m_Lambda = Expression.Lambda(this.ToData(this.m_Parameters.Values.FirstOrDefault(x => x.Type == typeof(RegistryKey))), this.m_Parameters.Values);
                    }
                    else
                    {
                        m_Lambda = Expression.Lambda(this.m_Parameters[pp.Name], this.m_Parameters.Values.Reverse());
                    }
                }
                else if(lambda.Body is ParameterExpression)
                {
                    var pp = lambda.Body as ParameterExpression;
                    m_Lambda = Expression.Lambda(this.m_Parameters[pp.Name], this.m_Parameters.Values.Reverse());
                }
                else
                {
                    m_Lambda = Expression.Lambda(lambda.Body, this.m_Parameters.Values.Reverse());
                }
            }
            else
            {
                m_Lambda = lambda;
            }
            this.m_Parameters.Clear();
            return expr;
        }

        Dictionary<string, ParameterExpression> m_Parameters = new Dictionary<string, ParameterExpression>();
        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitParameter {node.Type.Name}");
            if(node.Type == this.m_DataType)
            {
                if (this.m_Parameters.ContainsKey(node.Name) == false)
                {
                    var parameter = Expression.Parameter(typeof(RegistryKey), node.Name);
                    this.m_Parameters.Add(parameter.Name, parameter);
                }
            }
            else
            {
                if (this.m_Parameters.Count ==0)
                {
                    this.m_Parameters.Add(node.Name, node);
                }
                else if(this.m_Parameters.ContainsKey(node.Name)==false)
                {
                    this.m_Parameters.Add(node.Name, node);
                }
            }
            
            return base.VisitParameter(node);
        }

        List<Expression> m_Member2Regs = new List<Expression>();
        protected override Expression VisitMember(MemberExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMember {node.Member.Name}");
            var expr = base.VisitMember(node);
            if (this.m_Parameters.Count > 0 && node.Expression.Type == this.m_DataType)
            {
                var left_args_1 = Expression.Constant(node.Member.Name);
                var left_args_0 = this.m_Parameters.ElementAt(0).Value;
                var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod==true);
                this.m_Member2Regs.Add(Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), left_args_0, left_args_1));
            }
            else
            {
                this.m_Member2Regs.Add(expr);
            }
            return expr;
        }
        
        UnaryExpression m_Unary = null;
        protected override Expression VisitUnary(UnaryExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitUnary");
            var expr = base.VisitUnary(node);
            if(this.m_Lambda != null)
            {
                this.m_Unary = Expression.MakeUnary(node.NodeType, this.m_Lambda, typeof(RegistryKey));
                Type ttype = this.m_Lambda.ReturnType;

                this.m_ParamList.Push(Tuple.Create<Type, Expression>(ttype, this.m_Unary));
                this.m_Binarys.Clear();
                this.m_Lambda = null;
            }
            
            return expr;
        }

        bool m_IsUpdate = false;
        MethodCallExpression m_MethodCall_Member = null;
        MethodCallExpression m_MethodCall = null;
        Stack<Tuple<Type, Expression>> m_ParamList = new Stack<Tuple<Type, Expression>>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall {node.Method?.Name}");
            
            
            var expr = base.VisitMethodCall(node) as MethodCallExpression;
            if (this.m_IsUpdate == false)
            {
                this.m_IsUpdate = expr.Method?.Name == "Update";
            }

            if (this.m_Unary != null)
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
                    //this.m_ConstantExpression_Source = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                    //methodcall_param_0 = this.m_ConstantExpression_Source;
                    methodcall_param_0 = this.m_RegSource;
                }
                else
                {
                    methodcall_param_0 = m_MethodCall;
                }
                
                
                var pps = node.Method.GetParameters();
                //var methods = expr.Method.ReflectedType.GetMethods().Where(x=>x.Name == expr.Method.Name&& x.GetParameters().Length == expr.Method.GetParameters().Length);
                var methods = expr.Method.ReflectedType.GetMethods().Where(x => x.Name == expr.Method.Name);
                
                methods = methods.Where(x => x.IsGenericMethod == expr.Method.IsGenericMethod);
                methods = methods.Where(x => x.GetParameters().Length == expr.Method.GetParameters().Length);
                var pps1 = methods.ElementAt(0).GetParameters();
                //var src = methods.Select(x => x.MakeGenericMethod(expr.Method.GetGenericArguments()).GetParameters());
                var dst = expr.Method.GetParameters();


                methods = methods.Where(x => x.MakeGenericMethod(expr.Method.GetGenericArguments()).GetParameters().Except(dst).Count() == 0);
                pps1 = methods.ElementAt(0).GetParameters();
                var tts = expr.Method.GetGenericArguments();
                
                Type[] tts1 = null;
                if (expr.Method.Name.Contains("GroupBy"))
                {
                    methods = expr.Method.ReflectedType.GetMethods().Where(x => x.Name == expr.Method.Name && x.GetParameters().Length == 3);
                    if(tts.Length==2)
                    {
                        var temp = this.m_ParamList.ToList();
                        temp.Reverse();
                        this.m_ParamList.Clear();
                        this.m_ParamList.Push(Tuple.Create<Type, Expression>(this.m_DataType, this.ToSelectData()));
                        foreach (var oo in temp)
                        {
                            this.m_ParamList.Push(oo);
                        }
                    }
                    else
                    {
                        var temp = this.m_ParamList.ToList();
                        this.m_ParamList.Clear();
                        foreach (var oo in temp)
                        {
                            this.m_ParamList.Push(oo);
                        }
                    }
                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                    tts1 = this.m_ParamList.Select(x => x.Item1).Take(methods.ElementAt(0).GetGenericArguments().Length).ToArray();
                    
                }
                else if(expr.Method.Name.Contains("Join")|| expr.Method.Name.Contains("GroupJoin")||expr.Method.Name == "Zip")
                {
                    var temp = this.m_ParamList.ToList();
                    this.m_ParamList.Clear();
                    foreach (var oo in temp)
                    {
                        this.m_ParamList.Push(oo);
                    }
                    tts1 = node.Method.GetGenericArguments();
                    tts1[0] = typeof(RegistryKey);
                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                }
                else if (expr.Method.Name.Contains("Update"))
                {
                    tts1 = node.Method.GetGenericArguments();
                    tts1[0] = typeof(RegistryKey);
                    var method = typeof(RegExpressionVisitor).GetMethod("ToUpdate");
                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                }
                else if (expr.Method.Name.Contains("Select"))
                {
                    tts1 = methods.ElementAt(0).GetGenericArguments();
                    //tts1[0] = tts1[1] = typeof(RegistryKey);
                    tts1[0] = typeof(RegistryKey);
                    if(tts[1] == this.m_DataType)
                    {
                        tts1[1] = typeof(RegistryKey);
                    }
                    else
                    {
                        tts1[1] = tts[1];
                    }
                   
                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                }
                else
                {
                    tts1 = node.Method.GetGenericArguments();
                    tts1[0] = typeof(RegistryKey);
                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                }

                if (this.m_IsRegQuery)
                {
                    if(this.m_IsUpdate == true)
                    {
                        var method = typeof(RegExpressionVisitor).GetMethod("ToUpdate");
                        this.m_MethodCall = Expression.Call(Expression.Constant(this), method, methodcall_param_0);
                    }
                    else
                    {
                        var param = this.m_ParamList.Select(x => x.Item2).Take(methods.ElementAt(0).GetParameters().Length);
                        this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), param);
                    }
                }
                else
                {
                    if (this.m_IsUpdate == true)
                    {
                        var method = typeof(RegExpressionVisitor).GetMethod("ToUpdate");
                        tts1 = this.m_ParamList.Select(x => x.Item1).Take(method.GetParameters().Length).ToArray();
                        var param = this.m_ParamList.Select(x => x.Item2);
                        this.m_MethodCall = Expression.Call(Expression.Constant(this), method, param.ElementAt(0));
                        //this.m_MethodCall = Expression.Call(Expression.Constant(this), method, methodcall_param_0);
                    }
                    else
                    {
                        tts1 = this.m_ParamList.Select(x => x.Item1).Take(methods.ElementAt(0).GetGenericArguments().Length).ToArray();
                        var param = this.m_ParamList.Select(x => x.Item2);
                        this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), param);
                    }
                }
                this.m_ParamList.Clear();
                this.m_Unary = null;
                this.m_IsRegQuery = false;
                this.m_New = this.m_MethodCall;
            }
            else if (m_Member2Regs.Count >0)
            {
                if(node.Method.IsStatic == true)
                {
                    this.m_MethodCall_Member = Expression.Call(node.Method, m_Member2Regs[0]);
                }
                else
                {
                    this.m_MethodCall_Member = Expression.Call(m_Member2Regs[0], node.Method, this.m_ConstantExpression_Value);
                }
                m_Member2Regs.Clear();
            }
            else
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
                    //this.m_IsRegQuery = false;
#if CreateQuery
                    //methodcall_param_0 = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                    methodcall_param_0 = this.m_RegSource;
#else
                    //methodcall_param_0 = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                    methodcall_param_0 = this.m_RegSource;
#endif

                }
                else
                {
                    if(m_MethodCall == null)
                    {
                        if (node.Method.Name == "CreateRegs")
                        {
                            m_MethodCall =  expr;
                        }
                    }
                    methodcall_param_0 = m_MethodCall;
                }


                var pps = node.Method.GetParameters();
                var methods = expr.Method.ReflectedType.GetMethods().Where(x => x.Name == expr.Method.Name && x.GetParameters().Length == expr.Method.GetParameters().Length);
                var tts = expr.Method.GetGenericArguments();
                var tts1 = new Type[tts.Length];
                for (int i = 0; i < tts.Length; i++)
                {
                    if (tts.ElementAt(i) == this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else
                    {
                        tts1[i] = tts.ElementAt(i);
                    }
                }
                MethodInfo method = null;
                if(methods.ElementAt(0).IsGenericMethod==true)
                {
                    method = methods.ElementAt(0).MakeGenericMethod(tts1);
                }
                else
                {
                    method = methods.ElementAt(0);
                }
                if (this.m_ConstantExpression_Value != null)
                {
#if CreateQuery
                    this.m_MethodCall = Expression.Call(method, methodcall_param_0, this.m_ConstantExpression_Value);
#else
                    this.m_MethodCall = Expression.Call(method, methodcall_param_0, this.m_ConstantExpression_Value);
#endif

                }
                else
                {
                    if(method.IsStatic == true)
                    {
                        this.m_MethodCall = Expression.Call(method, methodcall_param_0);
                    }
                    else
                    {
                        this.m_MethodCall = Expression.Call(node.Object, method, methodcall_param_0);
                    }
                }
                this.m_ConstantExpression_Value = null;

                this.m_New = this.m_MethodCall;
            }

            return expr;
        }


        bool m_IsRegQuery = false;
        //ConstantExpression m_ConstantExpression_Source = null;
        Expression m_ConstantExpression_Value = null;
        protected override Expression VisitConstant(ConstantExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitConstant {node.Type.Name}");
            var expr = base.VisitConstant(node);
            if (node.Type.Name == "RegQuery`1")
            {
                m_IsRegQuery = true;
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
                //if (this.m_RegKeys == null)
                //{
                //    m_ConstantExpression_Source = null;
                //}
            }
            else if (node.Type.Name.Contains("IEnumerable"))
            {
                this.m_ParamList.Push(Tuple.Create<Type, Expression>(expr.Type.GetGenericArguments()[0], expr));
            }
            else
            {
                this.m_ParamList.Push(Tuple.Create<Type, Expression>(node.Type, expr));
                m_ConstantExpression_Value = expr;
            }
            return expr;
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
            var pps = m_DataType.GetProperties().Where(x=>x.CanWrite==true);
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

        //public int ToUpdate(IEnumerable<RegistryKey> regs)
        //{
        //    var assigns = this.m_UpdateBidings.Select(x=>x as MemberAssignment).Where(x=>x!=null);
        //    if(assigns.Count() > 0)
        //    {
        //        Dictionary<string, object> values = new Dictionary<string, object>();
        //        foreach (var oo in assigns)
        //        {
        //            var objectMember = Expression.Convert(oo.Expression, typeof(object));

        //            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

        //            var getter = getterLambda.Compile();
        //            object aaa = getter();
        //            values[oo.Member.Name] = aaa;
        //        }
        //        foreach (var reg in regs)
        //        {
        //            foreach (var oo in assigns)
        //            {
        //                //var objectMember = Expression.Convert(oo.Expression, typeof(object));

        //                //var getterLambda = Expression.Lambda<Func<object>>(objectMember);

        //                //var getter = getterLambda.Compile();
        //                //object aaa = getter();
        //                reg.SetValue(oo.Member.Name, values[oo.Member.Name]);


        //            }
        //        }
        //    }
            
            
        //    return regs.Count();
        //}


    }
}
