using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Registry.Linq
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RegIgnore : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RegPropertyName : Attribute
    {
        public string Name { set; get; }
        //public object Default { set; get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RegSubKeyName:Attribute
    {
        public bool IsFullName { set; get; } = false;
        public Expression ToExpression(Type src, Expression dst)
        {
            Expression member = null;
            member = Expression.Property(dst, "Name");
            if (this.IsFullName == false)
            {
                var method = typeof(RegQueryHelper).GetMethod("GetLastSegement");
                member = Expression.Call(method, member);
            }
            var property = src;
            bool isnuallable = false;
            if (property.IsGenericType == true && property.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isnuallable = true;
                property = src.GetGenericArguments()[0];
            }
            var typecode = Type.GetTypeCode(property);
            switch (typecode)
            {
                case TypeCode.DateTime:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Double:
                case TypeCode.Single:
                    {
                        var parseMethod = property.GetMethod("Parse", new[] { typeof(string) });
                        member = Expression.Call(parseMethod, member);
                        if (isnuallable == true)
                        {
                            member = Expression.Convert(member, property);
                        }
                    }
                    break;
            }


            return member;
        }
    }
}
