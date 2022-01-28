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
        public Expression VisitA(Expression node, Expression regfunc, Dictionary<Expression, Expression> saves, bool lastquery=false)
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
            this.LastMethodName = method?.Name;
            System.Diagnostics.Debug.WriteLine($"Visit {method?.Name}");
#if RegToEnd
            this.PreparMethod1(nodes[1], new Expression[] { nodes[0], nodes[1] }, method);
#else
            this.PreparMethod(nodes[1], new Expression[] { nodes[0], nodes[1] }, method);
#endif



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
#if RegToEnd
                    if(this.m_DataTypeNames.Any(x=>x==parameter.Name))
#else
                    if (parameter.Name == this.m_DataTypeName)
#endif
                    {
                        if (parameter.Type.IsGenericType == true)
                        {
#if RegToEnd
                            exprs[exprs.ElementAt(i).Key] = this.m_Parameters[parameter.Name];
#else
                            var param = this.m_Parameters[parameter.Name];
                            var select_method = typeof(TData).SelectMethod_Enumerable();
                            var sd = typeof(TData).ToLambdaData();
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key] = aaaaa;
#endif
                        }
                        else
                        {
#if RegToEnd
                            exprs[exprs.ElementAt(i).Key] = this.m_Parameters[parameter.Name];
#else
                            var param = this.m_Parameters[parameter.Name];
                            var todata = typeof(TData).ToData(param);
                            exprs[exprs.ElementAt(i).Key] = todata;
#endif
                        }
                    }
                    else
                    {
#if RegToEnd
                        if (parameter.Type.IsGenericType == true)
                        {

                            var temptype = exprs.ElementAt(i).Key.Type.GetGenericArguments()[0];
                            var param = this.m_Parameters[parameter.Name];
                            var select_method = temptype.SelectMethod_Enumerable();
                            var sd = temptype.ToLambdaData();
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key] = aaaaa;
                        }
                        else
                        {
                            var param = this.m_Parameters[parameter.Name];
                            if (param.Type == typeof(TData))
                            {
                                var todata = typeof(TData).ToData(param);
                                exprs[exprs.ElementAt(i).Key] = todata;
                            }
                        }
#else
                        if (parameter.Type.IsGenericType == true)
                        {

                            var temptype = exprs.ElementAt(i).Key.Type.GetGenericArguments()[0];
                            var param = this.m_Parameters[parameter.Name];
                            var select_method = temptype.SelectMethod_Enumerable();
                            var sd = temptype.ToLambdaData();
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key] = aaaaa;
                        }
                        else
                        {
                            var param = this.m_Parameters[parameter.Name];
                            if (param.Type == typeof(TData))
                            {
                                var todata = typeof(TData).ToData(param);
                                exprs[exprs.ElementAt(i).Key] = todata;
                            }
                        }
                            
#endif
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
#if RegToEnd
                var con_pps = expr.Constructor.GetParameters();
                Expression expr_new = null;
                if (exprs.Select(x => x.Value.Type).Any(x => x.HaseRegistryKey()))
                {
                    var pps1 = con_pps.Replace(typeof(TData), typeof(RegistryKey));
                    var pps2 = con_pps.Zip(exprs.Select(x => x.Value), (pp, values) => new { pp, values })
                        .Select(x => Tuple.Create(x.values.Type, x.pp.Name));
                    var exists = exprs.Where(x => Type.GetTypeCode(x.Value.Type) == TypeCode.Object && x.Value.Type != typeof(RegistryKey)).Select(x => x.Value.Type);
                    var anyt = pps2.BuildType(exists);

                    var po = anyt.GetConstructors()[0];

                    expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Select(x => x.Value), anyt.GetProperties());
                    //expr_new = expr;
                }
                else
                {
                    expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value), expr.Members);
                }
#else
                var expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value), expr.Members);
#endif
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
                switch(this.LastMethodName)
                {
                    case "Update":
                    case "InsertOrUpdate":
                        {
                            var anyt = expr.Bindings.Select(x => x.Member as PropertyInfo).BuildType();
                            var expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Skip(1).Select(x => x.Value), anyt.GetProperties());
                            this.m_ExpressionSaves[expr] = expr_new;
                        }
                        break;
                    default:
                        {
                            var expr_memberinit = Expression.MemberInit(exprs.First().Value as NewExpression, bindings);
                            this.m_ExpressionSaves[expr] = expr_memberinit;
                        }
                        break;
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
        List<string> m_DataTypeNames = new List<string>();
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

