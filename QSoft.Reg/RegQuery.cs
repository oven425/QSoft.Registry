using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QSoft.Registry
{
    public class RegQuery<T> : IOrderedQueryable<T>
    {
        public RegQuery(T data, RegistryHive hive, string path)
        {

            this.Provider = new RegProvider(hive, path);
            this.Expression = Expression.Constant(this);
        }



        public RegQuery(T data)
        {

            //this.Provider = new RegProvider(hive, path);
            //this.Expression = Expression.Constant(this);
        }

        public RegQuery(RegProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }


        public Expression Expression { private set; get; }

        public Type ElementType => typeof(T);

        public IQueryProvider Provider { private set; get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
