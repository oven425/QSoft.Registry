using Microsoft.Win32;
using System;
using System.CodeDom;
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
        public IEnumerable<RegQueryConvert> Converts { set; get; }
        public string Fail { private set; get; }
        public Expression Visit(Expression node, Expression regfunc)
        {
            this.m_ExpressionSaves[node] = new Ex();
            this.m_RegSource = regfunc;
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value.Expr;
            return expr;
        }

        Dictionary<Expression, Expression> m_Saves;
        public Expression VisitA(Expression node, Expression regfunc, Dictionary<Expression, Expression> saves, bool lastquery=false)
        {
            this.m_Saves = saves;
            this.m_ExpressionSaves[node] = new Ex();
            this.m_ExpressionSaves[node].SourceExpr = node;
            this.m_RegSource = regfunc;
            Expression expr = this.Visit(node);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value.Expr;
            return expr;
        }

        public Expression Visit(MethodInfo method, params Expression[] nodes)
        {
            this.LastMethodName = method?.Name;
            System.Diagnostics.Debug.WriteLine($"Visit {method?.Name}");

            this.PreparMethod1(nodes[1], new Expression[] { nodes[0], nodes[1] }, method);
            Expression expr = this.Visit(nodes[1]);

            var exprs = this.m_ExpressionSaves.Clone(expr);
            expr = exprs.First().Value.Expr;
            return expr;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            this.m_ExpressionSaves[node] = new Ex();
            System.Diagnostics.Debug.WriteLine($"VisitBinary");

            var expr = base.VisitBinary(node) as BinaryExpression;
            if (this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                //var binary = this.m_MembersExprs.ToBinary(node, exprs);
                var binary = exprs.ToBinary(node, this.Converts);

                if (binary == null)
                {
                    binary = Expression.MakeBinary(node.NodeType, exprs.ElementAt(0).Value.Expr, exprs.ElementAt(1).Value.Expr);
                }
                
                this.m_ExpressionSaves[expr].Expr = binary;

                //this.m_MembersExprs.Clear();
                this.m_Lastnode = expr;
            }
            return expr;
        }

        
        void GetAllName(Expression src, Stack<MemberInfo> members)
        {
            var member = src as MemberExpression;
            if (member != null)
            {
                members.Push(member.Member);
                if (member.Expression is MemberExpression)
                {
                    GetAllName(member.Expression, members);
                }
            }
        }

        IEnumerable<MemberInfo> GetMembers(Expression src)
        {
            Stack<MemberInfo> members1 = new Stack<MemberInfo>();
            GetAllName(src, members1);

            return members1;
        }

        void ToNew(DictionaryList<Expression, Ex> exprs)
        {
            //var eex = exprs.Where(x => x.Key.Type != x.Value.Expr.Type).Where(x=>x.Value.SourceExpr is MemberExpression);
            var eex = exprs.Where(x => x.Key.Type != x.Value.Expr.Type);
            //var eex = exprs;
            foreach (var oo in eex)
            {
                var liu = GetMembers(oo.Value.SourceExpr);
                var pps = liu.Select(x => x as PropertyInfo);
                var reg_p = Expression.Parameter(typeof(RegistryKey), "subreg");
                var regss = pps.BuildSubKey(oo.Value, reg_p, this.Converts);

                if (regss.Item1 == null && regss.Item2 == null)
                {
                    if (oo.Value.SourceExpr.Type.IsIEnumerable() == true)
                    {
                        var temptype = oo.Value.SourceExpr.Type.GetGenericArguments()[0];
                        var select_method = temptype.SelectMethod_Enumerable();
                        var sd = temptype.ToLambdaData(this.Converts);
                        var aaaaa = Expression.Call(select_method, oo.Value.Expr, sd);
                        oo.Value.Expr = aaaaa;

                        //var select_method = oo.Value.SourceExpr.Type.SelectMethod_Enumerable();
                        //var sd = oo.Value.SourceExpr.Type.GetGenericArguments()[0].ToLambdaData(this.Converts);
                        //var aaaaa = Expression.Call(select_method, oo.Value.Expr, sd);
                    }
                    else if (oo.Value.SourceExpr.Type.IsIGrouping() == true)
                    {

                    }
                    else
                    {
                        if (oo.Value.Handled == false)
                        {
                            oo.Value.Expr = oo.Value.SourceExpr.Type.ToData(oo.Value.Expr, this.Converts);
                        }
                    }

                    //return;
                }
                else if (regss.Item1 == regss.Item2)
                {
                    var reg_p_assign = Expression.Assign(reg_p, regss.Item1 ?? reg_p);
                    var hr_p = Expression.Parameter(oo.Value.SourceExpr.Type, "hr");
                    var hr_p_assign = Expression.Assign(hr_p, oo.Value.SourceExpr.Type.DefaultExpr());

                    var membgervalue = Expression.Block(new ParameterExpression[] { reg_p, hr_p },
                        reg_p_assign,
                        hr_p_assign,
                        Expression.IfThen(Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                        Expression.Block(
                            Expression.Assign(hr_p, oo.Value.SourceExpr.Type.ToData(reg_p, this.Converts)),
                            reg_p.DisposeExpr())),
                        hr_p
                        );
                    oo.Value.Expr = membgervalue;
                }
                else
                {
                    var typecode = Type.GetTypeCode(oo.Value.SourceExpr.Type);

                    if (oo.Value.SourceExpr.Type.IsNullable() == true)
                    {
                        typecode = Type.GetTypeCode(oo.Value.SourceExpr.Type.GetGenericArguments()[0]);
                    }
                    if (typecode == TypeCode.Object)
                    {
                        if (oo.Value.Convert != null)
                        {
                            var methods = oo.Value.Convert.GetType().GetMethod("ConvertBack");
                            if (regss.Item1 == null)
                            {
                                var reg_p_assign = Expression.Assign(reg_p, regss.Item1 ?? oo.Value.Expr);
                                var hr_p = Expression.Parameter(oo.Value.SourceExpr.Type, "hr");
                                var hr_p_assign = Expression.Assign(hr_p, oo.Value.SourceExpr.Type.DefaultExpr());
                                var membgervalue = Expression.Block(new ParameterExpression[] { reg_p, hr_p },
                                    reg_p_assign,
                                    hr_p_assign,
                                    Expression.IfThen(Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                                        Expression.Assign(hr_p, Expression.Call(Expression.Constant(oo.Value.Convert), methods, regss.Item2))),
                                    hr_p
                                    );

                                oo.Value.Expr = membgervalue;
                            }
                            else
                            {
                                var reg_p_assign = Expression.Assign(reg_p, regss.Item1 ?? reg_p);
                                var hr_p = Expression.Parameter(oo.Value.SourceExpr.Type, "hr");
                                var hr_p_assign = Expression.Assign(hr_p, oo.Value.SourceExpr.Type.DefaultExpr());
                                var membgervalue = Expression.Block(new ParameterExpression[] { reg_p, hr_p },
                                    reg_p_assign,
                                    hr_p_assign,
                                    Expression.IfThen(Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                                    Expression.Block(
                                        Expression.Assign(hr_p, Expression.Call(Expression.Constant(oo.Value.Convert), methods, regss.Item2)),
                                        reg_p.DisposeExpr())),
                                    hr_p
                                    );
                                oo.Value.Expr = membgervalue;
                            }
                        }
                    }
                    else
                    {
                        var needdispose = Expression.Parameter(typeof(bool), "needdispose");
                        var reg_p_assign = Expression.Assign(reg_p, regss.Item1 ?? oo.Value.Expr);
                        var hr_p = Expression.Parameter(oo.Value.SourceExpr.Type, "hr");
                        var hr_p_assign = Expression.Assign(hr_p, oo.Value.SourceExpr.Type.DefaultExpr());
                        var membgervalue = Expression.Block(new ParameterExpression[] { reg_p, hr_p, needdispose },
                            Expression.Assign(needdispose, Expression.Constant(regss.Item3)),
                            reg_p_assign,
                            hr_p_assign,
                            Expression.Block(
                                Expression.Assign(hr_p, regss.Item2),
                                Expression.IfThen(Expression.MakeBinary(ExpressionType.AndAlso,
                                Expression.MakeBinary(ExpressionType.NotEqual, reg_p, Expression.Constant(null, typeof(RegistryKey))),
                                Expression.MakeBinary(ExpressionType.Equal, needdispose, Expression.Constant(true, typeof(bool)))),
                                    reg_p.DisposeExpr())),
                            hr_p
                            );
                        oo.Value.Expr = membgervalue;
                    }
                }
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitNew");
            this.m_ExpressionSaves[node] = new Ex();
            //if (this.m_MembersExprs.Count > 0)
            //{
            //    //this.m_MembersExprGroup.Push(this.m_MembersExprs);
            //    this.m_MembersExprs = new List<Tuple<Expression, MemberExpression>>();
            //}
            
            var expr = base.VisitNew(node) as NewExpression;
            //if (this.m_MembersExprs.Count > 0)
            //{
            //    //this.m_MembersExprGroup.Push(this.m_MembersExprs);
            //    this.m_MembersExprs = new List<Tuple<Expression, MemberExpression>>();
            //}

            //var exprs = this.m_ExpressionSaves.Clone(expr).ToDictionary(x => x.Key, x => x.Value.Expr);
            var exprs = this.m_ExpressionSaves.Clone(expr);
            //if (this.m_MembersExprGroup.Count > 0)
            {
                //this.ToNew(this.m_MembersExprGroup.Pop(), exprs);
                this.ToNew(exprs);
            }
           

            for (int i = 0; i < exprs.Count; i++)
            {
                //this.m_MembersExprs.Clear();
                ParameterExpression parameter = exprs.ElementAt(i).Value.Expr as ParameterExpression;
                if (parameter != null)
                {
                    if(this.m_DataTypeNames.Any(x=>x==parameter.Name))
                    {
                        if (parameter.Type.IsGenericType == true)
                        {
                            exprs[exprs.ElementAt(i).Key].Expr = this.m_Parameters[parameter.Name];
                        }
                        else
                        {
                            exprs[exprs.ElementAt(i).Key].Expr = this.m_Parameters[parameter.Name];

                        }
                    }
                    else
                    {
                        if (parameter.Type.IsGenericType == true)
                        {
                            var temptype = exprs.ElementAt(i).Key.Type.GetGenericArguments()[0];
                            var param = this.m_Parameters[parameter.Name];
                            var select_method = temptype.SelectMethod_Enumerable();
                            var sd = temptype.ToLambdaData(this.Converts);
                            var aaaaa = Expression.Call(select_method, param, sd);
                            exprs[exprs.ElementAt(i).Key].Expr = aaaaa;
                        }
                        else
                        {
                            var param = this.m_Parameters[parameter.Name];
                            if (param.Type == typeof(TData))
                            {
                                var todata = typeof(TData).ToData(param, this.Converts);
                                exprs[exprs.ElementAt(i).Key].Expr = todata;
                            }
                        }
                    }
                }
            }
            if (expr.Members == null)
            {
                var pps = exprs.Select(x =>
                {
                    Expression p = x.Value.Expr;
                    if(x.Key.Type == typeof(TData))
                    {
                        p = typeof(TData).ToData(x.Value.Expr as ParameterExpression, this.Converts);
                    }
                    return p;
                });
                var expr_new = Expression.New(expr.Constructor, pps);
                this.m_ExpressionSaves[expr].Expr = expr_new;
            }
            else
            {
                var con_pps = expr.Constructor.GetParameters();
                Expression expr_new = null;
                if (exprs.Select(x => x.Value.Expr.Type).Any(x => x.HaseRegistryKey()))
                {
                    var pps1 = con_pps.Replace(typeof(TData), typeof(RegistryKey));
                    var pps2 = con_pps.Zip(exprs.Select(x => x.Value), (pp, values) => new { pp, values })
                        .Select(x => Tuple.Create(x.values.Expr.Type, x.pp.Name));
                    //var exists = exprs.Where(x => Type.GetTypeCode(x.Value.Type) == TypeCode.Object && x.Value.Type != typeof(RegistryKey)).Select(x => x.Value.Type);
                    //var anyt = pps2.BuildType(exists);
                    var anyt = pps2.BuildType();
                    var po = anyt.GetConstructors()[0].GetParameters();
                    var pps = anyt.GetProperties();
                    expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Select(x => x.Value.Expr), anyt.GetProperties());
                    //expr_new = expr;
                }
                else
                {
                    expr_new = Expression.New(expr.Constructor, exprs.Select(x => x.Value.Expr), expr.Members);
                }

                this.m_ExpressionSaves[expr].Expr = expr_new;
            }


            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMemberInit");
            this.m_ExpressionSaves[node] = new Ex();
            var expr = base.VisitMemberInit(node) as MemberInitExpression;

            if(this.m_Lastnode != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);

                
                var bindings = exprs.Skip(1).Select((x,i) =>
                {
                    if(x.Value.Expr.Type== typeof(RegistryKey))
                    {
                        x.Value.Expr = x.Value.SourceExpr.Type.ToData(x.Value.Expr, this.Converts);
                    }
                    var binding = Expression.Bind(expr.Bindings[i].Member, x.Value.Expr);
                    return binding;
                });
                this.m_ExpressionSaves[expr].SourceExpr = expr;
                switch (this.LastMethodName)
                {
                    case "Update":
                    case "InsertOrUpdate":
                        {
                            var aaaa = exprs.Skip(1);
                            var typecode = Type.GetTypeCode(aaaa.First().Value.Expr.Type);

                            var ll = expr.Bindings.Select(x => x.Member as PropertyInfo).ToList();
                            var p = exprs.First().Key.Type.PropertyName();
                            List<Tuple<Type, string>> typedefines = new List<Tuple<Type, string>>();
                            for(int i=0; i<ll.Count; i++)
                            {
                                if(Type.GetTypeCode(ll[i].PropertyType) == TypeCode.Object)
                                {
                                    var ooii = exprs.FirstOrDefault(x => x.Key.Type == ll[i].PropertyType);
                                    var iuy = p.Item2[ll[i]];
                                    typedefines.Add(Tuple.Create(ooii.Value.Expr.Type, iuy??ll[i].Name));
                                }
                                else
                                {
                                    typedefines.Add(Tuple.Create(ll[i].PropertyType, ll[i].Name));
                                }
                            }
                            var anyt = typedefines.BuildType();
                            //var anyt = expr.Bindings.Select(x => x.Member as PropertyInfo).BuildType();

                            var pps = anyt.GetProperties();
                            var expr_new = Expression.New(anyt.GetConstructors()[0], exprs.Skip(1).Select(x => x.Value.Expr), anyt.GetProperties());
                            this.m_ExpressionSaves[expr].Expr = expr_new;
                            this.m_ExpressionSaves[expr].Handled = true;
                        }
                        break;
                    default:
                        {
                            var expr_memberinit = Expression.MemberInit(exprs.First().Value.Expr as NewExpression, bindings);
                            this.m_ExpressionSaves[expr].Expr = expr_memberinit;
                        }
                        break;
                }

            }
            this.m_Lastnode = expr;
            return expr;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            if(this.m_ExpressionSaves.ContainsKey(node.Expression) == false)
            {
                this.m_ExpressionSaves[node.Expression] = new Ex();
            }
            this.m_ExpressionSaves[node.Expression].SourceExpr = node.Expression;
            //this.m_ExpressionSaves[node.Expression] = null;
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
        List<string> m_DataTypeNames = new List<string>();
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            this.m_ExpressionSaves[node] = new Ex();
            if (this.m_ExpressionSaves1.Count == 0)
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
               
                if (lambda.ReturnType!= typeof(string) && lambda.ReturnType.GetInterfaces().Any(x => x == typeof(System.Collections.IEnumerable)))
                {
                    foreach(var oo in exprs)
                    {
                        var liu = GetMembers(oo.Value.SourceExpr);
                        var pps = liu.Select(x => x as PropertyInfo);
                        var reg_p = Expression.Parameter(typeof(RegistryKey), "subreg");
                        var regss = pps.BuildSubKey(oo.Value, reg_p, this.Converts);
                        if(regss.Item1 != null)
                        {
                            var subreg_p1 = Expression.Parameter(typeof(RegistryKey), "subreg");
                            var subobjexpr1 = lambda.Type.GetGenericArguments()[0].ToData(parameters[0], this.Converts);
                            var subobjexpr2 = subobjexpr1.PropertyExpr("Rams");
                            oo.Value.Expr = subobjexpr2;
                        }
                    }
                    m_Lambda = Expression.Lambda(exprs.First().Value.Expr, parameters);
                    //var memberexpr = exprs.FirstOrDefault().Value.SourceExpr as MemberExpression;
                    //var subkeyname = memberexpr.Member.Name;
                    //var opensubkey = typeof(RegistryKey).GetMethod("OpenSubKey", new[] { typeof(string) });

                    //var getsubkeyexpr = Expression.Call(parameters[0], opensubkey, Expression.Constant(subkeyname));

                    //var opensubkeys_expr = Expression.Call(typeof(RegQueryEx).GetMethod("OpenSubKeys"), getsubkeyexpr);
                    //var opensubkeys_p = Expression.Parameter(typeof(List<RegistryKey>), "subkeys");
                    //var disposesubeys_expr = Expression.Call(typeof(RegQueryEx).GetMethod("DisposeSubkeys"), opensubkeys_p);

                    //var aaa = typeof(Enumerable).GetMethods().Where(x => x.Name == "Select").ElementAt(0);
                    //aaa = aaa.MakeGenericMethod(typeof(RegistryKey), lambda.Type.GetGenericArguments()[0]);
                    
                    //var subreg_p = Expression.Parameter(typeof(RegistryKey), "subreg");
                    //var subobjexpr = lambda.Type.GetGenericArguments()[0].ToData(subreg_p, this.Converts);
                    //var selectexpr = Expression.Call(aaa, opensubkeys_p, Expression.Lambda(subobjexpr, subreg_p));
                    //var bbb = typeof(Enumerable).GetMethods().Where(x => x.Name == "Select").ElementAt(0);
                    //bbb = bbb.MakeGenericMethod(lambda.Type.GetGenericArguments()[0], node.Type.GetGenericArguments()[1]);
                    //var selectexpr1 = Expression.Call(bbb, subobjexpr, node);
                    //var tolist = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(node.ReturnType);
                    //var tolist_expr = Expression.Call(tolist, selectexpr1);
                    //var return_expr = Expression.Parameter(tolist_expr.Type, "hr");
                    //Expression block_expr = Expression.Block(new[] { return_expr, opensubkeys_p },
                    //    Expression.Assign(opensubkeys_p, opensubkeys_expr),
                    //    Expression.Assign(return_expr, tolist_expr),
                    //    disposesubeys_expr,
                    //    return_expr
                    //    );
                    //var tolist = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(lambda.Type.GetGenericArguments()[0]);
                    //var tolist_expr = Expression.Call(tolist, selectexpr);
                    //var return_expr = Expression.Parameter(tolist_expr.Type, "hr");
                    //Expression block_expr = Expression.Block(new[] { return_expr, opensubkeys_p },
                    //    Expression.Assign(opensubkeys_p, opensubkeys_expr),
                    //    Expression.Assign(return_expr, tolist_expr),
                    //    disposesubeys_expr,
                    //    return_expr
                    //    );
                    //m_Lambda = Expression.Lambda(block_expr, parameters);
                }
                else if(lambda.ReturnType.IsNullable()==false && lambda.ReturnType.IsGenericType == true&& (lambda.Type.GetGenericTypeDefinition()==typeof(Func<,>)||lambda.Type.GetGenericTypeDefinition() == typeof(Func<,,>)))
                {
                    Dictionary<Type, Type> changes = new Dictionary<Type, Type>();
                    //changes[typeof(TData)] = typeof(RegistryKey);
                    foreach(var oo in exprs)
                    {
                        changes[oo.Key.Type] = oo.Value.Expr.Type;
                    }
                    var functype = lambda.Type.ChangeType(changes);
                    //var ggs = lambda.Type.GetGenericTypeDefinition().GetGenericArguments();
                    //ggs[0] = parameters[0].Type;
                    //ggs[1] = parameters[1].Type;
                    //ggs[2] = exprs.First().Value.Type;
                    List<Type> ggs = new List<Type>();
                    ggs.AddRange(parameters.Select(x => x.Type));
                    ggs.Add(exprs.First().Value.Expr.Type);
                    var functype1 = lambda.Type.GetGenericTypeDefinition().MakeGenericType(ggs.ToArray());
                    //var zz = functype.GetGenericArguments().Zip(functype1.GetGenericArguments(), (x, y) => new { x, y });
                    //foreach(var oo in zz)
                    //{
                    //    if(oo.x != oo.y)
                    //    {

                    //    }
                    //}
                    m_Lambda = Expression.Lambda(functype1, exprs.First().Value.Expr, parameters);
                }
                //else if(this.m_MembersExprs.Count > 0)
                else
                {
                    ToNew(exprs);
                    //m_Lambda = Expression.Lambda(exprs1.First().Value, parameters);
                    //this.m_MembersExprs.Clear();
                }
                if(m_Lambda == null)
                {
                    m_Lambda = Expression.Lambda(exprs.First().Value.Expr, parameters);
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
                this.m_ExpressionSaves[expr].SourceExpr = expr;
                this.m_ExpressionSaves[expr].Expr = this.m_Lambda;
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
            if(this.m_ExpressionSaves.ContainsKey(node) == false)
            {
                m_ExpressionSaves[node] = new Ex() { SourceExpr = node };
            }
            //m_ExpressionSaves[node].SourceExpr = node;
            //m_ExpressionSaves[node] = new Ex() { SourceExpr = node};
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
                            m_ExpressionSaves[expr].Expr = pp;
                            m_ExpressionSaves[expr].SourceExpr = expr;
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
                this.m_ExpressionSaves[expr].Expr = this.m_Parameters[expr.Name];
                this.m_ExpressionSaves[expr].SourceExpr = expr;
            }
            this.m_Lastnode = expr;
            return expr;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if(this.m_ExpressionSaves.ContainsKey(node) == false)
            {
                this.m_ExpressionSaves[node] = new Ex() { SourceExpr = node };
            }
            this.m_ExpressionSaves[node].Expr = node.Expression == null?node:null;
            var ttyp = node.Type;
            System.Diagnostics.Debug.WriteLine($"VisitMember {node.Member.Name}");

            var expr = base.VisitMember(node) as MemberExpression;
            if (this.LastMethodName == "DefaultIfEmpty")
            {

            }
            if (this.m_Lastnode != null && expr.Expression != null)
            {
                var exprs = this.m_ExpressionSaves.Clone(expr);
                if (this.m_DataTypeNames.Any(x => x == (expr.Expression as ParameterExpression)?.Name)  || this.m_DataTypeNames.Count == 0)
                {
                    if (exprs.Last().Value.Expr is ParameterExpression)
                    {
                        if (exprs.Last().Value.Expr.Type.IsGenericType == true && exprs.Last().Value.Expr.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                        {
                            var mes1 = exprs.Last().Value.Expr.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, mes1.ElementAt(0));
                            this.m_ExpressionSaves[expr].Expr = expr_member;
                        }
                        else if(expr.Type.IsGenericType == true && expr.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            var mem = exprs.First().Value.Expr.Type.GetMember(expr.Member.Name);
                            if(mem.Length == 0)
                            {
                                this.m_ExpressionSaves[expr].Expr = expr;
                            }
                            else
                            {
                                var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, mem[0]);
                                this.m_ExpressionSaves[expr].Expr = expr_member;
                            }
                        }
                        
                        else if(exprs.ElementAt(0).Value.Expr.Type == typeof(RegistryKey))
                        {
                            //this.m_MembersExprs.Add(Tuple.Create(exprs.ElementAt(0).Value, expr));
                            //bool add = false;
                            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                            var typecode = Type.GetTypeCode(expr.Type);
                            var attr = expr.Member.GetCustomAttributes(true).FirstOrDefault();
                            Expression member = null;
                            Expression left_args_1 = null;
                            if (attr is RegIgnore)
                            {
                                this.Fail = $"{expr.Member.Name} is ignored, please do not use";
                            }
                            else if (attr is RegPropertyName)
                            {
                                if(typecode == TypeCode.Object)
                                {
                                    member = exprs.ElementAt(0).Value.Expr;
                                    left_args_1 = Expression.Constant((attr as RegPropertyName).Name);
                                    
                                    //member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), exprs.ElementAt(0).Value, left_args_1);
                                    //add = true;
                                }
                                else
                                {
                                    left_args_1 = Expression.Constant((attr as RegPropertyName).Name);
                                    member = exprs.ElementAt(0).Value.Expr;
                                    //member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(expr.Type), exprs.ElementAt(0).Value.Expr, left_args_1);
                                }
                            }
                            else if (attr is RegSubKeyName)
                            {
                                var subkeyname = attr as RegSubKeyName;
                                member = subkeyname.ToExpression(expr.Type, exprs.First().Value.Expr);
                            }
                            if(member ==null&&left_args_1 == null)
                            {
                                if(typecode == TypeCode.Object && expr.Type.IsNullable()==false)
                                {
                                    //add = true;
                                    member = exprs.ElementAt(0).Value.Expr;
                                }
                                else
                                {
                                    left_args_1 = Expression.Constant(expr.Member.Name);
                                    member = exprs.ElementAt(0).Value.Expr;
                                    //member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(expr.Type), exprs.ElementAt(0).Value.Expr, left_args_1);
                                }
                            }
                            
                            this.m_ExpressionSaves[expr].Convert = this.Converts?.FirstOrDefault(x => x.CanConvert(node.Type));
                            this.m_ExpressionSaves[expr].Expr = member;
                            this.m_ExpressionSaves[expr].SourceExpr = expr;
                        }
                        else if (Type.GetTypeCode(expr.Type) == TypeCode.Object && expr.Type.IsNullable()==false)
                        {
                            var mem = exprs.First().Value.Expr.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, mem[0]);
                            this.m_ExpressionSaves[expr].Expr = expr_member;
                        }
                        else
                        {
                            var mem = exprs.First().Value.Expr.Type.GetMember(expr.Member.Name);
                            var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, mem[0]);
                            this.m_ExpressionSaves[expr].Expr = expr_member;
                        }
                    }
                    else
                    {
                        var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, expr.Member);
                        this.m_ExpressionSaves[expr].Expr = expr_member;
                    }
                }
                else
                {
                    if(exprs.First().Value.Expr.Type == typeof(RegistryKey))
                    {
                        var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name && x.IsGenericMethod == true);
                        var attr = expr.Member.GetCustomAttributes(true).FirstOrDefault();
                        var typecode = Type.GetTypeCode(expr.Type);
                        Expression member = null;
                        Expression left_args_1 = null;
                        if (attr is RegIgnore)
                        {
                            this.Fail = $"{expr.Member.Name} is ignored, please do not use";
                        }
                        else if (attr is RegPropertyName)
                        {
                            left_args_1 = Expression.Constant((attr as RegPropertyName).Name);
                            member = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(expr.Type), exprs.ElementAt(0).Value.Expr, left_args_1);
                        }
                        else if (attr is RegSubKeyName)
                        {
                            var subkeyname = attr as RegSubKeyName;
                            member = subkeyname.ToExpression(expr.Type, exprs.First().Value.Expr);
                        }
                        if (member == null && left_args_1 == null)
                        {
                            left_args_1 = Expression.Constant(expr.Member.Name);
                            if(typecode == TypeCode.Object)
                            {
                                member = exprs.ElementAt(0).Value.Expr;
                            }
                            else
                            {
                                member = exprs.ElementAt(0).Value.Expr;
                            }
                        }

                        //member = exprs.ElementAt(0).Value;
                        //if(this.m_MembersExprs.Count > 0)
                        //{
                        //    this.m_MembersExprs.Add(Tuple.Create(exprs.ElementAt(0).Value.Expr, expr));
                        //}
                        this.m_ExpressionSaves[expr].Convert = this.Converts.FirstOrDefault(x => x.CanConvert(node.Type));
                        this.m_ExpressionSaves[expr].Expr = member;
                        this.m_ExpressionSaves[expr].SourceExpr = expr;
                    }
                    else if(exprs.First().Value.Expr.Type.HaseRegistryKey() == true)
                    {
                        var mes1 = exprs.Last().Value.Expr.Type.GetMember(expr.Member.Name);
                        var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, mes1.ElementAt(0));
                        this.m_ExpressionSaves[expr].Expr = expr_member;
                    }
                    else
                    {
                        var expr_member = Expression.MakeMemberAccess(exprs.First().Value.Expr, expr.Member);
                        this.m_ExpressionSaves[expr].Expr = expr_member;
                    }
                    
                }

                this.m_Lastnode = expr;

                return expr;
            }
            return expr;
        }
        Tuple<ParameterInfo, MethodInfo, List<string>> m_PP = null;

        Stack<Dictionary<string, Type>> m_GenericTypes = new Stack<Dictionary<string, Type>>();
        protected override Expression VisitUnary(UnaryExpression node)
        {
            m_PP = null;
            if (m_PPs.ContainsKey(node) == true)
            {
                m_PP = m_PPs[node];
            }
            this.m_ExpressionSaves[node] = new Ex();
            if (this.m_ExpressionSaves1.ContainsKey(node) == true)
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
                    var kkey = pp.GetGenericArguments()[0].GetGenericArguments().Select(x =>
                    {
                        string name = x.Name;
                        if (x.IsGenericType == true && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            name = x.GetGenericArguments()[0].Name;
                        }

                        return name;
                    }).Last();
                    var lambda = exprs.First().Value.Expr as LambdaExpression;
                    if (lambda != null)
                    {
                        this.m_GenericTypes.First()[kkey] = lambda.ReturnType;

                    }

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
            if(expr.NodeType == ExpressionType.Convert)
            {
                ToNew(exprs);
            }
            UnaryExpression unary = Expression.MakeUnary(node.NodeType, exprs.First().Value.Expr, result_type);
            this.m_ExpressionSaves[expr].Expr = unary;
            
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

            m_ExpressionSaves[node] = new Ex();
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
                        else if (lo.Any(x => x.idx == i))
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
#pragma warning disable CS0219 // 已指派變數 'pp'，但是從未使用過它的值。
                            Tuple<ParameterInfo, MethodInfo, List<string>> pp = null;
#pragma warning restore CS0219 // 已指派變數 'pp'，但是從未使用過它的值。
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

        Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, List<string>>> m_PPs = new Dictionary<Expression, Tuple<ParameterInfo, MethodInfo, List<string>>>();

        DictionaryList<Expression, Ex> m_ExpressionSaves = new DictionaryList<Expression, Ex>();

        Dictionary<Expression, Type> m_ExpressionSaves1 = new Dictionary<Expression, Type>();
        Expression m_Lastnode = null;
        string LastMethodName = "";
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Debug.WriteLine($"VisitMethodCall {node.Method?.Name}");
            if (this.m_Saves!=null&&this.m_Saves.ContainsKey(node) == true)
            {
                var aaa = this.m_Saves[node];
                this.m_ExpressionSaves[node] = new Ex();
                this.m_ExpressionSaves[node].Expr = aaa;
                this.m_ExpressionSaves[node].SourceExpr = node;
                return node;
            }
            LastMethodName = node.Method.Name;


            this.PreparMethod1(node, node.Arguments.ToArray(), node.Method);

            var expr = base.VisitMethodCall(node) as MethodCallExpression;

            var exprs1 = this.m_ExpressionSaves.Clone(expr);
            
            var ttypes1 = exprs1.Select(x => x.Value.Expr).GetTypes(expr.Method);
            Expression methodcall = null;
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
                            var sd = typeof(TData).ToSelectData(this.Converts);
                            var select = typeof(TData).SelectMethod();
                            var selectexpr = Expression.Call(select, this.m_RegSource, sd);

                            var args = exprs1.Select(x => x.Value).ToList();
                            args[0].Expr = selectexpr;
                            methodcall = Expression.Call(expr.Method, args.Select(x=>x.Expr));
                        }
                        break;
                    default:
                        {
                            if(expr.Method.IsGenericMethod == true)
                            {
                                if (expr.Method.Name == "SelectMany")
                                {
                                    //ttypes1[1] = typeof(RegistryKey);
                                }
                                var bubkey = exprs1.Values.ToStaticMethodCall(expr.Method, exprs1.Select(x => x.Value.Expr), this.Converts);
                                if(bubkey != null)
                                {
                                    methodcall = bubkey;
                                }
                                else
                                {
                                    methodcall = Expression.Call(expr.Method.GetGenericMethodDefinition().MakeGenericMethod(ttypes1), exprs1.Select(x => x.Value.Expr));
                                }
                            }
                            else
                            {
                                var reg_p = Expression.Parameter(typeof(RegistryKey), "reg_p");
                                //var bubkey = this.m_MembersExprs.ToMethodCall(expr.Method, exprs1.Select(x => x.Value.Expr));
                                var bubkey1 = exprs1.Values.ToMethodCall(expr.Method, exprs1.Select(x => x.Value.Expr), this.Converts);
                                var bubkey = exprs1.FirstOrDefault().Value.ToMethodCall(expr.Method, exprs1.Skip(1).Select(x => x.Value.Expr), this.Converts);
                                if (bubkey != null)
                                {
                                    methodcall = bubkey;
                                }
                                else
                                {
                                    methodcall = Expression.Call(expr.Method, exprs1.Select(x => x.Value.Expr));
                                }
                                
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
                    var reg_p = Expression.Parameter(typeof(RegistryKey), "reg_p");
                    //var sssou = ((exprs1.FirstOrDefault().Value.SourceExpr as MemberExpression).Member as PropertyInfo);
                    //var aa = new List<PropertyInfo>() { sssou }.BuildSubKey(exprs1.FirstOrDefault().Value, reg_p);
                    var bubkey = exprs1.FirstOrDefault().Value.ToMethodCall(expr.Method, exprs1.Skip(1).Select(x => x.Value.Expr), this.Converts);
                    //var bubkey = this.m_MembersExprs.ToMethodCall(expr.Method, exprs1.Skip(1).Select(x => x.Value.Expr));
                    if (bubkey != null)
                    {
                        methodcall = bubkey;
                    }
                    else
                    {
                        methodcall = Expression.Call(exprs1.First().Value.Expr, expr.Method, exprs1.Skip(1).Select(x => x.Value.Expr));
                    }
                    
                }
                
            }
            if(expr.Method.IsGenericMethod == true && this.m_GenericTypes.Count > 0)
            {
                this.m_GenericTypes.Pop();
            }

            this.m_ExpressionSaves[expr].SourceExpr = expr;
            this.m_ExpressionSaves[expr].Expr = methodcall;
            this.m_Lastnode = expr;
            return expr;

        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.m_ExpressionSaves[node] = new Ex();
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
                }
            }
            
            if (node.Type == typeof(RegQuery<TData>))
            {
                var reguqery = node.Value as RegQuery<TData>;
                var regprovider = reguqery?.Provider as RegProvider<TData>;
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

            this.m_ExpressionSaves[expr].Expr = expr1??expr;
            this.m_ExpressionSaves[expr].SourceExpr = expr;
            this.m_Lastnode = expr;
            return expr;
        }
    }
}