#if RegToEnd
            if(this.m_PP == null)
            {
                if (m_PPs.ContainsKey(node) == true)
                {
                    m_PP = m_PPs[node];
                }
            }
            
            if (m_PP != null)
            {
                var pps = m_PP.Item2.GetParameters().ToList();
                for(int i=0; i<pps.Count; i++)
                {
                    ParameterInfo pp = null;
                    if (pps[i].ParameterType.IsGenericType == true)
                    {
                        if (pps[i].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {

                            if (m_PP.Item3.Any(x=>x== pps[i].ParameterType.GetGenericArguments()[0].Name))
                            {
                                pp = pps[i];
                            }
                        }
                    }
                    else
                    {
                        pp = pps.FirstOrDefault(x => m_PP.Item3.Any(y=>y==x.ParameterType.Name));
                    }
                    if (pp == null)
                    {
                        this.m_DataTypeNames.Add(node.Parameters[i].Name + "KK");
                    }
                    else
                    {
                        this.m_DataTypeNames.Add(node.Parameters[i].Name);
                    }
                }
            }
#else
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
#endif
            
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
#if RegToEnd
                if(lambda.ReturnType.IsGenericType == true&& (lambda.Type.GetGenericTypeDefinition()==typeof(Func<,>)||lambda.Type.GetGenericTypeDefinition() == typeof(Func<,,>)))
                {
                    Dictionary<Type, Type> changes = new Dictionary<Type, Type>();
                    //changes[typeof(TData)] = typeof(RegistryKey);
                    foreach(var oo in exprs)
                    {
                        changes[oo.Key.Type] = oo.Value.Type;
                    }
                    var functype = lambda.Type.ChangeType(changes);
                    //var ggs = lambda.Type.GetGenericTypeDefinition().GetGenericArguments();
                    //ggs[0] = parameters[0].Type;
                    //ggs[1] = parameters[1].Type;
                    //ggs[2] = exprs.First().Value.Type;
                    List<Type> ggs = new List<Type>();
                    ggs.AddRange(parameters.Select(x => x.Type));
                    ggs.Add(exprs.First().Value.Type);
                    var functype1 = lambda.Type.GetGenericTypeDefinition().MakeGenericType(ggs.ToArray());
                    var zz = functype.GetGenericArguments().Zip(functype1.GetGenericArguments(), (x, y) => new { x, y });
                    foreach(var oo in zz)
                    {
                        if(oo.x != oo.y)
                        {

                        }
                    }
                    //m_Lambda = Expression.Lambda(functype, exprs.First().Value, parameters);
                }
#endif
                if(m_Lambda == null)
                {
                    m_Lambda = Expression.Lambda(exprs.First().Value, parameters);
                }
                
                //if(m_Lambda.ReturnType == typeof(IEnumerable<RegistryKey>))
                //{
                //    var param = exprs.First().Value;
                //    var select_method = typeof(TData).SelectMethod_Enumerable();
                //    var uu = typeof(TData);
                //    var sd = typeof(TData).ToLambdaData();
                //    var select_expr = Expression.Call(select_method, param, sd);
                //    m_Lambda = Expression.Lambda(select_expr, parameters);
                //}
                this.m_ExpressionSaves[expr] = this.m_Lambda;
            }
            else
            {
                m_Lambda = lambda;
            }
            this.m_Parameters.Clear();
            this.m_DataTypeNames.Clear();
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
                        var igroup = find.Select(x=>x.Value).FirstOrDefault(x => x.IsGenericType == true && x.GetGenericTypeDefinition() == typeof(IGrouping<,>));
                        if(igroup == null)
                        {
                            var ie = expr.Type.GetGenericTypeDefinition().MakeGenericType(find.Values.ToArray());
                            var pp = Expression.Parameter(ie, expr.Name);
                            this.m_Parameters[expr.Name] = pp;
                        }
                        else
                        {
                            var ggs = igroup.GetGenericArguments();
                            var ie = expr.Type.GetGenericTypeDefinition().MakeGenericType(ggs);
                            var pp = Expression.Parameter(ie, expr.Name);
                            this.m_Parameters[expr.Name] = pp;
                        }
                        
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
                        ParameterExpression pp = null;
                        var type = this.m_GenericTypes.First()[pp1.Name];
                        if(type.IsGenericType==true && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            pp = Expression.Parameter(type.GetGenericArguments()[0], expr.Name);
                        }

                        //var pp = Expression.Parameter(type, expr.Name);
                        this.m_Parameters[expr.Name] = pp?? Expression.Parameter(type, expr.Name);
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
#if RegToEnd
                if (this.m_DataTypeNames.Any(x => x == (expr.Expression as ParameterExpression)?.Name)  || this.m_DataTypeNames.Count == 0)
