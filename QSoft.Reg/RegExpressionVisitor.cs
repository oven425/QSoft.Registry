using Microsoft.Win32;
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
        Expression m_RegSource;
        public Expression Visit(Expression node, Type datatype, Expression regfunc)
        {
            this.m_DataType = datatype;
            this.m_RegSource = regfunc;
            Type ttupe = regfunc.GetType();
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
                else if(this.m_Lambda != null)
                {
                    expr = this.m_Lambda;
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
            //else if (this.m_Binarys.Count == 1)
            //{
            //    var binary = Expression.MakeBinary(node.NodeType, m_Binarys[0], this.m_ParamList.First().Item2);
            //    this.m_Binarys.Clear();
            //    this.m_ParamList.Clear();
            //    this.m_Binarys.Add(binary);
            //}
            else
            {
                switch(expr.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.NotEqual:
                        {
                            this.m_Binarys.Add(expr);
                        }
                        break;
                }
                
                this.m_ParamList.Clear();
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
                        //if(parameter.Type == this.m_DataType)
                        if(parameter.Name == this.m_DataTypeName)
                        {
                            var param = this.m_Parameters[parameter.Name];
                            var todata = this.ToData(param);
                            args[i] = todata;
                        }
                    }
                }
                m_NewExpression = Expression.New(node.Constructor, args, expr.Members);
            }
            else if(expr.Arguments.Count == 0)
            {
                m_NewExpression = Expression.New(expr.Constructor);
            }
            else
            {
                this.m_ParamList.Clear();
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
                //var zip = this.m_Binarys.Zip(node.Bindings, (member, binding) => new { member, binding });
                //foreach (var oo in zip)
                //{
                //    var binding = Expression.Bind(oo.binding.Member, oo.member);
                //    bindings.Add(binding);
                //}
                for(int i=0; i<node.Bindings.Count; i++)
                {
                    if(this.m_Binarys.Count >i)
                    {
                        var binding = Expression.Bind(node.Bindings[i].Member, this.m_Binarys[i]);
                        bindings.Add(binding);
                    }
                    else
                    {
                        bindings.Add(node.Bindings[i]);
                    }
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
                this.m_ConstantExpression_Value = null;
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
            else if(this.m_MemberAssigns.Count > 0)
            {
                if (this.m_NewExpression != null)
                {
                    m_MemberInit = Expression.MemberInit(m_NewExpression, this.m_MemberAssigns.Values);
                    m_NewExpression = null;
                }
                else
                {
                    m_MemberInit = Expression.MemberInit(node.NewExpression, this.m_MemberAssigns.Values);
                }
                this.m_MemberAssigns.Clear();
            }
            //this.m_ParamList.Clear();
            return expr;
        }

        Dictionary<string, MemberAssignment> m_MemberAssigns = new Dictionary<string, MemberAssignment>();
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMemberAssignment {node.Member.Name}");
            var expr = base.VisitMemberAssignment(node);
            if(this.m_MethodCall_Member != null)
            {
                var binding = Expression.Bind(expr.Member, this.m_MethodCall_Member);
                this.m_MemberAssigns[node.Member.Name] = binding;
                this.m_MethodCall_Member = null;
            }
            else if(this.m_Binarys.Count > 0)
            {
                var binding = Expression.Bind(expr.Member, this.m_Binarys[0]);
                this.m_MemberAssigns[node.Member.Name] = binding;
                this.m_Binarys.Clear();
            }
            else
            {
                this.m_MemberAssigns[node.Member.Name] = node;
            }
            
            return expr;
        }

        LambdaExpression m_Lambda = null;
        string m_DataTypeName = "";
        Type igroup1 = typeof(IGrouping<,>).MakeGenericType(typeof(RegistryKey), typeof(RegistryKey));
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitLambda T:{typeof(T)}");
            if(this.m_DataTypeInfos?.Count>0)
            {
                this.m_DataTypeName = node.Parameters[this.m_DataTypeInfos[0].Position].Name;
            }
            
            var expr = base.VisitLambda(node);

            var lambda = expr as LambdaExpression;
            Type type = lambda.Body.GetType();
            ParameterExpression[] parameters = new ParameterExpression[lambda.Parameters.Count];
            for(int i=0; i<parameters.Length; i++)
            {
                parameters[i] = this.m_Parameters[lambda.Parameters[i].Name];
            }
            if (this.m_Binarys.Count > 0)
            {
                if(this.m_Parameters.Count == 0)
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], lambda.Parameters);
                }
                else
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], parameters);
                }
            }
            else if(m_Member2Regs.Count>0)
            {
                m_Lambda = Expression.Lambda(this.m_Member2Regs[0], parameters);
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
                m_Lambda = Expression.Lambda(this.m_NewExpression, parameters);
                this.m_NewExpression = null;
            }
            else if (m_MemberInit != null)
            {
                m_Lambda = Expression.Lambda(this.m_MemberInit, this.m_Parameters.Values);
                this.m_MemberInit = null;
            }
            else if (this.m_Parameters.Count > 0 && this.m_Parameters.Values.Any(x => x.Type == typeof(RegistryKey) || x.Type == igroup1))
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

                        m_Lambda = Expression.Lambda(this.m_Parameters[pp.Name], parameters);
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
            this.m_ReturnType = this.m_Lambda.ReturnType;
            return expr;
        }

        Type m_ReturnType = null;

        Dictionary<string, ParameterExpression> m_Parameters = new Dictionary<string, ParameterExpression>();
        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitParameter {node.Type.Name}");
            //if(node.Type == this.m_DataType)
            if(node.Name == this.m_DataTypeName)
            {
                if (this.m_Parameters.ContainsKey(node.Name) == false)
                {
                    var parameter = Expression.Parameter(typeof(RegistryKey), node.Name);
                    this.m_Parameters.Add(parameter.Name, parameter);
                }
            }
            else if (node.Type == this.m_DataType)
            {
                if (this.m_Parameters.ContainsKey(node.Name) == false)
                {
                    if(string.IsNullOrEmpty(this.m_DataTypeName))
                    {
                        var parameter = Expression.Parameter(typeof(RegistryKey), node.Name);
                        this.m_Parameters.Add(parameter.Name, parameter);
                    }
                    else
                    {
                        this.m_Parameters.Add(node.Name, node);
                    }
                }
            }
            else if(node.Type.Name.Contains("IGrouping"))
            {
                var args = node.Type.GetGenericArguments();
                args.Replace(this.m_DataType, typeof(RegistryKey));
                var parameter = Expression.Parameter(typeof(IGrouping<, >).MakeGenericType(args), node.Name);
                if(this.m_Parameters.ContainsKey(parameter.Name) == false)
                {
                    this.m_Parameters[parameter.Name]= parameter;
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
        List<Expression> m_InnerMember2Regs = new List<Expression>();
        protected override Expression VisitMember(MemberExpression node)
        {
            var ttyp = node.Expression.GetType();
            System.Diagnostics.Trace.WriteLine($"VisitMember {node.Member.Name}");
            var expr = base.VisitMember(node) as MemberExpression;
            if ((expr.Expression as ParameterExpression)?.Name == this.m_DataTypeName || this.m_DataTypeName=="")
            {
                if (this.m_Parameters.Count > 0 && expr.Expression.Type == this.m_DataType)
                //if (this.m_Parameters.Count > 0)
                {
                    Expression left_args_1 = Expression.Constant(node.Member.Name);
                    string pp_name = (expr.Expression as ParameterExpression)?.Name;

                    Expression left_args_0 = this.m_Parameters.ElementAt(0).Value;
                    if (string.IsNullOrEmpty(pp_name) == false)
                    {
                        left_args_0 = this.m_Parameters[pp_name];
                    }
                    //if(this.m_Member2Regs.Count > 0)
                    //{
                    //    left_args_0 = this.m_Member2Regs[0];
                    //    this.m_Member2Regs.Clear();
                    //}
                    var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                    this.m_Member2Regs.Add(Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), left_args_0, left_args_1));
                }
                else if (this.m_Member2Regs.Count > 0)
                {
                    var mm = Expression.MakeMemberAccess(this.m_Member2Regs[0], node.Member);
                    this.m_Member2Regs.Clear();
                    this.m_Member2Regs.Add(mm);
                }
                else if (this.m_MethodCall_Member != null)
                {
                    var mm = Expression.MakeMemberAccess(this.m_MethodCall_Member, node.Member);
                    this.m_Member2Regs.Add(mm);
                    this.m_MethodCall_Member = null;
                }
                else if (this.m_Parameters.Count > 0)
                {
                    var mes = typeof(IGrouping<RegistryKey, RegistryKey>).GetMember("Key");
                    var mes1 = this.m_Parameters.ElementAt(0).Value.Type.GetMember(expr.Member.Name);
                    var mm = Expression.MakeMemberAccess(this.m_Parameters.ElementAt(0).Value, mes1.ElementAt(0));
                    //var mm = Expression.MakeMemberAccess(this.m_Parameters.ElementAt(0).Value, node.Member);
                    this.m_Member2Regs.Add(mm);
                }
                else
                {
                    this.m_Member2Regs.Add(expr);
                }
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

        //bool m_IsUpdate = false;
        MethodCallExpression m_MethodCall_Member = null;
        MethodCallExpression m_MethodCall = null;
        Stack<Tuple<Type, Expression>> m_ParamList = new Stack<Tuple<Type, Expression>>();
        List<ParameterInfo> m_DataTypeInfos = new List<ParameterInfo>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall {node.Method?.Name}");
            if (node.Method.ReturnType.IsGenericType == true)
            {
                var return1 = node.Method.ReturnType.GetGenericTypeDefinition();
                if (return1 == typeof(IQueryable<>))
                {
                    
                    var t4 = node.Method.GetGenericMethodDefinition().GetParameters();
                    string datatypename = t4.First().ParameterType.GenericTypeArguments[0].Name;
                    int select = -1;
                    if(node.Method.Name == "Zip")
                    {
                        select = 2;
                    }
                    if(select >=0)
                    {
                        var td = t4[select].ParameterType.GenericTypeArguments[0].GetMethods();
                        var rt = td[0].GetParameters();

                        m_DataTypeInfos = rt.Where(x => x.ParameterType.Name == datatypename).ToList();
                    }
                }
            }
            
            
            

            var expr = base.VisitMethodCall(node) as MethodCallExpression;


            if (this.m_Unary != null)
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
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
                var pps12 = methods.ElementAt(0).GetGenericArguments();
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
                        //this.m_ParamList.Push(Tuple.Create<Type, Expression>(this.m_DataType, this.ToSelectData()));
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
                else if(expr.Method.Name.Contains("Join")|| expr.Method.Name.Contains("GroupJoin"))
                {
                    var temp = this.m_ParamList.ToList();
                    this.m_ParamList.Clear();
                    foreach (var oo in temp)
                    {
                        this.m_ParamList.Push(oo);
                    }
                    tts1 = node.Method.GetGenericArguments();
                    tts1[0] = typeof(RegistryKey);
                    //if (tts1[3] == this.m_DataType)
                    //{
                    //    tts1[3] = typeof(RegistryKey);
                    //}
                    if (this.m_ReturnType == typeof(RegistryKey))
                    {
                        tts1[tts1.Length - 1] = typeof(RegistryKey);
                    }

                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                    
                }
                else if(expr.Method.Name == "Zip")
                {
                    var g1 = expr.Method.GetGenericArguments();
                    var g2 = expr.Method.GetGenericMethodDefinition().GetGenericArguments();
                    //var input1 = expr.Method.GetGenericMethodDefinition().GetParameters()[0].ParameterType.GenericTypeArguments[0].Name;
                    //var return4 = expr.Method.GetGenericMethodDefinition().ReturnType.GenericTypeArguments[0].Name;

                    var temp = this.m_ParamList.ToList();
                    this.m_ParamList.Clear();
                    foreach (var oo in temp)
                    {
                        this.m_ParamList.Push(oo);
                    }
                    tts1 = node.Method.GetGenericArguments();
                    tts1[0] = typeof(RegistryKey);
                    //if (tts1[tts1.Length - 1] == this.m_DataType)
                    if(this.m_ReturnType == typeof(RegistryKey))
                    {
                        tts1[tts1.Length - 1] = typeof(RegistryKey);
                    }

                    this.m_ParamList.Push(Tuple.Create<Type, Expression>(typeof(RegistryKey), methodcall_param_0));
                    //tts1 = this.m_ParamList.Select(x => x.Item1).ToArray();
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
                    //if(this.m_IsUpdate == true)
                    //{
                    //    var method = typeof(RegExpressionVisitor).GetMethod("ToUpdate");
                    //    this.m_MethodCall = Expression.Call(Expression.Constant(this), method, methodcall_param_0);
                    //}
                    //else
                    {
                        var param = this.m_ParamList.Select(x => x.Item2).Take(methods.ElementAt(0).GetParameters().Length);
                        var ggg = methods.ElementAt(0).GetGenericArguments();
                        var ggg2 = expr.Method.GetGenericArguments();
                        //this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), param);
                        this.m_MethodCall = Expression.Call(expr.Method.GetGenericMethodDefinition().MakeGenericMethod(tts1), m_ParamList.Select(x=>x.Item2));
                    }
                }
                else
                {
                    //if (this.m_IsUpdate == true)
                    //{
                    //    var method = typeof(RegExpressionVisitor).GetMethod("ToUpdate");
                    //    tts1 = this.m_ParamList.Select(x => x.Item1).Take(method.GetParameters().Length).ToArray();
                    //    var param = this.m_ParamList.Select(x => x.Item2);
                    //    this.m_MethodCall = Expression.Call(Expression.Constant(this), method, param.ElementAt(0));
                    //    //this.m_MethodCall = Expression.Call(Expression.Constant(this), method, methodcall_param_0);
                    //}
                    //else
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
                    this.m_MethodCall_Member = Expression.Call(m_Member2Regs[0], node.Method, this.m_ParamList.Select(x => x.Item2));
                }
                this.m_ParamList.Clear();
                m_Member2Regs.Clear();
            }
            else
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
                    //this.m_IsRegQuery = false;
#if CreateQuery
                    methodcall_param_0 = this.m_RegSource;
#else
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
                
                if (methods.ElementAt(0).IsGenericMethod==true)
                {
                    method = methods.ElementAt(0).MakeGenericMethod(tts1);
                }
                else
                {
                    method = methods.ElementAt(0);
                }
                if(this.m_Lambda != null)
                {
                    tts1[1] = this.m_Lambda.ReturnType;
                    method = node.Method.GetGenericMethodDefinition().MakeGenericMethod(tts1);
                    this.m_MethodCall = Expression.Call(method, methodcall_param_0, this.m_Lambda);
                }
                else if (this.m_ConstantExpression_Value != null)
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
                        this.m_MethodCall = Expression.Call(node.Object, method, this.m_ParamList.Select(x=>x.Item2));
                        //this.m_MethodCall = Expression.Call(node.Object, method, methodcall_param_0);
                    }
                    this.m_ParamList.Clear();
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
            }
            else if (node.Type.Name == "RegProvider")
            {
                m_IsRegQuery = true;
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
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
