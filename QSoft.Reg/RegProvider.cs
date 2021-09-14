﻿using Microsoft.Win32;
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
        Type m_DataType;
        public RegProvider(RegistryHive hive, string path, Type datatype)
        {
            this.m_DataType = datatype;
            this.m_Reg = hive.OpenView64(path);
        }

        public IQueryable CreateQuery(Expression expression)
        {

            throw new NotImplementedException();
        }

        Queue<(MethodInfo method, Expression expression)> CreateQuertys = new Queue<(MethodInfo method, Expression expression)>();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            //var type = expression.GetType();
            //var method = expression as MethodCallExpression;
            //type = method.Arguments[0].GetType();
            //if (method.Arguments.Count > 1)
            //{
            //    //var unary = method.Arguments[1] as UnaryExpression;
            //    //var lambda = unary.Operand as LambdaExpression;
            //    //var binary = lambda.Body as BinaryExpression;
            //    //var left = binary.Left as MemberExpression;
            //    //var param = left.Expression as ParameterExpression;
            //    //var right = binary.Right as ConstantExpression;
            //    //type = binary.Right.GetType();
            //    //var query = this.Build(left.Member.Name, right.Value, binary);

            //    //CreateQuertys.Enqueue((method.Method, query));

            //    RegExpressionVisitor vv = new RegExpressionVisitor();
            //    //var expr = vv.Visit(expression, );
            //    //CreateQuertys.Enqueue((method.Method, expr));
            //}
           
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
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            return unary;
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
            var type = typeof(TResult);
            Type[] tts = type.GetGenericArguments();
            
            List<RegistryKey> regs = new List<RegistryKey>();
            var subkeynames = this.m_Reg.GetSubKeyNames();

            foreach (var subkeyname in subkeynames)
            {
                regs.Add(this.m_Reg.OpenSubKey(subkeyname));
            }

            var tte = regs.AsQueryable();
            RegExpressionVisitor regvisitor = new RegExpressionVisitor();

            var expr = regvisitor.Visit(expression, this.m_DataType, tte);
            
            var methodcall_param_0 = Expression.Constant(tte);


            if (type.Name == "IEnumerable`1")
            {
                var creatquerys = typeof(IQueryProvider).GetMethods().Where(x=>x.Name== "CreateQuery" && x.IsGenericMethod==true);
                var tts1 = new Type[tts.Length];
                for(int i=0; i<tts.Length; i++)
                {
                    if(tts[i]==this.m_DataType)
                    {
                        tts1[i] = typeof(RegistryKey);
                    }
                    else
                    {
                        tts1[i] = tts[i];
                    }
                }
                var creatquery = creatquerys.First().MakeGenericMethod(tts1[0]);
                var excute = creatquery.Invoke(tte.Provider, new object[] { expr });
                //var excute = tte.Provider.CreateQuery<RegistryKey>(expr);
                if(tts[0] == this.m_DataType)
                {
                    var mi = typeof(RegProvider).GetMethod("Enumerable");
                    var fooRef = mi.MakeGenericMethod(tts[0]);
                    return (TResult)fooRef.Invoke(this, new object[] { excute });
                }
                return (TResult)excute;

            }
            else
            {
                object inst = null;
                var excute = tte.Provider.Execute<TResult>(expr);
                var excute_reg = excute as RegistryKey;
                if (excute_reg !=null)
                {
                    var pps = typeof(TResult).GetProperties().Where(x => x.CanWrite == true);
                    inst = Activator.CreateInstance(typeof(TResult));
                    foreach (var pp in pps)
                    {
                        pp.SetValue(inst, excute_reg.GetValue(pp.Name));
                    }
                }
                inst = excute;


                return (TResult)inst;
            }
        }
    }

    
}