#else
                if((expr.Expression as ParameterExpression)?.Name == this.m_DataTypeName || this.m_DataTypeName == "")
#endif
                {
                    if (exprs.Last().Value is ParameterExpression)
                    {
                        if (exprs.Last().Value.Type.IsGenericType == true && exprs.Last().Value.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                        {
                            var mes1 = exprs.Last().Value.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mes1.ElementAt(0));
                            this.m_ExpressionSaves[expr] = expr_member;
                        }
                        else if(node.Type.IsGenericType == true && node.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            var mem = exprs.First().Value.Type.GetMember(expr.Member.Name);
                            if(mem.Length == 0)
                            {
                                this.m_ExpressionSaves[expr] = node;
                            }
                            else
                            {
                                var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mem[0]);
                                this.m_ExpressionSaves[expr] = expr_member;
                            }
                        }
                        else if(Type.GetTypeCode(node.Type) == TypeCode.Object && node.Type.IsGenericType==true&&node.Type.GetGenericTypeDefinition()!=typeof(Nullable<>))
                        {
                            var mem = exprs.First().Value.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mem[0]);
                            this.m_ExpressionSaves[expr] = expr_member;
                        }
                        else if(exprs.ElementAt(0).Value.Type == typeof(RegistryKey))
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
                        else
                        {
                            var mem = exprs.First().Value.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value, mem[0]);
                            this.m_ExpressionSaves[expr] = expr_member;
                            //var expr_member = Expression.MakeMemberAccess(exprs.First().Value, expr.Member);
                            //this.m_ExpressionSaves[expr] = expr_member;
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
                    if(exprs.First().Value.Type == typeof(RegistryKey))
                    {
                        var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                        var left_args_1 = Expression.Constant(node.Member.Name);
                        var member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), exprs.ElementAt(0).Value, left_args_1);
                        this.m_ExpressionSaves[expr] = member;
                    }
                    else
                    {
                        var expr_member = Expression.MakeMemberAccess(exprs.First().Value, expr.Member);
                        this.m_ExpressionSaves[expr] = expr_member;
                    }
                    
                }

                this.m_Lastnode = expr;

                return expr;
            }
            return expr;
        }
#if RegToEnd
        Tuple<ParameterInfo, MethodInfo, List<string>> m_PP = null;
#else
        Tuple<ParameterInfo, MethodInfo, string> m_PP = null;
#endif
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
#if RegToEnd
                    var kkey = pp.GetGenericArguments()[0].GetGenericArguments().Select(x =>
                    {
                        string name = x.Name;
                        if (x.IsGenericType == true && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            name = x.GetGenericArguments()[0].Name;
                        }

                        return name;
                    }).Last();
                    var lambda = exprs.First().Value as LambdaExpression;
                    if (lambda != null)
                    {
                        this.m_GenericTypes.First()[kkey] = lambda.ReturnType;

                    }
#else
                    var kkey = pp.GetGenericArguments()[0].GetGenericArguments().Select(x =>
                    {
                        string name = x.Name;
                        if (x.IsGenericType == true && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            name = x.GetGenericArguments()[0].Name;
                        }

                        return name;
                    }).Last();
                    var lambda = exprs.First().Value as LambdaExpression;
                    if (lambda != null)
                    {
                        this.m_GenericTypes.First()[kkey] = lambda.ReturnType;
                        
                    }
