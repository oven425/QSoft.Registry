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
        public Expression Visit(Expression node, Type datatype)
        {
            this.m_DataType = datatype;
            return this.Visit(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitBinary");
            return base.VisitBinary(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitLambda T:{typeof(T)}");
            var pps = node.Parameters.Where(x => x.Type == this.m_DataType);
            var lambda = node as LambdaExpression;
            if (pps.Count() > 0)
            {
               
            }
            return base.VisitLambda(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitParameter");
            if(node.Type == this.m_DataType)
            {
                //return Expression.Parameter(typeof(RegistryKey), node.Name);
            }
            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMember");
            if (node.Type == this.m_DataType)
            {
                //return Expression.Parameter(typeof(RegistryKey), node.Name);
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitUnary");
            return base.VisitUnary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitMethodCall");
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitConstant");
            return base.VisitConstant(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            System.Diagnostics.Trace.WriteLine($"VisitBlock");
            return base.VisitBlock(node);
        }
    }
}
