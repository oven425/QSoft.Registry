using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqFileSystemProvider
{
    public class FileSystemProvider : IQueryProvider
    {
        private readonly string root;

        public FileSystemProvider(string root)
        {
            System.Diagnostics.Trace.WriteLine($"FileSystemProvider(string root)");
            this.root = root;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            System.Diagnostics.Trace.WriteLine($"CreateQuery");
            return new FileSystemContext(this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            
            var type = expression.GetType();
            var yy = expression as MethodCallExpression;
            //foreach(var arg in yy.Arguments)
            //{
            //    UnaryExpression unary = arg as UnaryExpression;
            //    if(unary != null)
            //    {
            //        var lambda = unary.Operand as LambdaExpression;
            //        type = lambda.Parameters[0].GetType();
            //        var binary = lambda.Body as BinaryExpression;
            //        var left = binary.Left as UnaryExpression;
            //        var property = left.Operand as MemberExpression;
            //        var param = property.Expression as ParameterExpression;
            //        type = property.Expression.GetType();

            //        var right = binary.Right as ConstantExpression;
                    
            //    }
                
            //}


            var param = Expression.Parameter(typeof(FileSystemElement), "x");
            var property = Expression.MakeMemberAccess(param, typeof(FileSystemElement).GetProperty("ElementType"));
            var left = Expression.MakeUnary(ExpressionType.Convert, property, typeof(int));
            ConstantExpression right = Expression.Constant(0);
            var binary = Expression.MakeBinary(ExpressionType.NotEqual, left, right);
            var lambda = Expression.Lambda(binary, param);
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(bool));

            MethodCallExpression method = Expression.Call(yy.Method, yy.Arguments[0], unary);

            System.Diagnostics.Trace.WriteLine($"CreateQuery<TElement> {yy.Method.Name}");

            return (IQueryable<TElement>)new FileSystemContext(this, method);
        }

        public object Execute(Expression expression)
        {
            System.Diagnostics.Trace.WriteLine($"Execute");
            return Execute<FileSystemElement>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var yy = expression as MethodCallExpression;
            var type = expression.NodeType;
            System.Diagnostics.Trace.WriteLine($"Execute<TResult>");
            var isEnumerable = (typeof(TResult).Name == "IEnumerable`1");
            return (TResult)FileSystemQueryContext.Execute(expression, isEnumerable, root);
        }
    }
}
