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

        List<BinaryExpression> m_Binarys = new List<BinaryExpression>();
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Type left = node.Left.GetType();
            Type right = node.Right.GetType();
            System.Diagnostics.Trace.WriteLine($"VisitBinary");
            
            //return base.VisitBinary(node);


            var expr = base.VisitBinary(node);

            if (this.m_Member2Reg != null)
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Member2Reg, node.Right);
                m_Binarys.Add(binary);
                this.m_Member2Reg = null;
            }
            else
            {
                var binary = Expression.MakeBinary(node.NodeType, m_Binarys[0], m_Binarys[1]);
                m_Binarys.Clear();
                m_Binarys.Add(binary);
            }

            return expr;
        }

        LambdaExpression m_Lambda = null;
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitLambda T:{typeof(T)}");

            var expr = base.VisitLambda(node);
            var pps = node.Parameters.Where(x => x.Type == this.m_DataType);
            var lambda = node as LambdaExpression;
            Type type = lambda.Body.GetType();

            if(this.m_Binarys.Count > 0)
            {
                m_Lambda = Expression.Lambda(this.m_Binarys[0], this.parameter);
            }
            else if(this.m_MethodCall != null)
            {
                m_Lambda = Expression.Lambda(this.m_MethodCall, this.parameter);
                this.m_MethodCall = null;
            }

            return expr;
        }

        ParameterExpression parameter = null;
        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitParameter");
            if(node.Type == this.m_DataType&& parameter==null)
            {
                parameter = Expression.Parameter(typeof(RegistryKey), node.Name);
            }
            return base.VisitParameter(node);
        }

        MethodCallExpression m_Member2Reg = null;
        protected override Expression VisitMember(MemberExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMember");
            var expr = base.VisitMember(node);
            if (parameter != null)
            {
                var left_args_1 = Expression.Constant(node.Member.Name);
                var left_args_0 = parameter;
                var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
                m_Member2Reg = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(node.Type), left_args_0, left_args_1);
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
                this.m_Lambda = null;
            }
            


            return expr;
        }

        MethodCallExpression m_MethodCall = null;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall");
            var expr = base.VisitMethodCall(node);
            if(this.m_Unary != null)
            {
                var methodcall_param_0 = Expression.Constant(this.m_RegKeys, typeof(IQueryable<RegistryKey>));
                
                var pps = node.Method.GetParameters();
                var methods = node.Method.ReflectedType.GetMethods().Where(x=>x.Name == node.Method.Name&& x.GetParameters().Length == node.Method.GetParameters().Length);
                this.m_MethodCall = Expression.Call(methods.ElementAt(0).MakeGenericMethod(typeof(RegistryKey)), methodcall_param_0, m_Unary);
                //var dds = m_Regs.Provider.Execute(this.m_MethodCall);
                this.m_Unary = null;
                this.m_New = this.m_MethodCall;
            }
            if (m_Member2Reg != null)
            {
                this.m_MethodCall = Expression.Call(node.Method, m_Member2Reg);
                m_Member2Reg = null;
            }

            return expr;
        }

        

        protected override Expression VisitConstant(ConstantExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitConstant");
            if (node.Type.Name == "RegQuery`1")
            {
                
                this.m_DataType = node.Type.GetGenericArguments().FirstOrDefault();
                if (this.m_RegKeys == null)
                {
                    //List<RegistryKey> regs = new List<RegistryKey>();
                    //var subkeynames = this.m_RegKey.GetSubKeyNames();

                    //foreach (var subkeyname in subkeynames)
                    //{
                    //    regs.Add(this.m_RegKey.OpenSubKey(subkeyname));
                    //}

                    //m_Regs = regs.AsQueryable();
                }

            }
            return base.VisitConstant(node);
        }

    }
}
