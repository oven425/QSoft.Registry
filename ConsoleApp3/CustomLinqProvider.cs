using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Titan.ODataClient
{
    public class DbQuery<T> : IQueryable<T>, IDisposable
    {
        public DbQuery()
        {
            Provider = new DbQueryProvider();
            Expression = Expression.Constant(this);//最后一个表达式将是第一IQueryable对象的引用。 
        }
        public DbQuery(Expression expression)
        {
            Provider = new DbQueryProvider();
            Expression = expression;
        }

        public Type ElementType
        {
            get { return typeof(T); }
            private set { ElementType = value; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get;
            private set;
        }

        public IQueryProvider Provider
        {
            get;
            private set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<T>(Expression) as IEnumerable<T>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (Provider.Execute(Expression) as IEnumerable).GetEnumerator();
        }

        public void Dispose()
        {

        }
    }

    public class DbQueryProvider : IQueryProvider
    {
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new DbQuery<TElement>();
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            //这里牵扯到对表达式树的分析，就不多说了。
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return default(TResult);//强类型数据集
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return new List<object>();//弱类型数据集
        }
    }




    public class CustomLinqProvider<T> : IQueryable<T>, IQueryProvider
    {

        List<T> results = new List<T>();
        public IEnumerator<T> GetEnumerator()
        {
            return (this as IQueryable<T>).Provider.Execute<IEnumerator<T>>(Expression);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get { return Expression.Constant(this); } }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (this as IQueryProvider).CreateQuery<T>(expression);
        }

        private Expression _whereExpression = null;
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var methodCall = expression as MethodCallExpression;
            if (methodCall != null && methodCall.Method.Name == "Where")
            {
                //Expression<Func<TElement, bool>> predicate = (Expression<Func<TElement, bool>>) methodCall.Arguments[1] ;//as Expression<Func<TElement, bool>>;
                ProcessExpression(methodCall.Arguments[1]);
                _whereExpression = expression;
                return (IQueryable<TElement>)this;
            }
            return new CustomLinqProvider<TElement>();
        }

        public object Execute(Expression expression)
        {
            var methodCall = expression as MethodCallExpression;
            foreach (var argument in methodCall.Arguments)
            {
                ProcessExpression(argument);
            }

            return results;
            //return (this as IQueryProvider).Execute<IEnumerator<T>>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var type = typeof(TResult);
            return (TResult)Execute(expression);
        }

        private void ProcessExpression(Expression expression)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                //var logicalBinaryExpression = expression as LogicalBinaryExpression;

                if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    ProcessEqualResult(binaryExpression);
                }
                else if (binaryExpression.NodeType == ExpressionType.LessThan)
                {
                    var val = GetValue(binaryExpression);
                }
                else if (binaryExpression.NodeType == ExpressionType.GreaterThan)
                {
                    var val = GetValue(binaryExpression);
                }
                else if (binaryExpression.NodeType == ExpressionType.And || binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    ProcessExpression(binaryExpression.Left);
                    ProcessExpression(binaryExpression.Right);
                }
                else if (binaryExpression.NodeType == ExpressionType.Or || binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    ProcessExpression(binaryExpression.Left);
                    ProcessExpression(binaryExpression.Right);
                }
                else
                {
                    ProcessAndOrResult(binaryExpression);
                }
            }
            else if (expression is UnaryExpression)
            {
                UnaryExpression uExp = expression as UnaryExpression;
                ProcessExpression(uExp.Operand);
            }
            else if (expression is LambdaExpression)
            {
                ProcessExpression(((LambdaExpression)expression).Body);
            }
            else if (expression is ParameterExpression)
            {
                var type = ((ParameterExpression)expression).Type;
                //if (((ParameterExpression)expression).Type == typeof(Person))
                //{
                //    _person = GetPersons();
                //}
            }
        }

        private void ProcessEqualResult(BinaryExpression expression)
        {
            if (expression.Left.NodeType == ExpressionType.MemberAccess)
            {
                var name = ((MemberExpression)expression.Left).Expression;
                //ProceesItem(name);
            }

            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                var name = ((ConstantExpression)expression.Right).Value;
                //ProceesItem(name);
            }
            else
            {

            }
        }
        private void ProcessAndOrResult(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.And || expression.NodeType == ExpressionType.AndAlso)
            {
                ProcessAndOrResult(expression.Left as BinaryExpression);
                ProcessAndOrResult(expression.Right as BinaryExpression);
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                ProcessEqualResult(expression);
            }
            else if (expression.NodeType == ExpressionType.Or || expression.NodeType == ExpressionType.OrElse)
            {
                ProcessOrOrElse(expression);
            }
            else if (expression.Right.NodeType == ExpressionType.And || expression.Right.NodeType == ExpressionType.AndAlso)
            {
                string name = (String)((ConstantExpression)expression.Right).Value;
                //ProceesItem(name);
            }
            else
            {

            }
        }
        private void ProcessOrOrElse(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Or || expression.NodeType == ExpressionType.OrElse)
            {
                ProcessAndOrResult(expression.Left as BinaryExpression);
                ProcessAndOrResult(expression.Right as BinaryExpression);
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                ProcessEqualResult(expression);
            }
        }

        //private void ProceesItem(string name)
        //{
        //    IList<Person> filtered = new List<Person>();

        //    foreach (Person person in GetPersons())
        //    {
        //        if (string.Compare(person.Name, name, true) == 0)
        //        {
        //            filtered.Add(person);
        //        }
        //    }
        //    _person = filtered;
        //}


        private object GetValue(BinaryExpression expression)
        {
            if (expression.Right.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression.Right).Value;
            }
            return null;
        }
    }
}