using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QSoft.Registry
{
    public class RegProvider : IQueryProvider
    {
        RegistryKey m_Reg;
        //IQueryable<Test> m_Datas;
        public RegProvider(RegistryHive hive, string path)
        {
            this.m_Reg = hive.OpenView64(path);
        }

        public IQueryable CreateQuery(Expression expression)
        {

            throw new NotImplementedException();
        }

        Queue<Expression> CreateQuertys = new Queue<Expression>(); 
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var type = expression.GetType();
            var method = expression as MethodCallExpression;
            type = method.Arguments[0].GetType();
            if (method.Arguments.Count > 1)
            {
                var unary = method.Arguments[1] as UnaryExpression;
                var lambda = unary.Operand as LambdaExpression;
                var binary = lambda.Body as BinaryExpression;
                var left = binary.Left as MemberExpression;
                var param = left.Expression as ParameterExpression;
                var right = binary.Right as ConstantExpression;
                type = binary.Right.GetType();
                var query = this.Build(left.Member.Name, right.Value, binary);
                CreateQuertys.Enqueue(query);
            }
            //RegExpressionVisitor vv = new RegExpressionVisitor();
            //vv.Visit(expression);

            return new RegQuery<TElement>(this, expression);
        }

        Expression Build(string left_value, object right_value, BinaryExpression binary1)
        {
            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => x.Name == "GetValue");
            var left_args_1 = Expression.Constant(left_value);
            var left_args_0 = Expression.Parameter(typeof(RegistryKey), "x");
            var left = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), left_args_0, left_args_1);
            var arg1 = Expression.Parameter(typeof(RegistryKey), "x");
            arg1 = left_args_0;

            var right = Expression.Constant(right_value);
            var binary = Expression.MakeBinary(binary1.NodeType, left, right, binary1.IsLiftedToNull, binary1.Method);
            var param = Expression.Parameter(typeof(RegistryKey), "x");
            param = arg1;
            var lambda = Expression.Lambda(binary, param);

            return lambda;
        }

        bool Process(object data)
        {
            return true;
        }


        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Enumerable<T>(IQueryable<RegistryKey> query)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true);
            foreach (var oo in query)
            {
                var inst = Activator.CreateInstance(typeof(T));
                foreach (var pp in pps)
                {
                    pp.SetValue(inst, oo.GetValue(pp.Name));
                }
                yield return (T)inst;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            RegExpressionVisitor regvisitor = new RegExpressionVisitor();
            var expr = regvisitor.Visit(expression);
            List<RegistryKey> regs = new List<RegistryKey>();
            var subkeynames = this.m_Reg.GetSubKeyNames();

            foreach (var subkeyname in subkeynames)
            {
                regs.Add(this.m_Reg.OpenSubKey(subkeyname));
            }
            var wheres = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Where" && x.GetParameters().Length == 2);
            var unary = Expression.MakeUnary(ExpressionType.Quote, CreateQuertys.ElementAt(0), typeof(RegistryKey));
            var tte = regs.AsQueryable();
            var methodcall_param_0 = Expression.Constant(tte);
            var methodcall1 = Expression.Call(wheres.ElementAt(0).MakeGenericMethod(typeof(RegistryKey)), methodcall_param_0, unary);
            var excute = tte.Provider.CreateQuery<RegistryKey>(methodcall1);
            
            var type = typeof(TResult);
            Type[] tt = type.GetGenericArguments();
            var pps = tt[0].GetProperties().Where(x=>x.CanWrite==true);

            
            if (type.Name == "IEnumerable`1")
            {
                var mi = typeof(RegProvider).GetMethod("Enumerable");
                var fooRef = mi.MakeGenericMethod(tt[0]);
                return (TResult)fooRef.Invoke(this, new object[]{ excute, tt[0]});
                //return (TResult)Enumerable<Test>(excute, tt[0]);
            }
            else
            {
                
            }
            throw new NotImplementedException();            
        }
    }

    
}