#endif
                    this.m_Parameters.Clear();
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
            m_Lambda = null;
            return expr;
        }

        void PreparMethod1(Expression node, Expression[] args, MethodInfo method)
        {
            var lo = args.Select((x, idx) =>
            {
                bool bb = x.Type.IsGenericType == true && x.Type.GetGenericTypeDefinition() == typeof(RegQuery<>);
                return new { bb, idx };
            }).Where(x => x.bb == true);

            m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"PreparMethod1 {method?.Name}");

            if (method.IsGenericMethod == true)
            {
                this.m_GenericTypes.Push(new Dictionary<string, Type>());
                var dic = method.GetParameters();
                var dicc1 = method.GetGenericMethodDefinition();
                var dicc = method.GetGenericMethodDefinition().GetGenericArguments();
                if (lo.Any(x => x.bb == true) == false && this.m_Saves != null && this.m_Saves.ContainsKey(args[0]) == true)
                {
                    var newargs = new Expression[args.Length];
                    Array.Copy(args, newargs, args.Length);
                    newargs[0] = this.m_Saves[args[0]];
                    var types = newargs.GetTypes(method);
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (i == 0)
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = this.m_Saves[args[0]].Type.GetGenericArguments()[0];
                        }
                        else
                        {
                            if (types[i] == typeof(TData))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                            }
                            else
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = types[i];
                            }
                        }
                    }
                }
                else if (this.m_GenericTypes.Count > 1)
                {
                    var newargs = new Expression[args.Length];
                    Array.Copy(args, newargs, args.Length);
                    var types = newargs.GetTypes(method);
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (this.m_ExpressionSaves1.ContainsKey(args[i]) == true)
                        {
                            var aaa = this.m_ExpressionSaves1[args[i]];
                            if (aaa.IsGenericType == true && aaa.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                string ssssss = aaa.GetGenericArguments()[0].Name;
                                this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)[ssssss];
                            }
                            //this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)["TSource"];
                        }
                        else if (this.m_GenericTypes.ElementAt(1).ContainsKey("TSource") == true)
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)["TSource"];
                        }
                        else
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = types[i];
                        }
                    }
                }
                else if (this.m_Saves.Count > 0)
                {
                    var types = args.GetTypes(method);
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (this.m_Saves.ContainsKey(args[i]) == true)
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = this.m_Saves[args[i]].Type.GetGenericArguments()[0];
                        }
                        if (lo.Any(x => x.idx == i))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else if (i == 0 && types[i] == typeof(TData))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else
                        {
                            if (types[i] == typeof(TData))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                            }
                            else
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = types[i];
                            }
                        }
                    }
                }
                else
                {
                    var types = args.GetTypes(method);
                    for (int i=0; i<types.Length; i++)
                    {
                        if (lo.Any(x => x.idx == i))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else if (i==0 && types[i]==typeof(TData))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = types[i];
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

                if (return1 == typeof(IQueryable<>) && method.IsGenericMethod == true)
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
                            var ttypes = invoke.GetParameters().Select(x => x.ParameterType);
                            List<string> lo1 = new List<string>();
                            foreach(var oo in ttypes)
                            {
                                lo1.AddRange(oo.FindAllGeneric());
                            }
                            var invokepps = invoke.GetParameters().Select(x => x.ParameterType.Name);
                            List<string> argsnames = new List<string>();
                            Tuple<ParameterInfo, MethodInfo, List<string>> pp = null;
                            foreach(var oo in lo1)
                            {
                                if (this.m_GenericTypes.First().ContainsKey(oo) && this.m_GenericTypes.First()[oo] == typeof(RegistryKey))
                                {
                                    argsnames.Add(oo);
                                }
                                else
                                {
                                    argsnames.Add(oo);
                                }
                            }
                            m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, List<string>>(t4[i], invoke, argsnames);

                            //var invokepps = invoke.GetParameters().Select(x => x.ParameterType.Name).First();
                            //if (this.m_GenericTypes.First().ContainsKey(invokepps) && this.m_GenericTypes.First()[invokepps] == typeof(RegistryKey))
                            //{
                            //    m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, invokepps);
                            //}
                            //else
                            //{
                            //    m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, datatypename);
                            //}

                        }
                    }
                }
            }
        }

