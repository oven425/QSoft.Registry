using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QSoft.Registry.Linq
{
    class RegExpressionVisitor<TData>: ExpressionVisitor
    {
        Expression m_RegSource;
        public string Fail { private set; get; }
        public Expression Visit(Expression node, Expression regfunc)
        {
            this.m_ExpressionSaves[node] = null;
            this.m_RegSource = regfunc;
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value;
            return expr;
        }

        Dictionary<Expression, Expression> m_Saves;
        public Expression VisitA(Expression node, Expression regfunc, Dictionary<Expression, Expression> saves)
        {
            this.m_Saves = saves;
            this.m_ExpressionSaves[node] = null;
            this.m_RegSource = regfunc;
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value;
            return expr;
        }

        public Expression Visit(MethodInfo method, params Expression[] nodes)
        {
            System.Diagnostics.Debug.WriteLine($"Visit {method?.Name}");
            this.PreparMethod(nodes[1], new Expression[] { nodes[0], nodes[1] }, method);


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
                            var select_method = typeof(TData).SelectMethod_Enumerable();
                            var sd = typeof(TData).ToLambdaData();
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key] = aaaaa;
                        }
                        else
                        {
                            var param = this.m_Parameters[parameter.Name];
                            var todata = typeof(TData).ToData(param);
                            exprs[exprs.ElementAt(i).Key] = todata;
                            exprs[exprs.ElementAt(i).Key] = this.m_Parameters[parameter.Name];
                        }
                    }
                    else
                    {
                        var param = this.m_Parameters[parameter.Name];
                        if(param.Type == typeof(TData))
                        {
                            var todata = typeof(TData).ToData(param);
                            exprs[exprs.ElementAt(i).Key] = todata;
                        }
                        
                    }
                }
            }
            if (expr.Members == null)
            {
                var pps = exprs.Select(x =>
                {
                    Expression p = x.Value;
                    if(x.Key.Type == typeof(TData))
                    {
                        p = typeof(TData).ToData(x.Value as ParameterExpression);
                    }
                    return p;
                });
                var expr_new = Expression.New(expr.Constructor, pps);
                this.m_ExpressionSaves[expr] = expr_new;
            }
            else
            {
                var members = expr.Members.ToArray();
                var con_pps = expr.Constructor.GetParameters();
                var pps1 = con_pps.Select(x =>
                {
                    Type type = x.ParameterType;
                    if(x.ParameterType == typeof(TData))
                    {
                        type = typeof(RegistryKey);
                    }
                    return Tuple.Create(type, x.Name);
                });
                
                var anyt = pps1.BuildType();
                var anyt_con = anyt.GetConstructors();
                var mems = anyt.GetMembers();
                 var expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Select(x => x.Value), anyt.GetProperties());
                //var expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value), expr.Members);
                this.m_ExpressionSaves[expr] = expr_new;
            }


            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMemberInit");
            this.m_ExpressionSaves[node] = null;
            var expr = base.VisitMemberInit(node) as MemberInitExpression;

            if(this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);

                
                var bindings = exprs.Skip(1).Select((x,i) =>
                {
                    var binding = Expression.Bind(expr.Bindings[i].Member, x.Value);
                    return binding;
                });
                if(this.LastMethodName != "Update")
                {
                    var expr_memberinit = Expression.MemberInit(exprs.First().Value as NewExpression, bindings);
                    this.m_ExpressionSaves[expr] = expr_memberinit;
                }
                else
                {
                    var anyt = expr.Bindings.Select(x => x.Member as PropertyInfo).BuildType();
                    var expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Skip(1).Select(x => x.Value), anyt.GetProperties());
                    this.m_ExpressionSaves[expr] = expr_new;
                }
            }
            this.m_Lastnode = expr;
            return expr;
        }

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
                        break;
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
                    var select_method = typeof(TData).SelectMethod_Enumerable();
                    var uu = typeof(TData);
                    var sd = typeof(TData).ToLambdaData();
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
                        var type = this.m_GenericTypes.First()[pp1.GetGenericArguments()[0].Name];
                        var ie = pp1.GetGenericTypeDefinition().MakeGenericType(type);
                        var pp = Expression.Parameter(ie, expr.Name);
                        this.m_Parameters[expr.Name] = pp;
                    }
                    else if(expr.Type.IsGenericType==true && expr.Type.GetGenericTypeDefinition()==typeof(IGrouping<,>))
                    {
                        var find = this.m_GenericTypes.FirstOrDefault(x => x.ContainsKey(pp1.Name));
                        var ie = expr.Type.GetGenericTypeDefinition().MakeGenericType(find.Values.ToArray());
                        var pp = Expression.Parameter(ie, expr.Name);
                        this.m_Parameters[expr.Name] = pp;
                    }
                    else if(this.m_Parameters.Count ==0)
                    {
                        var find = this.m_GenericTypes.FirstOrDefault(x => x.ContainsKey(pp1.Name));
                        if(find != null)
                        {
                            var type = find[pp1.Name];
                            var pp = Expression.Parameter(type, expr.Name);
                            this.m_Parameters[expr.Name] = pp;
                        }
                        else
                        {
                            this.m_Parameters[expr.Name] = expr;
                        }
                    }
                    else if(this.m_GenericTypes.First().ContainsKey(pp1.Name) == true)
                    {
                        var type = this.m_GenericTypes.First()[pp1.Name];
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
            m_ExpressionSaves[node] = node.Expression == null?node:null;

            var ttyp = node.Type;

            System.Diagnostics.Debug.WriteLine($"VisitMember {node.Member.Name}");

            var expr = base.VisitMember(node) as MemberExpression;
            if(this.m_Lastnode != null && expr.Expression != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                if((expr.Expression as ParameterExpression)?.Name == this.m_DataTypeName || this.m_DataTypeName == "")
                {
                    if (exprs.Last().Value is ParameterExpression)
                    {
                        if (exprs.Last().Value.Type.IsGenericType == true && exprs.Last().Value.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                        {
                            var mes1 = exprs.Last().Value.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mes1.ElementAt(0));
                            this.m_ExpressionSaves[expr] = expr_member;
                        }
                        else
                        {
                            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);

                            var attr = node.Member.GetCustomAttributes(true).FirstOrDefault();
                            Expression member = null;
                            Expression left_args_1 = null;
                            if (attr is RegIgnore)
                            {
                                this.Fail = $"{node.Member.Name} is ignored, please do not use";
                            }
                            else if (attr is RegPropertyName)
                            {
                                left_args_1 = Expression.Constant((attr as RegPropertyName).Name);
                                member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), exprs.ElementAt(0).Value, left_args_1);
                            }
                            else if (attr is RegSubKeyName)
                            {
                                var subkeyname = attr as RegSubKeyName;
                                member = Expression.Property(exprs.First().Value, "Name");
                                if(subkeyname.IsFullName == false)
                                {
                                    var method = typeof(RegQueryHelper).GetMethod("GetLastSegement");
                                    member = Expression.Call(method, member);
                                }
                            }
                            if(member ==null&&left_args_1 == null)
                            {
                                left_args_1 = Expression.Constant(node.Member.Name);
                                member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), exprs.ElementAt(0).Value, left_args_1);
                            }
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
        Stack<Dictionary<string, Type>> m_GenericTypes = new Stack<Dictionary<string, Type>>();
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
                        this.m_GenericTypes.First()[kkey] = lambda.ReturnType;
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
                this.m_GenericTypes.Push(new Dictionary<string, Type>());
                var dic = method.GetParameters();
                var dicc = method.GetGenericMethodDefinition().GetGenericArguments();
                for (int i = 0; i < dicc.Length; i++)
                {
                    if (i == 0)
                    {
                        if (dic[i].ParameterType == typeof(IEnumerable<TData>))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else if (dic[i].ParameterType == typeof(IQueryable<TData>))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else if(dic[i].ParameterType == typeof(IQueryable<IGrouping<TData,TData>>))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                            this.m_GenericTypes.First()[dicc[i+1].Name] = typeof(RegistryKey);
                            i = i + 1;
                        }
                        else
                        {
                            if(dic[i].ParameterType.IsGenericType == true)
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType.GetGenericArguments()[0];
                            }
                            else
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType;
                            }
                        }
                        
                    }
                    else
                    {
                        if (dic[i].ParameterType.IsGenericType == true && dic[i].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType.GetGenericArguments()[0];
                        }
                        else
                        {
                            if (dic[i].ParameterType.IsGenericType == true)
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType.GetGenericArguments()[0];
                            }
                            else
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType;
                            }
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

                if (return1 == typeof(IQueryable<>)&& method.IsGenericMethod==true)
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
        //Dictionary<Expression, string> GenericTypes = new Dictionary<Expression, string>();
        string LastMethodName = "";
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMethodCall {node.Method?.Name}");
            if (this.m_Saves.ContainsKey(node) == true)
            {
                var aaa = this.m_Saves[node];
                this.m_ExpressionSaves[node] = aaa;
                return node;
            }
            LastMethodName = node.Method.Name;
            this.PreparMethod(node, node.Arguments.ToArray(), node.Method);

            var expr = base.VisitMethodCall(node) as MethodCallExpression;

            var exprs1 = this.m_ExpressionSaves.Clone(expr);
            var ttypes1 = exprs1.Select(x => x.Value).GetTypes(expr.Method);
            MethodCallExpression methodcall = null;
            if (expr.Method.IsStatic == true)
            {
                switch(expr.Method.Name)
                {
                    //case "Except":
                    //case "Intersect":
                    //    {
                    //        var pps = expr.Method.GetParameters();
                    //        var vv = typeof(RegQueryEx).GetMethods().Where(x =>
                    //        {
                    //            bool result = false;
                    //            if(x.Name == $"{expr.Method.Name}_RegistryKey" && x.GetParameters().Length == pps.Length)
                    //            {
                    //                result = true;
                    //            }
                    //            return result;
                    //        }).First();
                    //        ttypes1 = exprs1.Select(x => x.Value).GetTypes(vv);
                    //        methodcall = Expression.Call(vv.MakeGenericMethod(ttypes1), exprs1.Select(x => x.Value));
                    //    }
                    //    break;
                    case "Except":
                    case "Intersect":
                    case "Union":
                    case "Distinct":
                        {
                            var sd = typeof(TData).ToSelectData();
                            var select = typeof(TData).SelectMethod();
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
            if(expr.Method.IsGenericMethod == true && this.m_GenericTypes.Count > 0)
            {
                this.m_GenericTypes.Pop();
            }
            

            this.m_ExpressionSaves[expr] = methodcall;
            this.m_Lastnode = expr;
            return expr;

        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"VisitConstant {node.Type.Name}");

            var expr = base.VisitConstant(node) as ConstantExpression;
            Expression expr1 = null;
            var ttype = node.Value.GetType();
            if (ttype.GetCustomAttributes(true).Any(x=>x is CompilerGeneratedAttribute) ==true)
            {
                var pps = ttype.GetFields();
                var op = pps.First().GetValue(expr.Value);
                if(op.GetType() == typeof(RegQuery<TData>))
                {
                    var reguqery = op as RegQuery<TData>;
                    var regprovider = reguqery?.Provider as RegProvider<TData>;
                    //expr1 = regprovider.m_RegSource;
                }
            }
            
            if (node.Type == typeof(RegQuery<TData>))
            {
                var reguqery = node.Value as RegQuery<TData>;
                var regprovider = reguqery?.Provider as RegProvider<TData>;
                //this.m_ExpressionSaves[expr] = regprovider.m_RegSource;
                expr1 = regprovider.m_RegSource;
            }
            //else if(node.Value is RegQuery<TData>)
            //{

            //}
            //else
            //{
            //    this.m_ExpressionSaves[expr] = expr;
            //}
            this.m_ExpressionSaves[expr] = expr1??expr;
            this.m_Lastnode = expr;
            return expr;
        }

    }
}
