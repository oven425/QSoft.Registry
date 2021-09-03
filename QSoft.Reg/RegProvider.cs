using Microsoft.Win32;
using System;
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

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var type = expression.GetType();
            var method = expression as MethodCallExpression;
            if(method.Arguments.Count>1)
            {
                var unary = method.Arguments[1] as UnaryExpression;
                var lambda = unary.Operand as LambdaExpression;
                var binary = lambda.Body as BinaryExpression;
                var left = binary.Left as MemberExpression;
                var param = left.Expression as ParameterExpression;
                type = binary.Left.GetType();
            }
            return new RegQuery<TElement>(this, expression);
        }

        bool Process(object data)
        {
            return true;
        }


        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
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
            var regqerty = regs.AsQueryable();
            var type = typeof(TResult);
            if(type.Name == "IEnumerable`1")
            {

            }
            else
            {
                
            }
            throw new NotImplementedException();
            //List<Test> dds = new List<Test>();
            //for (int i = 0; i < 200; i++)
            //{
            //    dds.Add(new Test() { DisplayName = i.ToString() });
            //}
            //m_Datas = dds.AsQueryable();
            //ExpressionVisitorA aa = new ExpressionVisitorA(this.m_Datas);
            //Expression expr = aa.Visit(expression);

            //return (TResult)this.m_Datas.Provider.CreateQuery(expr);

            
        }

    }

}
