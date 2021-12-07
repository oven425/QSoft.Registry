using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QSoft.Registry.Linq
{
    class RegExpressionVisitor: ExpressionVisitor
    {
        Type m_DataType;
        Expression m_RegSource;
        public string Fail { private set; get; }
        public Expression Visit(Expression node, Type datatype, Expression regfunc)
        {
            this.m_ExpressionSaves[node] = null;
            this.m_DataType = datatype;
            this.m_RegSource = regfunc;
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value;
            return expr;
        }

        public Expression Visit(Type datatype, MethodInfo method, params Expression[] nodes)
        {
            System.Diagnostics.Debug.WriteLine($"Visit {method?.Name}");

            this.PreparMethod(nodes[1], new Expression[] { nodes[0], nodes[1] }, method);
            this.m_DataType = datatype;

            Expression expr = this.Visit(nodes[1]);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value;
            return expr;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"VisitBinary");

            var expr = base.VisitBinary(node) as BinaryExpression;
            if(this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                var binary = Expression.MakeBinary(node.NodeType, exprs.ElementAt(0).Value, exprs.ElementAt(1).Value);
                this.m_ExpressionSaves[expr] = binary;
                this.m_Lastnode = expr;
            }
            return expr;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitNew");
            this.m_ExpressionSaves[node] = null;
            var expr = base.VisitNew(node) as NewExpression;

            var exprs = this.m_ExpressionSaves.Clone(expr).ToDictionary(x => x.Key, x => x.Value);
            for (int i = 0; i < exprs.Count; i++)
            {
                ParameterExpression parameter = exprs.ElementAt(i).Value as ParameterExpression;
                if (parameter != null)
                {
                    if (parameter.Name == this.m_DataTypeName)
                    {
                        if (parameter.Type.IsGenericType == true)
                        {
                            var param = this.m_Parameters[parameter.Name];
                            var select_method = this.m_DataType.SelectMethod_Enumerable();
                            var sd = this.m_DataType.ToLambdaData();
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key] = aaaaa;
                        }
                        else
                        {
                            var param = this.m_Parameters[parameter.Name];
                            var todata = this.m_DataType.ToData(param);
                            exprs[exprs.ElementAt(i).Key] = todata;
                        }
                    }
                    else
                    {
                        var param = this.m_Parameters[parameter.Name];
                        if(param.Type == this.m_DataType)
                        {
                            var todata = this.m_DataType.ToData(param);
                            exprs[exprs.ElementAt(i).Key] = todata;
                        }
                        
                    }
                }
            }
            if (expr.Members == null)
            {
                var expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value));
                this.m_ExpressionSaves[expr] = expr_new;
            }
            else
            {
                var expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value), expr.Members);
                this.m_ExpressionSaves[expr] = expr_new;
            }


            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            var expr = base.VisitMemberInit(node) as MemberInitExpression;

            if(this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                List<MemberBinding> bindings = new List<MemberBinding>();
                var members = exprs.Skip(1).Select(x => x.Value);
                for (int i = 0; i < expr.Bindings.Count; i++)
                {
                    var binding = Expression.Bind(expr.Bindings[i].Member, members.ElementAt(i));
                    bindings.Add(binding);
                }
                var expr_memberinit = Expression.MemberInit(exprs.First().Value as NewExpression, bindings);
                this.m_ExpressionSaves[expr] = expr_memberinit;
            }
            this.m_Lastnode = expr;
            return expr;
        }

        //Dictionary<string, MemberAssignment> m_MemberAssigns = new Dictionary<string, MemberAssignment>();
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            this.m_ExpressionSaves[node.Expression] = null;
            System.Diagnostics.Debug.WriteLine($"VisitMemberAssignment {node.Member.Name}");

            var expr = base.VisitMemberAssignment(node);
            if(this.m_Lastnode != null)
            {
                var exrps = this.m_ExpressionSaves.Clone(expr.Expression);
            }

            this.m_Lastnode = expr.Expression;
            return expr;
        }

        LambdaExpression m_Lambda = null;
        string m_DataTypeName = "";
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            this.m_ExpressionSaves[node] = null;
            if(this.m_ExpressionSaves1.Count == 0)
            {
                //var pps1 = pp1.GetGenericArguments();
                //for (int i = 0; i < node.Parameters.Count; i++)
                //{
                //    this.m_ExpressionSaves1[node.Parameters[i]] = pps1[i];
                //}
            }
            else
            {
                var pp1 = this.m_ExpressionSaves1[node];
                var pps1 = pp1.GetGenericArguments();
                for (int i = 0; i < node.Parameters.Count; i++)
                {
                    this.m_ExpressionSaves1[node.Parameters[i]] = pps1[i];
                }
            }

            System.Diagnostics.Debug.WriteLine($"VisitLambda T:{typeof(T)}");

            if(m_PP != null)
            {
                var pps = m_PP.Item2.GetParameters();
                foreach(var oo in pps)
                {
                    ParameterInfo pp = null;
                    if(oo.ParameterType.IsGenericType == true)
                    {
                        if(oo.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            if(oo.ParameterType.GetGenericArguments()[0].Name == m_PP.Item3)
                            {
                                pp = oo;
                            }
                        }
                    }
                    else
                    {
                        pp = pps.FirstOrDefault(x => x.ParameterType.Name == m_PP.Item3);
                    }
                    if (pp == null)
                    {
                        this.m_DataTypeName = node.Parameters[0].Name + "KK";
                    }
                    else
                    {
                        this.m_DataTypeName = node.Parameters[pp.Position].Name;
                    }
                }
            }
            
            var expr = base.VisitLambda(node);

            var lambda = expr as LambdaExpression;
            Type type = lambda.Body.GetType();
            ParameterExpression[] parameters = new ParameterExpression[lambda.Parameters.Count];
            for(int i=0; i<parameters.Length; i++)
            {
                parameters[i] = this.m_Parameters[lambda.Parameters[i].Name];
            }
            if (this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                m_Lambda = Expression.Lambda(exprs.First().Value, parameters);
                if(m_Lambda.ReturnType == typeof(IEnumerable<RegistryKey>))
                {
                    var param = exprs.First().Value;
                    var select_method = this.m_DataType.SelectMethod_Enumerable();
                    var sd = this.m_DataType.ToLambdaData();
                    var select_expr = Expression.Call(select_method, param, sd);
                    m_Lambda = Expression.Lambda(select_expr, parameters);
                }
                this.m_ExpressionSaves[expr] = this.m_Lambda;
            }
            else
            {
                m_Lambda = lambda;
            }
            this.m_Parameters.Clear();
            this.ReturnType = this.m_Lambda.ReturnType;
            this.m_Lastnode = expr;
            return expr;
        }

        public Type ReturnType { set; get; } = null;

        Dictionary<string, ParameterExpression> m_Parameters = new Dictionary<string, ParameterExpression>();
        protected override Expression VisitParameter(ParameterExpression node)
        {
            m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"VisitParameter {node.Type.Name}");

            var expr = base.VisitParameter(node) as ParameterExpression;
            var pp1 = this.m_ExpressionSaves1[expr];
            //if(this.m_Lastnode != null)
            {
                if (this.m_Parameters.ContainsKey(expr.Name) == false)
                {
                    if(pp1.IsGenericType == true&&pp1.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var type = this.m_GenericTypes[pp1.GetGenericArguments()[0].Name];
                        var ie = pp1.GetGenericTypeDefinition().MakeGenericType(type);
                        var pp = Expression.Parameter(ie, expr.Name);
                        this.m_Parameters[expr.Name] = pp;
                    }
                    else if(this.m_GenericTypes.ContainsKey(pp1.Name) == true)
                    {
                        var type = this.m_GenericTypes[pp1.Name];
                        var pp = Expression.Parameter(type, expr.Name);
                        this.m_Parameters[expr.Name] = pp;
                    }
                    else
                    {
                        this.m_Parameters[expr.Name] = expr;
                    }
                    
                }
                this.m_ExpressionSaves[expr] = this.m_Parameters[expr.Name];
            }
            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            m_ExpressionSaves[node] = null;
            var ttyp = node.Type;

            System.Diagnostics.Debug.WriteLine($"VisitMember {node.Member.Name}");

            var expr = base.VisitMember(node) as MemberExpression;
            if(this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                if((expr.Expression as ParameterExpression)?.Name == this.m_DataTypeName || this.m_DataTypeName == "")
                {
                    if(exprs.Last().Value is ParameterExpression)
                    {
                        if(exprs.Last().Value.Type.IsGenericType==true&& exprs.Last().Value.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                        {
                            var mes1 = exprs.Last().Value.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mes1.ElementAt(0));
                            this.m_ExpressionSaves[expr] = expr_member;
                        }
                        else
                        {
                            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                            if (node.Member.GetCustomAttributes(typeof(RegIgnore), true).Length > 0)
                            {
                                this.Fail = $"{node.Member.Name} is ignored, please do not use";
                                //throw new Exception($"{node.Member.Name} is ignored, please do not use");
                            }
                            var regname = node.Member.GetCustomAttributes(typeof(RegPropertyName), true) as RegPropertyName[];
                            Expression left_args_1 = null;
                            if (regname.Length > 0)
                            {
                                left_args_1 = Expression.Constant(regname.First().Name);
                            }
                            else
                            {
                                left_args_1 = Expression.Constant(node.Member.Name);
                            }
                            var member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), exprs.ElementAt(0).Value, left_args_1);
                            this.m_ExpressionSaves[expr] = member;
                        }
                    }
                    else
                    {
                        var expr_member = Expression.MakeMemberAccess(exprs.First().Value, expr.Member);
                        this.m_ExpressionSaves[expr] = expr_member;
                    }
                }
                else
                {
                    var expr_member = Expression.MakeMemberAccess(exprs.First().Value, expr.Member);
                    this.m_ExpressionSaves[expr] = expr_member;
                }

                this.m_Lastnode = expr;

                return expr;
            }
            return expr;
        }

        Tuple<ParameterInfo, MethodInfo, string> m_PP = null;
        Dictionary<string, Type> m_GenericTypes = new Dictionary<string, Type>();
        protected override Expression VisitUnary(UnaryExpression node)
        {
            m_PP = null;
            if (m_PPs.ContainsKey(node) == true)
            {
                m_PP = m_PPs[node];
            }
            this.m_ExpressionSaves[node] = null;
            if(this.m_ExpressionSaves1.ContainsKey(node) == true)
            {
                var pp1 = this.m_ExpressionSaves1[node];
                if (pp1 != null)
                {
                    this.m_ExpressionSaves1[node.Operand] = pp1.GetGenericArguments()[0];
                }
            }

            System.Diagnostics.Debug.WriteLine($"VisitUnary");

            var expr = base.VisitUnary(node);
            var exprs = this.m_ExpressionSaves.Clone(expr);

            if(this.m_ExpressionSaves1.ContainsKey(expr) == true)
            {
                var pp = this.m_ExpressionSaves1[expr];
                if (pp != null)
                {
                    var kkey = pp.GetGenericArguments()[0].GetGenericArguments().Last().Name;
                    var lambda = exprs.First().Value as LambdaExpression;
                    if (lambda != null)
                    {
                        this.m_GenericTypes[kkey] = lambda.ReturnType;
                        this.m_Parameters.Clear();
                    }
                }
            }

            Type result_type = expr.Type;
            if (result_type.IsGenericType == true)
            {
                if (result_type.GetGenericTypeDefinition() == typeof(Func<,>))
                {
                    result_type = result_type.GetGenericArguments().First();
                }
            }

            UnaryExpression unary = Expression.MakeUnary(node.NodeType, exprs.First().Value, result_type);
            this.m_ExpressionSaves[expr] = unary;
            
            this.m_Lastnode = expr;
            return expr;
        }

        void PreparMethod(Expression node, Expression[] args, MethodInfo method)
        {
            m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"PreparMethod {method?.Name}");

            if (method.IsGenericMethod == true)
            {
                var dic = method.GetParameters();
                var dicc = method.GetGenericMethodDefinition().GetGenericArguments();
                for (int i = 0; i < dicc.Length; i++)
                {
                    if (i == 0)
                    {
                        this.m_GenericTypes[dicc[i].Name] = typeof(RegistryKey);
                    }
                    else
                    {
                        if (dic[i].ParameterType.IsGenericType == true && dic[i].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            this.m_GenericTypes[dicc[i].Name] = dic[i].ParameterType.GetGenericArguments()[0];
                        }
                        else
                        {
                            this.m_GenericTypes[dicc[i].Name] = null;
                        }
                    }
                }


                var dic2 = method.GetGenericMethodDefinition().GetParameters();
                for (int i = 0; i < args.Length; i++)
                {
                    m_ExpressionSaves1[args[i]] = dic2[i].ParameterType;
                }
            }


            if (method.ReturnType.IsGenericType == true)
            {
                var return1 = method.ReturnType.GetGenericTypeDefinition();

                if (return1 == typeof(IQueryable<>))
                {
                    var t4 = method.GetGenericMethodDefinition().GetParameters();
                    string datatypename = t4.First().ParameterType.GetGenericArguments()[0].Name;
                    for (int i = 0; i < t4.Length; i++)
                    {
                        var gss = t4[i].ParameterType.GetGenericArguments();
                        MethodInfo invoke = null;
                        if (gss.Length > 0)
                        {
                            invoke = gss[0].GetMethod("Invoke");
                        }
                        if (invoke != null)
                        {
                            m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, datatypename);
                        }
                    }
                }
            }
        }


        Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>> m_PPs = new Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>>();
        DictionaryList<Expression, Expression> m_ExpressionSaves = new DictionaryList<Expression, Expression>();
        Dictionary<Expression, Type> m_ExpressionSaves1 = new Dictionary<Expression, Type>();
        Expression m_Lastnode = null;
        Dictionary<Expression, string> GenericTypes = new Dictionary<Expression, string>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMethodCall {node.Method?.Name}");

            this.PreparMethod(node, node.Arguments.ToArray(), node.Method);

            var expr = base.VisitMethodCall(node) as MethodCallExpression;

            var exprs1 = this.m_ExpressionSaves.Clone(expr);
            var ttypes1 = exprs1.Select(x => x.Value).GetTypes(expr.Method);
            MethodCallExpression methodcall = null;
            if (expr.Method.IsStatic == true)
            {
                switch(expr.Method.Name)
                {
                    case "Except":
                    case "Union":
                    case "Intersect":
                    case "Distinct":
                        {
                            var sd = this.m_DataType.ToSelectData();
                            var select = this.m_DataType.SelectMethod();
                            var selectexpr = Expression.Call(select, this.m_RegSource, sd);

                            var args = exprs1.Select(x => x.Value).ToList();
                            args[0] = selectexpr;
                            methodcall = Expression.Call(expr.Method, args);
                        }
                        break;
                    default:
                        {
                            if(expr.Method.IsGenericMethod == true)
                            {
                                methodcall = Expression.Call(expr.Method.GetGenericMethodDefinition().MakeGenericMethod(ttypes1), exprs1.Select(x => x.Value));
                            }
                            else
                            {
                                methodcall = Expression.Call(expr.Method, exprs1.Select(x => x.Value));
                            }
                        }
                        break;
                }
                
            }
            else
            {
                if(expr.Method.IsGenericMethod == true)
                {

                }
                else
                {
                    methodcall = Expression.Call(exprs1.First().Value, expr.Method, exprs1.Skip(1).Select(x => x.Value));
                }
                
            }
            this.m_ExpressionSaves[expr] = methodcall;
            this.m_Lastnode = expr;
            return expr;

        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"VisitConstant {node.Type.Name}");

            var expr = base.VisitConstant(node);
            if (node.Type.Name == "RegQuery`1")
            {
                this.m_ExpressionSaves[expr] = this.m_RegSource;
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
            }
            else if (node.Type.Name == "RegProvider")
            {
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
            }
            else if (node.Type.Name.Contains("IEnumerable"))
            {
                this.m_ExpressionSaves[expr] = expr;
            }
            else
            {
                this.m_ExpressionSaves[expr] = expr;
            }
            this.m_Lastnode = expr;
            return expr;
        }

    }
}
