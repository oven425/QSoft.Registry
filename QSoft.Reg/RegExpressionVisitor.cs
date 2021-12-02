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
        Expression m_RegSource;
        public Expression Visit(Expression node, Type datatype, Expression regfunc)
        {
            this.m_ExpressionSaves[node] = null;
            this.m_DataType = datatype;
            this.m_RegSource = regfunc;
            Type ttupe = regfunc.GetType();
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value;
            return expr;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            Type left = node.Left.GetType();
            Type right = node.Right.GetType();
            System.Diagnostics.Trace.WriteLine($"VisitBinary");
            

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
            System.Diagnostics.Trace.WriteLine($"VisitNew");
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
                        if (parameter.Type.IsGenericType == true && parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
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

        Dictionary<string, MemberAssignment> m_MemberAssigns = new Dictionary<string, MemberAssignment>();
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            this.m_ExpressionSaves[node.Expression] = null;
            System.Diagnostics.Trace.WriteLine($"VisitMemberAssignment {node.Member.Name}");
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
            System.Diagnostics.Trace.WriteLine($"VisitLambda T:{typeof(T)}");
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
                //var iiii = pps.FirstOrDefault(x => x.ParameterType.Name == m_PP.Item3);
                
                
            }
            //if(this.m_DataTypeInfos?.Count>0)
            //{
            //    this.m_DataTypeName = node.Parameters[this.m_DataTypeInfos[0].Position].Name;
            //}
            
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
            this.m_ReturnType = this.m_Lambda.ReturnType;
            this.m_Lastnode = expr;
            return expr;
        }

        Type m_ReturnType = null;

        Dictionary<string, ParameterExpression> m_Parameters = new Dictionary<string, ParameterExpression>();
        protected override Expression VisitParameter(ParameterExpression node)
        {
            m_ExpressionSaves[node] = null;
            System.Diagnostics.Trace.WriteLine($"VisitParameter {node.Type.Name}");
            var expr = base.VisitParameter(node) as ParameterExpression;
            if (expr.Name == this.m_DataTypeName)
            {
                if (this.m_Parameters.ContainsKey(expr.Name) == false)
                {
                    ParameterExpression pp = null;
                    
                    if (expr.Type.IsGenericType == true&&expr.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        pp = Expression.Parameter(typeof(IEnumerable<RegistryKey>), expr.Name);
                    }
                    else
                    {
                        pp = Expression.Parameter(typeof(RegistryKey), expr.Name);
                    }
                    
                    this.m_Parameters.Add(pp.Name, pp);
                    this.m_ExpressionSaves[expr] = pp;
                }
            }
            else if (expr.Type == this.m_DataType)
            {
                if (this.m_Parameters.ContainsKey(expr.Name) == false)
                {
                    if(string.IsNullOrEmpty(this.m_DataTypeName))
                    {
                        var parameter = Expression.Parameter(typeof(RegistryKey), expr.Name);
                        this.m_Parameters.Add(parameter.Name, parameter);
                        this.m_ExpressionSaves[expr] = parameter;
                    }
                    else
                    {
                        this.m_Parameters.Add(expr.Name, expr);
                        this.m_ExpressionSaves[expr] = expr;
                    }
                }
            }
            else if(expr.Type.Name.Contains("IGrouping"))
            {
                var args = expr.Type.GetGenericArguments();
                args.Replace(this.m_DataType, typeof(RegistryKey));
                var parameter = Expression.Parameter(typeof(IGrouping<, >).MakeGenericType(args), expr.Name);
                if(this.m_Parameters.ContainsKey(parameter.Name) == false)
                {
                    this.m_Parameters[parameter.Name]= parameter;
                    this.m_ExpressionSaves[expr] = parameter;
                }
            }
            else
            {
                if (this.m_Parameters.Count ==0)
                {
                    this.m_Parameters.Add(expr.Name, expr);
                    this.m_ExpressionSaves[expr] = expr;
                }
                else if(this.m_Parameters.ContainsKey(expr.Name)==false)
                {
                    this.m_Parameters.Add(expr.Name, expr);
                    this.m_ExpressionSaves[expr] = expr;
                }
            }
            if(this.m_ExpressionSaves[node] == null)
            {
                this.m_ExpressionSaves[node] = this.m_Parameters[node.Name];
            }
            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            m_ExpressionSaves[node] = null;
            //m_ExpressionSaves.Add(new Tr() { Src = node });
            var ttyp = node.Type;
            System.Diagnostics.Trace.WriteLine($"VisitMember {node.Member.Name}");
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
                            Expression left_args_1 = Expression.Constant(node.Member.Name);
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

        Tuple<ParameterInfo, MethodInfo, string> m_PP;
        protected override Expression VisitUnary(UnaryExpression node)
        {
            m_PP = null;
            if (m_PPs.ContainsKey(node) == true)
            {
                m_PP = m_PPs[node];
            }
            this.m_ExpressionSaves[node] = null;
            
            System.Diagnostics.Trace.WriteLine($"VisitUnary");
            var expr = base.VisitUnary(node);
            var exprs = this.m_ExpressionSaves.Clone(expr);
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

        //Stack<Tuple<Type, Expression>> m_ParamList = new Stack<Tuple<Type, Expression>>();
        Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>> m_PPs = new Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>>();
        DictionaryList<Expression, Expression> m_ExpressionSaves = new DictionaryList<Expression, Expression>();
        Expression m_Lastnode = null;
        Dictionary<Expression, string> GenericTypes = new Dictionary<Expression, string>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            m_ExpressionSaves[node] = null;
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall {node.Method?.Name}");
            if (node.Method.ReturnType.IsGenericType == true)
            {
                var return1 = node.Method.ReturnType.GetGenericTypeDefinition();
                
                if (return1 == typeof(IQueryable<>))
                {
                    
                    var t4 = node.Method.GetGenericMethodDefinition().GetParameters();
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
                            m_PPs[node.Arguments[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, datatypename);
                        }
                    }
                }
            }
            
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

        //Expression m_ConstantExpression_Value = null;
        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            System.Diagnostics.Trace.WriteLine($"VisitConstant {node.Type.Name}");
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
                //this.m_ParamList.Push(Tuple.Create(expr.Type.GetGenericArguments()[0], expr));
                this.m_ExpressionSaves[expr] = expr;
            }
            else
            {
                //this.m_ParamList.Push(Tuple.Create(node.Type, expr));
                //m_ConstantExpression_Value = expr;
                this.m_ExpressionSaves[expr] = expr;
            }
            this.m_Lastnode = expr;
            return expr;
        }

    }
}
