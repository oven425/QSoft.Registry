using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QSoft.Registry
{
    class RegExpressionVisitor: ExpressionVisitor
    {
        Type m_DataType;
        Expression m_New = null;
        IQueryable<RegistryKey> m_RegKeys = null;
        public Expression Visit(Expression node, Type datatype, IQueryable<RegistryKey> regkeys)
        {
            this.m_RegKeys = regkeys;
            //this.m_DataType = datatype;
            Expression expr = this.Visit(node);

            if(expr != null)
            {
                expr = this.m_New;
            }
            return expr;
        }

        List<Expression> m_Binarys = new List<Expression>();
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Type left = node.Left.GetType();
            Type right = node.Right.GetType();
            System.Diagnostics.Trace.WriteLine($"VisitBinary");
            

            var expr = base.VisitBinary(node);

            if (this.m_Member2Regs.Count >0)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Member2Regs[0], node.Right);
                m_Binarys.Add(binary);
                this.m_Member2Regs.Clear();
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
            var expr = base.VisitNew(node);
            if (this.m_Member2Regs.Count > 0)
            {


                m_NewExpression = Expression.New(node.Constructor, this.m_Member2Regs, node.Members);
                this.m_Member2Regs.Clear();
            }


            return expr;
        }


        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var expr = base.VisitMemberInit(node);
            var type = node.Bindings[0].GetType();
            return expr;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var regexs = typeof(RegistryKey).GetMethods().Where(x => "SetValue" == x.Name);
            
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
                if(this.parameter == null)
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], lambda.Parameters);
                }
                else
                {
                    m_Lambda = Expression.Lambda(this.m_Binarys[0], this.parameter);
                }
            }
            else if(m_Member2Regs.Count>0)
            {
                m_Lambda = Expression.Lambda(this.m_Member2Regs[0], this.parameter);
                m_Member2Regs.Clear();
            }
            else if(this.m_MethodCall_Member != null)
            {
                m_Lambda = Expression.Lambda(this.m_MethodCall_Member, this.parameter);
                this.m_MethodCall_Member = null;
            }
            else if(this.m_NewExpression != null)
            {
                m_Lambda = Expression.Lambda(this.m_NewExpression, this.parameter);
                this.m_NewExpression = null;
            }
            else
            {
                m_Lambda = lambda;
            }
            this.parameter = null;
            return expr;
        }

        ParameterExpression parameter = null;
        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitParameter {node.Type.Name}");
            if(node.Type == this.m_DataType)
            {
                if(parameter == null)
                {
                    parameter = Expression.Parameter(typeof(RegistryKey), node.Name);
                }
                
            }
            else
            {
                //parameter = node;
            }
            
            return base.VisitParameter(node);
        }

        List<Expression> m_Member2Regs = new List<Expression>();
        protected override Expression VisitMember(MemberExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMember {node.Member.Name}");
            var expr = base.VisitMember(node);
            if (parameter != null)
            {
                var left_args_1 = Expression.Constant(node.Member.Name);
                var left_args_0 = parameter;
                var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
                this.m_Member2Regs.Add(Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), left_args_0, left_args_1));
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
                this.m_Binarys.Clear();
                this.m_Lambda = null;
            }
            
            return expr;
        }

        MethodCallExpression m_MethodCall_Member = null;
        MethodCallExpression m_MethodCall = null;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall {node.Method?.Name}");
            var expr = base.VisitMethodCall(node) as MethodCallExpression;
            if(this.m_Unary != null)
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
                    this.m_ConstantExpression_Source = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                    methodcall_param_0 = this.m_ConstantExpression_Source;
                }
                else
                {
                    methodcall_param_0 = m_MethodCall;
                }
                
                
                var pps = node.Method.GetParameters();
                var methods = expr.Method.ReflectedType.GetMethods().Where(x=>x.Name == expr.Method.Name&& x.GetParameters().Length == expr.Method.GetParameters().Length);
                var tts = expr.Method.GetGenericArguments();
                var tts1 = new Type[tts.Length];
                for(int i=0; i<tts.Length; i++)
                {
                    if(tts.ElementAt(i) == this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else
                    {
                        tts1[i] = tts.ElementAt(i);
                    }
                }
                
                if (this.m_IsRegQuery)
                {
                    this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), methodcall_param_0, m_Unary);
                }
                else
                {
                    var mmme = methods.ElementAt(0).MakeGenericMethod(tts);
                    this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), methodcall_param_0, m_Unary);
                }
                
                //var dds = m_Regs.Provider.Execute(this.m_MethodCall);
                this.m_Unary = null;
                this.m_IsRegQuery = false;
                this.m_New = this.m_MethodCall;
            }
            else if (m_Member2Regs.Count >0)
            {
                this.m_MethodCall_Member = Expression.Call(node.Method, m_Member2Regs[0]);
                m_Member2Regs.Clear();
            }
            else
            {
                Expression methodcall_param_0 = null;
                if (this.m_IsRegQuery == true)
                {
                    //this.m_IsRegQuery = false;
                    methodcall_param_0 = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                }
                else
                {
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

                //if (this.m_IsRegQuery)
                {
                    var aaa = methods.ElementAt(0).MakeGenericMethod(tts1);
                    this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(tts1), methodcall_param_0);
                }
                this.m_New = this.m_MethodCall;
            }

            return expr;
        }


        bool m_IsRegQuery = false;
        ConstantExpression m_ConstantExpression_Source = null;
        protected override Expression VisitConstant(ConstantExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitConstant");
            var expr = base.VisitConstant(node);
            if (node.Type.Name == "RegQuery`1")
            {
                m_IsRegQuery = true;
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
                if (this.m_RegKeys == null)
                {
                    m_ConstantExpression_Source = null;
                    //List<RegistryKey> regs = new List<RegistryKey>();
                    //var subkeynames = this.m_RegKey.GetSubKeyNames();

                    //foreach (var subkeyname in subkeynames)
                    //{
                    //    regs.Add(this.m_RegKey.OpenSubKey(subkeyname));
                    //}

                    //m_Regs = regs.AsQueryable();
                }

            }
            return expr;
        }

    }
}