#if RegToEnd
#else
        void PreparMethod(Expression node, Expression[] args, MethodInfo method)
        {
            var lo = args.Select((x,idx) =>
            {
                bool bb = x.Type.IsGenericType==true&&x.Type.GetGenericTypeDefinition() == typeof(RegQuery<>);
                return new { bb, idx };
            }).Where(x=>x.bb==true);
            
            m_ExpressionSaves[node] = null;
            System.Diagnostics.Debug.WriteLine($"PreparMethod {method?.Name}");
            
            if (method.IsGenericMethod == true)
            {
                this.m_GenericTypes.Push(new Dictionary<string, Type>());
                var dic = method.GetParameters();
                var dicc1 = method.GetGenericMethodDefinition();
                var dicc = method.GetGenericMethodDefinition().GetGenericArguments();
                if (lo.Any(x => x.bb == true) == false && this.m_Saves!=null&& this.m_Saves.ContainsKey(args[0])==true)
                {
                    //var dsttype = method.GetParameters().Select(x => Tuple.Create(x.ParameterType, x.Name)).BuildType(null);
                    var sourcetype = this.m_Saves[args[0]].Type.GetGenericArguments()[0];
                    var dic1 = sourcetype.GetProperties();
                    for (int i = 0; i < dicc.Length; i++)
                    {
                        if(i==0)
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = sourcetype;
                        }
                        else
                        {
                            if (dic[i].ParameterType.IsGenericType == true && dic[i].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = dic[i].ParameterType.GetGenericArguments()[0];
                            }
                            else
                            {
                                var dui = dic[i].ParameterType.GetGenericTypeDefinition();
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
                }
                else if(this.m_GenericTypes.Count>1)
                {
                    var bt = this.m_ExpressionSaves1.ContainsKey(args[0]);
                    for (int i = 0; i < dicc.Length; i++)
                    {

                        if(this.m_ExpressionSaves1.ContainsKey(args[i]) == true)
                        {
                            var aaa = this.m_ExpressionSaves1[args[i]];
                            if(aaa.IsGenericType==true&&aaa.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                string ssssss = aaa.GetGenericArguments()[0].Name;
                                this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)[ssssss];
                            }
                            //this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)["TSource"];
                        }
                        else if(this.m_GenericTypes.ElementAt(1).ContainsKey("TSource") ==true)
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = this.m_GenericTypes.ElementAt(1)["TSource"];
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
                }
                else
                {
                    for (int i = 0; i < dicc.Length; i++)
                    {
                        if (lo.Any(x => x.idx == i))
                        {
                            this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                        }
                        else if (i == 0)
                        {
                            if (dic[i].ParameterType == typeof(IEnumerable<TData>))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                            }
                            else if (dic[i].ParameterType == typeof(IQueryable<TData>))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                            }
                            else if (dic[i].ParameterType == typeof(IQueryable<IGrouping<TData, TData>>))
                            {
                                this.m_GenericTypes.First()[dicc[i].Name] = typeof(RegistryKey);
                                this.m_GenericTypes.First()[dicc[i + 1].Name] = typeof(RegistryKey);
                                i = i + 1;
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
                            var invokepps = invoke.GetParameters().Select(x=>x.ParameterType.Name).First();
                            if(this.m_GenericTypes.First().ContainsKey(invokepps) && this.m_GenericTypes.First()[invokepps] == typeof(RegistryKey))
                            {
                                m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, invokepps);
                            }
                            else
                            {
                                m_PPs[args[i]] = Tuple.Create<ParameterInfo, MethodInfo, string>(t4[i], invoke, datatypename);
                            }
                            
                        }
                    }
                }
            }
        }
#endif

#if RegToEnd
        Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, List<string>>> m_PPs = new Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, List<string>>>();
#else
        Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>> m_PPs = new Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, string>>();
#endif

        DictionaryList<Expression, Expression> m_ExpressionSaves = new DictionaryList<Expression, Expression>();
        Dictionary<Expression, Type> m_ExpressionSaves1 = new Dictionary<Expression, Type>();
        Expression m_Lastnode = null;
        //Dictionary<Expression, string> GenericTypes = new Dictionary<Expression, string>();
        string LastMethodName = "";
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMethodCall {node.Method?.Name}");
            if (this.m_Saves!=null&&this.m_Saves.ContainsKey(node) == true)
            {
                var aaa = this.m_Saves[node];
                this.m_ExpressionSaves[node] = aaa;
                return node;
            }
            LastMethodName = node.Method.Name;
            if (node.Method.Name == "Any")
            {
                //ttypes1[1] = typeof(RegistryKey);
            }
#if RegToEnd
            this.PreparMethod1(node, node.Arguments.ToArray(), node.Method);
#else
            this.PreparMethod(node, node.Arguments.ToArray(), node.Method);
#endif
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
                                var ggs = expr.Method.GetGenericArguments();
                                if (expr.Method.Name == "SelectMany")
                                {
                                    //ttypes1[1] = typeof(RegistryKey);
                                }
                                var iuy = expr.Method.GetGenericArguments();
                                var expert = iuy.Except(ttypes1);
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
            var ttype = node.Type;
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
            else if(node.Type.IsGenericType==true&&node.Type.GetGenericTypeDefinition() == typeof(RegQuery<>))
            {
                var regquerytype = typeof(RegQuery<>).MakeGenericType(node.Type.GetGenericArguments()[0]);
                var regprovidertype = typeof(RegProvider<>).MakeGenericType(node.Type.GetGenericArguments()[0]);
                var pp_provider = regquerytype.GetProperty("Provider");
                var provider = pp_provider.GetValue(node.Value, null);
                var mems = regprovidertype.GetField("m_RegSource");
                var regsource = mems.GetValue(provider) as MethodCallExpression;
                expr1 = regsource;
            }

            this.m_ExpressionSaves[expr] = expr1??expr;
            this.m_Lastnode = expr;
            return expr;
        }

    }
}
