#define Queryable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;

namespace ConsoleApp1
{
    public class InstallAppCompare : IEqualityComparer<InstalledApp>
    {
        public bool Equals(InstalledApp x, InstalledApp y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.DisplayName == y.DisplayName;
        }

        public int GetHashCode(InstalledApp obj)
        {
            int hashcode = 0;
            if (object.ReferenceEquals(obj, null))
            {
                hashcode = 0;
            }
            hashcode = obj.DisplayName == null ? 0 : obj.DisplayName.GetHashCode();
            System.Diagnostics.Trace.WriteLine($"hashcode:{hashcode}");
            return hashcode;
        }
    }

    public class App
    {
        public int ID { set; get; }
        [RegPropertyName(Name = "DisplayName")]
        public string Name { set; get; }
        //[RegPropertyName(Name = "DisplayVersion")]
        public Version Version { set; get; }
        [RegIgnore]
        public int Size { set; get; }
    }

    public class Company
    {
        [RegSubKeyName]
        public int Key { set; get; }
        [RegPropertyName(Name = "Name1")]
        public string Name { set; get; }
        [RegPropertyName(Name="ID")]
        public int ID { set; get; }
        public string Address { set; get; }

        [RegIgnore]
        public int OrderBy { set; get; }
        [RegIgnore]
        public int ThenBy { set; get; }
        [RegIgnore]
        public int ThenBy1 { set; get; }
    }

    public class InstalledApp
    {
        public string DisplayName { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public Version Version { set; get; }
        public int? EstimatedSize { set; get; }
        public DateTime? Now { set; get; }

        public InstalledApp() { }

        public InstalledApp(InstalledApp data)
        {
            var pps = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach (var pp in pps)
            {
                pp.SetValue(this, pp.GetValue(data, null), null);
            }
        }
    }

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }

    public class AAAA
    {
        //[RegSubKeyName]
        //public string Key { set; get; }
        //public string port { set; get; }
    }

    public class AAA
    {
        //[RegSubKeyName]
        //public string Key { set; get; }
        public AAAA AAAA { set; get; }
    }

    public class AA
    {
        //[RegSubKeyName]
        //public string Key { set; get; }
        public AAA AAA { set; get; }
    }

    public class A
    {
        //[RegSubKeyName]
        //public string Key { set; get; }
        public AA AA { set; get; }
    }

    //public class Phone
    //{
    //    public string home { set; get; }
    //    public string mobile { set; get; }
    //}

    public class Phone
    {
        public string company { set; get; }
        public string number { set; get; }
        //[RegPropertyName(Name ="home")]
        public PhoneCat home { set; get; }
        public PhoneCat office { set; get; }
    }

    public class PhoneCat
    {
        public string name { set; get; }
    }

    public class People
    {
        public string Name { set; get; }
        [RegSubKeyName]
        public string Key { set; get; }
        public int? Weight { set; get; }
        [RegPropertyName(Name = "height")]
        public int Height { set; get; }
        
        public Phone phone { set; get; }
        //public List<Phone> phones { set; get; }
    }



    class Program
    {
        public static T Build<T>(RegistryKey reg)
        {
            var pps = typeof(T).GetProperties().Where(x => x.CanWrite == true)
                .Select(x => new
                {
                    x,
                    attr = x.GetCustomAttributes(true).FirstOrDefault(y => y is RegSubKeyName || y is RegIgnore || y is RegPropertyName)
                }).Where(x => !(x.attr is RegIgnore));
            foreach(var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.x.PropertyType);
                var property = pp.x.PropertyType;
                if (pp.x.PropertyType.IsGenericType == true && pp.x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    property = pp.x.PropertyType.GetGenericArguments()[0];
                    typecode = Type.GetTypeCode(property);
                }
            }
            T obj = Activator.CreateInstance<T>();

            foreach (var pp in pps)
            {
                var typecode = Type.GetTypeCode(pp.x.PropertyType);
                if (pp.x.PropertyType.IsGenericType == true && pp.x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var property = pp.x.PropertyType.GetGenericArguments()[0];
                    typecode = Type.GetTypeCode(property);
                }
                if (typecode == TypeCode.Object)
                {
                    if (reg.GetSubKeyNames().Any(x => x == pp.x.Name) == true)
                    {
                        var methd = typeof(Program).GetMethods().Where(x => x.Name == "Build");
                        var subreg = reg.OpenSubKey(pp.x.Name);
                        var subobj = methd.First().MakeGenericMethod(pp.x.PropertyType).Invoke(null, new object[] { subreg });
                        subreg.Close();
                        pp.x.SetValue(obj, subobj, null);
                    }
                }
                else
                {
                    var vv = typeof(RegistryKeyEx).GetMethods().Where(x => x.Name == "GetValue");
                    var h = vv.First().MakeGenericMethod(pp.x.PropertyType).Invoke(null, new object[] { reg, pp.x.Name });
                    pp.x.SetValue(obj, h, null);
                }
            }

            return obj;
        }

        public class BuildExprTemp
        {
            public object attr { set; get; }
            public PropertyInfo x { set; get; }
        }

        static public Expression<Func<int, int>> MakeFactorialExpression()
        {
            var nParam = Expression.Parameter(typeof(int), "n");
            var methodVar = Expression.Variable(typeof(Func<int, int>), "factorial");
            var one = Expression.Convert(Expression.Constant(1), typeof(int));

            var block = Expression.Block(
                    // Func<uint, uint> method;
                    new[] { methodVar },
                    // method = n => ( n <= 1 ) ? 1 : n * method( n - 1 );
                    Expression.Assign(
                        methodVar,
                        Expression.Lambda<Func<int, int>>(
                            Expression.Condition(
                                // ( n <= 1 )
                                Expression.LessThanOrEqual(nParam, one),
                                // 1
                                one,
                                // n * method( n - 1 )
                                Expression.Multiply(
                                    // n
                                    nParam,
                                    // method( n - 1 )
                                    Expression.Invoke(
                                        methodVar,
                                        Expression.Subtract(nParam, one)))),
                            nParam)),
                    // return method( n );
                    Expression.Invoke(methodVar, nParam));
            var aaaa = Expression.Lambda<Func<int, int>>(block, nParam).Compile()(4);

            return Expression.Lambda<Func<int, int>>(
                Expression.Block(
                    // Func<uint, uint> method;
                    new[] { methodVar },
                    block),
                nParam);
        }


        //https://www.codeproject.com/Articles/107477/Use-of-Expression-Trees-in-NET-for-Lambda-Decompos
        static void BuildExpr<T>(RegistryKey reg)
        {
            try
            {
                
                ParameterExpression getpps_where_p_1 = Expression.Variable(typeof(PropertyInfo), "getpps_where_p_1");
                ParameterExpression getpps_where_p_2 = Expression.Variable(typeof(BuildExprTemp), "getpps_where_p_2");
                var getpps_first_param = Expression.Parameter(typeof(object), "getpps_first_param");
                ParameterExpression getpps_select_p_1 = Expression.Variable(typeof(PropertyInfo), "getpps_select_p_1");

                var pps = Expression.Variable(typeof(IEnumerable<BuildExprTemp>), "pps");
                
                try
                {
                    var oi = typeof(Expression).GetMethods().Where(x => x.Name == "Lambda" && x.IsGenericMethod == true).First();
                    var tts = Enumerable.Range(1, 2).AsQueryable().Select(x => new
                    {
                        lambda = Expression.Lambda<Func<int>>(Expression.Constant(1))
                    });
                    typeof(Type).GetMethod("MakeGenericType");
                    var mmn = Expression.NewArrayInit(typeof(Type), Expression.Constant(typeof(int)));
                    var type = Expression.Call(Expression.Constant(typeof(Func<>)), typeof(Type).GetMethod("MakeGenericType"), mmn);

                    var mms = typeof(MethodInfo).GetMethods().Where(x => x.Name == "MakeGenericMethod");
                    var expression_lambda = Expression.Call(Expression.Constant(oi), typeof(MethodInfo).GetMethod("MakeGenericMethod"), Expression.NewArrayInit(typeof(Type), type));
                    //var mmmm = Expression.Lambda<Func<int>>(Expression.Constant(1));
                    //oi = oi.MakeGenericMethod(typeof(Func<int>));
                    var ytr = Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), Expression.Constant(1).Convert(typeof(object)));
                    //var aaa = Expression.Call(oi, ytr, Expression.NewArrayInit(typeof(ParameterExpression)));
                    var m3_1 = Expression.Call(typeof(Expression).GetMethod("Call", new Type[] { typeof(MethodInfo), typeof(Expression) }), expression_lambda, ytr);

                    var ppp = Expression.Call(typeof(Expression).GetMethod("Parameter", new[] { typeof(Type), typeof(string) }), type, Expression.Constant("x"));
                    var aaasi = Expression.Call(typeof(Expression).GetMethod("Assign"), ppp, m3_1);

                    var invokeexpr = Expression.Call(typeof(Expression).GetMethod("Invoke",new[] { typeof(Expression), typeof(Expression[])}), ppp, Expression.NewArrayInit(typeof(Expression)));
                    var hr = Expression.Lambda(invokeexpr).Compile().DynamicInvoke();


                    Type define_type = typeof(int);
                    var data = Expression.Parameter(typeof(double), "data");
                    var data_in = Expression.Variable(typeof(int), "data_in");
                    var func = Expression.Parameter(typeof(Func<double, double>), "func");
                    //var result = Expression.Parameter(typeof(int), "result");
                    var lable_break = Expression.Label("lable_break");


                    //var tt = Enumerable.Range(1, 2).AsQueryable().Select(x => new
                    //{
                    //    aa = Expression.NewArrayInit(typeof(Expression), Expression.con)
                    //});

                    var one = Expression.Constant(1.0);
                    var func_type_expr = Expression.Call(Expression.Constant(typeof(Func<,>)), typeof(Type).GetMethod("MakeGenericType"), Expression.NewArrayInit(typeof(Type), Expression.Constant(typeof(int)), Expression.Constant(typeof(int))));
                    var func_expr = Expression.Call(typeof(Expression).GetMethod("Parameter", new[] { typeof(Type), typeof(string) }), func_type_expr, Expression.Constant("func"));


                    //var block_method = typeof(Expression).GetMethod("Block", new[] { typeof(IEnumerable<ParameterExpression>), typeof(Expression[]) });
                    //var ayy = Expression.NewArrayInit(typeof(ParameterExpression), func_expr);

                    //var call = Expression.Call(typeof(Expression).GetMethod("Call", new Type[] { typeof(MethodInfo), typeof(Expression) }), "123".WriteLineExpr());

                    //var bbl = Expression.NewArrayInit(typeof(Expression), Expression.Block("123".WriteLineExpr()));
                    //var bblock = Expression.Call(block_method, ayy, bbl);
                    //Expression.Lambda(bblock).Compile().DynamicInvoke();


                    var blockc = Expression.Block(
                       new[] { func },
                       Expression.Assign(
                           func,
                           Expression.Lambda(
                               Expression.Condition(Expression.LessThanOrEqual(data, one), one,
                               Expression.Block(
                                   data.WriteLineExpr(),
                                   Expression.Add(data,
                                       Expression.Invoke(func, Expression.Subtract(data, one)))
                                       )),
                               data)),
                       Expression.Invoke(func, data));
                    var result = Expression.Lambda<Func<double, double>>(blockc, data).Compile()(5);

                }
                catch (Exception ee)
                {
                    System.Diagnostics.Trace.WriteLine(ee.Message);
                }
                //runtime build lambda
                try
                {
                    var new_me = typeof(Expression).GetMethod("New", new[] {typeof(Type) });
                    var makeb = typeof(Expression).GetMethod("MakeBinary", new[] { typeof(ExpressionType), typeof(Expression), typeof(Expression) });
                    var cco = typeof(Expression).GetMethod("Condition", new[] { typeof(Expression), typeof(Expression), typeof(Expression) });
                    var loi = typeof(Expression).GetMethods().Where(x => x.Name == "Lambda" && x.IsGenericMethod == false && x.GetParameters().Length == 2).FirstOrDefault();
                    var ppp1 = Expression.Call(typeof(Expression).GetMethod("Parameter", new[] { typeof(Type), typeof(string) }), Expression.Constant(typeof(RegistryKey)), Expression.Constant("x"));
                    var ll_call_pps = Expression.NewArrayInit(typeof(ParameterExpression), ppp1);
                    var ll_call = Expression.Call(loi,
                        Expression.Call(new_me, Expression.Constant(typeof(People))),
                        ll_call_pps);
                    var dd = Expression.Lambda(ll_call).Compile();
                    //var hhr = dd.Compile().DynamicInvoke(typeof(People));
                }
                catch (Exception ee)
                {
                    System.Diagnostics.Trace.WriteLine(ee.Message);
                }


                var subregfunc = Expression.Variable(typeof(Func<RegistryKey, MemberInitExpression>), "subregfunc");
                LabelTarget label = Expression.Label();
                ParameterExpression regkey = Expression.Variable(typeof(RegistryKey), "reg");
                ParameterExpression enumerator = Expression.Variable(typeof(IEnumerator<>).MakeGenericType(pps.Type.GetGenericArguments()[0]), "enumerator");
                ParameterExpression typecode = Expression.Variable(typeof(TypeCode), "typecode");
                ParameterExpression property = Expression.Variable(typeof(Type), "property");
                ParameterExpression name = Expression.Variable(typeof(Expression), "name");
                ParameterExpression memberbinds = Expression.Variable(typeof(List<MemberBinding>), "memberbinds");
                ParameterExpression isconvert = Expression.Variable(typeof(bool), "isconvert");
                
                ParameterExpression actionp = Expression.Parameter(typeof(T), "actionp");
                var actiont = Expression.Variable(typeof(Action<T>), "actiont");
                var foreach1 = Expression.Block(new[] { actiont },
                    Expression.Assign(actiont, Expression.Lambda(
                    Expression.Block(
                        $"{typeof(T).Name}-".WriteLineExpr(),
                    Expression.Block(new[] { enumerator, memberbinds, subregfunc, pps },
                    $"{typeof(T).Name}--".WriteLineExpr(),
                    Expression.Assign(pps,
                        ExpressionEx.Where(
                            ExpressionEx.Select(
                                ExpressionEx.Where(Expression.Call(Expression.Constant(typeof(T)), typeof(Type).GetMethod("GetProperties", new Type[] { })), Expression.Lambda(Expression.MakeBinary(ExpressionType.Equal, getpps_where_p_1.PropertyExpr("CanWrite"), Expression.Constant(true)), getpps_where_p_1)),
                                Expression.Lambda(Expression.MemberInit(Expression.New(typeof(BuildExprTemp)),
                                    Expression.Bind(typeof(BuildExprTemp).GetProperty("x"), getpps_select_p_1), Expression.Bind(typeof(BuildExprTemp).GetProperty("attr"), ExpressionEx.FirstOrDefault(Expression.Call(getpps_select_p_1, typeof(PropertyInfo).GetMethod("GetCustomAttributes", new Type[] { typeof(bool) }), Expression.Constant(true)),
                                        Expression.Lambda(ExpressionType.OrElse.MakeBinary(Expression.TypeIs(getpps_first_param, typeof(RegSubKeyName)), Expression.TypeIs(getpps_first_param, typeof(RegIgnore)), Expression.TypeIs(getpps_first_param, typeof(RegPropertyName))), getpps_first_param)))
                                    ), getpps_select_p_1)),
                        Expression.Lambda(ExpressionType.NotEqual.MakeBinary(Expression.TypeIs(getpps_where_p_2.PropertyExpr("attr"), typeof(RegIgnore)), Expression.Constant(true)), getpps_where_p_2))),
                        Expression.Assign(enumerator, Expression.Call(pps, pps.Type.GetMethod("GetEnumerator"))),
                        Expression.Assign(memberbinds, Expression.New(typeof(List<MemberBinding>))),
                        Expression.Loop(
                                Expression.IfThenElse(ExpressionType.Equal.MakeBinary(Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")), Expression.Constant(true)),
                                Expression.Block(new[] { typecode, property, name, isconvert },
                                    Expression.Assign(isconvert, Expression.Constant(false)),
                                    Expression.Assign(property, enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("PropertyType")),
                                    Expression.Assign(typecode, Expression.Call(typeof(Type).GetMethod("GetTypeCode"), property)),
                                    Expression.IfThen(Expression.MakeBinary(ExpressionType.AndAlso,
                                            ExpressionType.Equal.MakeBinary(property.PropertyExpr("IsGenericType"), Expression.Constant(true)),
                                            ExpressionType.Equal.MakeBinary(Expression.Call(property, typeof(Type).GetMethod("GetGenericTypeDefinition")), Expression.Constant(typeof(Nullable<>)))),
                                        Expression.Block(
                                            Expression.Assign(isconvert, Expression.Constant(true)),
                                            Expression.Assign(property, Expression.ArrayIndex(Expression.Call(property, typeof(Type).GetMethod("GetGenericArguments")), Expression.Constant(0))),
                                            Expression.Assign(typecode, Expression.Call(typeof(Type).GetMethod("GetTypeCode"), property))
                                            )
                                            ),
                                    enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("Name").WriteLineExpr(),
                                    enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("PropertyType").WriteLineExpr(),
                                    Expression.IfThen(ExpressionType.Equal.MakeBinary(typecode, Expression.Constant(TypeCode.Object)),
                                    Expression.Block(
                                        "Object".WriteLineExpr(), 
                                        Expression.Invoke(actiont, actionp))
                                        )
                                    ),
                                Expression.Block(
                                    "brear".WriteLineExpr(),
                                Expression.Break(label))
                                    ), label)
                        )), actionp)),
                        Expression.Invoke(actiont, actionp)
                    );
                Expression.Lambda<Action<People>>(foreach1, actionp).Compile()(new People());

                //var lambda_memberinit = Expression.Lambda<Func<RegistryKey, MemberInitExpression>>(foreach1, regkey);
                //Expression.Assign(subregfunc, lambda_memberinit);
                //var peoplekey = Registry.CurrentConfig.OpenSubKey(@"people");
                //var peoplekeys = peoplekey.GetSubKeyNames().Select(x => peoplekey.OpenSubKey(x));
                //var peoples_query = peoplekeys.AsQueryable();

                
                //var mmmm = Expression.Lambda<Func<RegistryKey, MemberInitExpression>>(foreach1, regkey).Compile()(peoplekeys.Last());
                //var hr = Expression.Lambda(mmmm).Compile().DynamicInvoke();


            }
            catch (Exception ee)
            {
                System.Diagnostics.Trace.WriteLine(ee.Message);
            }

//            var foreach1 = Expression.Block(new[] { enumerator, memberbinds, subregfunc, pps },
//Expression.Assign(pps,
//    ExpressionEx.Where(
//        ExpressionEx.Select(
//            ExpressionEx.Where(Expression.Call(Expression.Constant(typeof(T)), typeof(Type).GetMethod("GetProperties", new Type[] { })), Expression.Lambda(Expression.MakeBinary(ExpressionType.Equal, getpps_where_p_1.PropertyExpr("CanWrite"), Expression.Constant(true)), getpps_where_p_1)),
//            Expression.Lambda(Expression.MemberInit(Expression.New(typeof(BuildExprTemp)),
//                Expression.Bind(typeof(BuildExprTemp).GetProperty("x"), getpps_select_p_1), Expression.Bind(typeof(BuildExprTemp).GetProperty("attr"), ExpressionEx.FirstOrDefault(Expression.Call(getpps_select_p_1, typeof(PropertyInfo).GetMethod("GetCustomAttributes", new Type[] { typeof(bool) }), Expression.Constant(true)),
//                    Expression.Lambda(ExpressionType.OrElse.MakeBinary(Expression.TypeIs(getpps_first_param, typeof(RegSubKeyName)), Expression.TypeIs(getpps_first_param, typeof(RegIgnore)), Expression.TypeIs(getpps_first_param, typeof(RegPropertyName))), getpps_first_param)))
//                ), getpps_select_p_1)),
//    Expression.Lambda(Expression.MakeBinary(ExpressionType.NotEqual, Expression.TypeIs(getpps_where_p_2.PropertyExpr("attr"), typeof(RegIgnore)), Expression.Constant(true)), getpps_where_p_2))),
//Expression.Assign(enumerator, Expression.Call(pps, pps.Type.GetMethod("GetEnumerator"))),
//    Expression.Assign(memberbinds, Expression.New(typeof(List<MemberBinding>))),
//    Expression.Loop(
//        Expression.Block(
//            Expression.IfThenElse(Expression.MakeBinary(ExpressionType.Equal, Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")), Expression.Constant(true)),
//            Expression.Block(new[] { typecode, property, name, isconvert },
//                Expression.Assign(isconvert, Expression.Constant(false)),
//                //Expression.Assign(regkey, reg_send),
//                Expression.Assign(property, enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("PropertyType")),
//                Expression.Assign(typecode, Expression.Call(typeof(Type).GetMethod("GetTypeCode"), property)),
//                Expression.IfThen(Expression.MakeBinary(ExpressionType.AndAlso,
//                        Expression.MakeBinary(ExpressionType.Equal, property.PropertyExpr("IsGenericType"), Expression.Constant(true)),
//                        Expression.MakeBinary(ExpressionType.Equal, Expression.Call(property, typeof(Type).GetMethod("GetGenericTypeDefinition")), Expression.Constant(typeof(Nullable<>)))),
//                    Expression.Block(
//                        Expression.Assign(isconvert, Expression.Constant(true)),
//                        Expression.Assign(property, Expression.ArrayIndex(Expression.Call(property, typeof(Type).GetMethod("GetGenericArguments")), Expression.Constant(0))),
//                        Expression.Assign(typecode, Expression.Call(typeof(Type).GetMethod("GetTypeCode"), property))
//                        )
//                        ),
//                Expression.IfThen(Expression.MakeBinary(ExpressionType.Equal, typecode, Expression.Constant(TypeCode.Object)), "object".WriteLineExpr()),
//                Expression.Assign(name, Expression.Constant(null, typeof(Expression))),
//                Expression.IfThenElse(Expression.MakeBinary(ExpressionType.NotEqual, enumerator.PropertyExpr("Current").PropertyExpr("attr"), Expression.Constant(null, typeof(object))),
//                    Expression.Block(
//                        Expression.IfThen(Expression.TypeIs(enumerator.PropertyExpr("Current").PropertyExpr("attr"), typeof(RegSubKeyName)),
//                            Expression.Block(
//                                Expression.Call(memberbinds, typeof(List<MemberBinding>).GetMethod("Add"),
//                                Expression.Call(typeof(Expression).GetMethod("Bind", new Type[] { typeof(MemberInfo), typeof(Expression) }),
//                                    enumerator.PropertyExpr("Current").PropertyExpr("x"),
//                                    Expression.Call(Expression.TypeAs(enumerator.PropertyExpr("Current").PropertyExpr("attr"), typeof(RegSubKeyName)), typeof(RegSubKeyName).GetMethod("ToExpression"), property, Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), regkey))))
//                                )),
//                        Expression.IfThen(Expression.TypeIs(enumerator.PropertyExpr("Current").PropertyExpr("attr"), typeof(RegPropertyName))
//                            , Expression.Assign(name, Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), Expression.TypeAs(enumerator.PropertyExpr("Current").PropertyExpr("attr"), typeof(RegPropertyName)).PropertyExpr("Name")))),
//                        "attr != null".WriteLineExpr()
//                        ),
//                        Expression.Block(
//                            Expression.Assign(name, Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("Name")))
//                        )),
//                Expression.IfThen(Expression.MakeBinary(ExpressionType.NotEqual, name, Expression.Constant(null, typeof(Expression))),
//                    Expression.IfThenElse(Expression.MakeBinary(ExpressionType.Equal, isconvert, Expression.Constant(true)),
//                    Expression.Block(
//                        "Convert".WriteLineExpr(),
//                        Expression.Call(memberbinds, typeof(List<MemberBinding>).GetMethod("Add"),
//                        Expression.Call(typeof(Expression).GetMethod("Bind", new Type[] { typeof(MemberInfo), typeof(Expression) }),
//                            enumerator.PropertyExpr("Current").PropertyExpr("x"),
//                            Expression.Call(typeof(Expression).GetMethod("Convert", new Type[] { typeof(Expression), typeof(Type) }),
//                                Expression.Call(typeof(Expression).GetMethod("Call", new Type[] { typeof(MethodInfo), typeof(Expression), typeof(Expression) }),
//                                    Expression.Call(Expression.Constant(reg_getvalue), typeof(MethodInfo).GetMethod("MakeGenericMethod"), Expression.NewArrayInit(typeof(Type), property)),
//                                    Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), regkey),
//                                    Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("Name"))), enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("PropertyType"))))
//                        ),
//                    Expression.Call(memberbinds, typeof(List<MemberBinding>).GetMethod("Add"),
//                        Expression.Call(typeof(Expression).GetMethod("Bind", new Type[] { typeof(MemberInfo), typeof(Expression) }),
//                            enumerator.PropertyExpr("Current").PropertyExpr("x"),
//                            Expression.Call(typeof(Expression).GetMethod("Call", new Type[] { typeof(MethodInfo), typeof(Expression), typeof(Expression) }),
//                                Expression.Call(Expression.Constant(reg_getvalue), typeof(MethodInfo).GetMethod("MakeGenericMethod"), Expression.NewArrayInit(typeof(Type), property)),
//                                Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), regkey),
//                                Expression.Call(typeof(Expression).GetMethod("Constant", new Type[] { typeof(object) }), enumerator.PropertyExpr("Current").PropertyExpr("x").PropertyExpr("Name"))))))),
//                memberbinds.PropertyExpr("Count").WriteLineExpr()
//                ),
//            Expression.Break(label)
//                )
//            ),
//    label),


//    Expression.Call(typeof(Expression).GetMethod("MemberInit", new Type[] { typeof(NewExpression), typeof(System.Linq.Expressions.MemberBinding[]) })
//        , Expression.Call(typeof(Expression).GetMethod("New", new Type[] { typeof(ConstructorInfo) }), Expression.Constant(typeof(T).GetConstructors()[0]))
//        , Expression.Call(memberbinds, typeof(List<MemberBinding>).GetMethod("ToArray")))

//    );

        }

        static void Main(string[] args)
        {
            //TypeCode cc = TypeCode.Boolean;
            //cc.ToString();
            //var mms = typeof(TypeCode).GetMethods().Where(x => x.Name == "ToString").Select(x => new
            //{
            //    x,
            //    pps = x.GetParameters()
            //});
            //var mm = typeof(TypeCode).GetMethod("ToString", new Type[] { });
            //mms = null;
            //Test();

            //add2(1, 2);
            //add3(1, 2);


            //TestDB();

            //return;
            //var g1 = zip_method.GetType().GetGenericArguments();
            ////var g2 = zip_method.GetType().GetGenericParameterConstraints();
            //var g3 = zip_method.GetType().GetGenericTypeDefinition();
            List<AppData> apps = new List<AppData>();
            apps.Add(new AppData() { Name = "A", IsOfficial = true });
            apps.Add(new AppData() { Name = "AA", IsOfficial = false });
            //apps.Add(new App() { Name = "B", Offical = true });
            //apps.Add(new App() { Name = "BB", Offical = false });
            //apps.Add(new App() { Name = "C", Offical = true });
            //apps.Add(new App() { Name = "CC", Offical = false });
            //apps.Add(new App() { Name = "D", Offical = true });
            //apps.Add(new App() { Name = "DD", Offical = false });

            try
            {
                var regt_h = new RegQuery<A>()
                    .useSetting(x =>
                    {
                        x.Hive = RegistryHive.CurrentConfig;
                        x.View = RegistryView.Registry64;
                        x.SubKey = @"Users";
                    });
                //regt_h.Backup("Users");
                //regt_h.Restore("Users");

                //RegistryKey temp;
                //var testkey = Registry.CurrentConfig.OpenSubKey(@"hierarchy", true);
                //var query = testkey.GetSubKeyNames().Select(x => testkey.OpenSubKey(x)).AsQueryable();
                //var bb = query.Select(x => Build<A>(x)).ToArray();
                //var valuenames = testkey.GetValueNames();
                //foreach (var oo in valuenames)
                //{
                //    var kind = testkey.GetValueKind(oo);
                //    var vlu = testkey.GetValue(oo);
                //}
                //var regs = testkey.FindAll(x => x.GetValue<string>("port") != "").ToList();
                //var names = testkey.GetSubKeyNames();



                //var testkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                //var query = testkey.GetSubKeyNames().Select(x => testkey.OpenSubKey(x)).AsQueryable();
                //var syntax = from oo in query
                //             where oo.GetValue<string>("DisplayName") != ""
                //             let kk1 = oo.GetValue<string>("DisplayName").ToLower()
                //             let kk2 = oo.GetValue<string>("DisplayName").ToUpper()
                //             select new { d1 = oo.GetValue<string>("DisplayName"), d2 = kk1, d3 = kk2 };


                //var qq = query.Where(x => x.GetValue<string>("DisplayName") != "")
                //    .Select(x => new { x, kk1 = x.GetValue<string>("DisplayName").ToLower() })
                //    .Select(x => new { x1 = x.x, x2 = x, kk2 = x.x.GetValue<string>("DisplayName").ToUpper() })
                //    .Select(x => new { d1=x.x1.GetValue<string>("DisplayName"), d2=x.x2.kk1, d3=x.kk2 });



                //var pps = dst.GetProperties().Where(x => x.CanWrite == true)
                //.Select(x => new
                //{
                //    x,
                //    attr = x.GetCustomAttributes(true).FirstOrDefault(y => y is RegSubKeyName || y is RegIgnore || y is RegPropertyName)
                //}).Where(x => !(x.attr is RegIgnore));


                //Build<People>(null);
                

                var peoplekey = Registry.CurrentConfig.OpenSubKey(@"people");
                var peoplekeys = peoplekey.GetSubKeyNames().Select(x => peoplekey.OpenSubKey(x));
                var iuyt = peoplekeys.First().OpenSubKey("phone\\home");
                var peoples_query = peoplekeys.AsQueryable();
                //Build<People>(peoplekeys.First());
                //BuildExpr<People>(peoplekeys.First());
                //var peopels = peoples_query.Select(x => Build<People>(x));
                //foreach (var oo in peopels)
                //{

                //}


                var tryfin = Expression.TryFinally("123".WriteLineExpr(), "321".WriteLineExpr());
                People pp = new People();
                var nn = pp.phone?.home?.name;
                var regt_people = new RegQuery<People>()
                    .useSetting(x=> 
                    {
                        x.View = RegistryView.Registry64;
                        x.Hive = RegistryHive.CurrentConfig;
                        x.SubKey = "people";
                    });
                var peopel_where = regt_people.Where(x => x.phone.home.name != "");
                foreach(var oo in peopel_where)
                {

                }
            }
            catch (Exception ee)
            {
                System.Diagnostics.Trace.WriteLine(ee.Message);
            }

            //string user = Environment.UserDomainName + "\\" + Environment.UserName;
            //RegistrySecurity rs = new RegistrySecurity();

            //// Allow the current user to read and delete the key.
            ////
            //rs.AddAccessRule(new RegistryAccessRule(user,
            //    RegistryRights.ReadKey | RegistryRights.Delete,
            //    InheritanceFlags.None,
            //    PropagationFlags.None,
            //    AccessControlType.Allow));

            //rs.AddAccessRule(new RegistryAccessRule(user,
            //RegistryRights.WriteKey | RegistryRights.ReadKey | RegistryRights.Delete | RegistryRights.ChangePermissions,
            //InheritanceFlags.None,
            //PropagationFlags.None,
            //AccessControlType.Deny));

            //// Create the example key with registry security.
            //RegistryKey rk = null;
            //try
            //{
            //    rk = Registry.CurrentUser.CreateSubKey("RegistryRightsExample",
            //        RegistryKeyPermissionCheck.Default, rs);
            //    Console.WriteLine("\r\nExample key created.");
            //    rk.SetValue("ValueName", "StringValue");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("\r\nUnable to create the example key: {0}", ex);
            //}
            //if (rk != null) rk.Close();


            //rk = Registry.CurrentUser;

            //RegistryKey rk2;

            //// Open the key with read access.
            //rk2 = rk.OpenSubKey("RegistryRightsExample", false);
            //Console.WriteLine("\r\nRetrieved value: {0}", rk2.GetValue("ValueName"));
            //rk2.Close();

            //// Attempt to open the key with write access.
            //try
            //{
            //    rk2 = rk.OpenSubKey("RegistryRightsExample", true);
            //}
            //catch (SecurityException ex)
            //{
            //    Console.WriteLine("\nUnable to write to the example key." +
            //        " Caught SecurityException: {0}", ex.Message);
            //}
            //if (rk2 != null) rk2.Close();

            //// Attempt to change permissions for the key.
            //try
            //{
            //    rs = new RegistrySecurity();
            //    rs.AddAccessRule(new RegistryAccessRule(user,
            //        RegistryRights.WriteKey,
            //        InheritanceFlags.None,
            //        PropagationFlags.None,
            //        AccessControlType.Allow));
            //    rk2 = rk.OpenSubKey("RegistryRightsExample", false);
            //    rk2.SetAccessControl(rs);
            //    Console.WriteLine("\r\nExample key permissions were changed.");
            //}
            //catch (UnauthorizedAccessException ex)
            //{
            //    Console.WriteLine("\nUnable to change permissions for the example key." +
            //        " Caught UnauthorizedAccessException: {0}", ex.Message);
            //}
            //if (rk2 != null) rk2.Close();

            //Console.WriteLine("\r\nPress Enter to delete the example key.");
            //Console.ReadLine();

            //try
            //{
            //    rk.DeleteSubKey("RegistryRightsExample");
            //    Console.WriteLine("Example key was deleted.");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Unable to delete the example key: {0}", ex);
            //}

            //rk.Close();


            


            InstalledApp installedapp = new InstalledApp();
            var pps = typeof(InstalledApp).GetProperties().Where(x=>x.CanRead==true&&x.CanWrite == true);
            foreach(var pp in pps)
            {
                var pv = pp.GetValue(installedapp, null);
            }


            var regt = new RegQuery<InstalledApp>()
                .HasDefault1(x=>
                {
                    x.Now = DateTime.Now;
                    x.DisplayName = "AA";
                })
                .HasDefault(() => new InstalledApp()
                {
                    Now = DateTime.Now,
                    DisplayName = "A"
                })
                .useSetting(x =>
                { 
                    x.Hive = RegistryHive.LocalMachine;
                    x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                    x.View = RegistryView.Registry64;
                });

            var sytnax = from oo in regt
                         where oo.DisplayName != null
                         let kk1 =oo.DisplayName.ToUpper()
                         let kk2 = oo.DisplayName.ToLower()
                         select oo;
            foreach(var oo in sytnax)
            {

            }
            var testq = regt.Where(x => x.DisplayName.Contains("A")).ToList();
            var tolist = testq.ToList();
            var dictionary = testq.ToLookup(x => x.DisplayName);
            regt.GroupBy(x => x.DisplayName, x => x.DisplayName).ToList();
            var sel = regt.Select(x => new { x.DisplayName, x.Version });
            var where_version = regt.Where(x => x.Version > new Version(1, 1, 1, 1));
            List<InstalledApp> m_Tests = new List<InstalledApp>();
            //m_Tests.Add(new InstalledApp());
            m_Tests.AddRange(regt.Take(2).ToList());
            var zip1 = regt.Zip(m_Tests, (reg, app) => new {reg, app });
            var groupby1 = regt.GroupBy(x => x).Select(x => x.Key);
            var where1 = regt.Where(x => x.DisplayName != "").Select(x=>x.DisplayName);
            foreach(var oo in zip1)
            {

            }
            //var tolist1 = regt.Where(x => x.DisplayName.Contains("A")).Count();
            //var tolist11 = regt.Where(x => x.DisplayName!="").ToList();
            var tolist2 = regt.ToDictionary(x => x.DisplayName);
            regt.RemoveAll();
            //regt.Insert(new List<InstalledApp>()
            //{
            //    new InstalledApp(){DisplayName="AA", Version=new Version(1,1,1,1), EstimatedSize=100, Key="1" },
            //    new InstalledApp(){DisplayName="BB", Version=new Version(2,2,2,2),EstimatedSize=101, Key="2" },
            //    new InstalledApp(){DisplayName="CC", Version=new Version(3,3,3,3),EstimatedSize=102, Key="3" },
            //    new InstalledApp(){DisplayName="DD", Version=new Version(4,4,4,4),EstimatedSize=103, Key="4" },
            //    new InstalledApp(){DisplayName="EE", Version=new Version(5,5,5,5),EstimatedSize=104, Key="5" },
            //});

            //var regt1 = new RegQuery<InstalledApp>()
            //    .useSetting(x =>
            //        {
            //            x.Hive = RegistryHive.LocalMachine;
            //            x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
            //            x.View = RegistryView.Registry64;
            //        });
            


        }

        public static void TestDB()
        {
            RegQuery<AppMapping> regt_appmapping = new RegQuery<AppMapping>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\AppMapping";
                x.View = RegistryView.Registry64;
            });

            RegQuery<Company> regt_company = new RegQuery<Company>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTest\Company";
                    x.View = RegistryView.Registry64;
                });


            RegQuery<App> regt_apps = new RegQuery<App>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\Apps";
            });
            //regt_company.RemoveAll();
            //regt_company.Insert(new List<Company>()
            //{
            //    new Company(){Name = "Company_A", ID=1, Key=100, OrderBy=6, ThenBy=100, ThenBy1=205},
            //    new Company(){Name = "Company_B", ID=2, Key=101, OrderBy=7, ThenBy=100, ThenBy1=204},
            //    new Company(){Name = "Company_C", ID=3, Key=102, OrderBy=8, ThenBy=90, ThenBy1=203},
            //    new Company(){Name = "Company_D", ID=4, Key=103, OrderBy=9, ThenBy=90, ThenBy1=202},
            //    new Company(){Name = "Company_E", ID=5, Key=104, OrderBy=10, ThenBy=90, ThenBy1=201},
            //});
            //var oedereby = regt_company.OrderBy(x => x.OrderBy);
            //var thenby = oedereby.ThenBy(x => x.ThenBy);
            //regt_apps.RemoveAll();
            //regt_apps.Insert(new List<InstalledApp>()
            //{
            //    new InstalledApp() { Key="AA", DisplayName = "AA", Version = new Version("1.1.1.1"), EstimatedSize = 10, ID=0},
            //    new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.2"), EstimatedSize = 10, ID = 1 },
            //    new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.3"), EstimatedSize = 10, ID = 2 },
            //    new InstalledApp() { Key = "BB", DisplayName = "BB", Version = new Version("2.2.2.2"), EstimatedSize = 20, ID = 3 },
            //    new InstalledApp() { Key = "CC", DisplayName = "CC", Version = new Version("3.3.3.3"), EstimatedSize = 30, ID = 4 },
            //    new InstalledApp() { Key = "DD", DisplayName = "DD", Version = new Version("4.4.4.4"), EstimatedSize = 40, ID = 5 },
            //    new InstalledApp() { Key = "EE", DisplayName = "EE", Version = new Version("5.5.5.5"), EstimatedSize = 50, ID = 6 },
            //    new InstalledApp() { Key = "FF", DisplayName = "FF", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 7 }
            //});

            //regt_appmapping.RemoveAll();
            //regt_appmapping.Insert(new List<AppMapping>()
            //{
            //    new AppMapping(){AppID = 0, CompanyID = 1},
            //    new AppMapping(){AppID = 1, CompanyID = 1},
            //    new AppMapping(){AppID = 2, CompanyID = 1},
            //    new AppMapping(){AppID = 3, CompanyID = 1},
            //    new AppMapping(){AppID = 4, CompanyID = 2},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //    new AppMapping(){AppID = 33, CompanyID = 1},
            //});


            //var applist = from company in regt_company where company.ID==1 select new { company };
            //var applist = from company in regt_company
            //              join appmapping in regt_appmapping
            //              on company.ID equals appmapping.CompanyID into gj
            //              from subpet in gj.DefaultIfEmpty()
            //              select new { company.ID, company.Name, appid=subpet.AppID};
            //var gg = regt_company.Join(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y })
            //    .Join(regt_apps, x => x.y.AppID, y => y.ID, (x, y) => new { x, y }).GroupBy(x=>x.x.x.Name);

            //var mapping = regt_appmapping.ToList();
            //regt_appmapping.Select((x, index) => new { x, index});
            //regt_appmapping.Select(x => new { x }).ToList();
            //regt_appmapping.Join(regt_apps, x => x.AppID, y => y.ID, (x, y) => new { x, y }).ToList();

            //var join = regt_company.Join(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new {x,y });
            //var gj = regt_company.GroupJoin(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y }).ToList();
            //var comp = regt_company.First(x=>x.Name == "Company_A");

            var queryreg_company = RegistryHive.CurrentConfig.OpenView64(@"UnitTest\Company", false).ToList().AsQueryable();
            var queryreg_appmapping = RegistryHive.CurrentConfig.OpenView64(@"UnitTest\AppMapping", false).ToList().AsQueryable();
            var rr = queryreg_company.GroupJoin(queryreg_appmapping, a => a.GetValue<string>("ID"), b => b.GetValue<string>("CompanyID"), (c, d) => new { c, d })
                .SelectMany(e => e.d.DefaultIfEmpty(), (f, g) => new { f.c, g })
                .Select(h => new
                {
                    f = new Company()
                    {
                        Name = h.c.GetValue<string>("Name")
                    },
                    g = h.g==null?null:new AppMapping()
                    {
                        CompanyID = h.g.GetValue<int>("CompanyID")
                    }
                });
            var methodcall = rr.Expression as MethodCallExpression;
            var unary = methodcall.Arguments[1] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;
            var new1 = lambda.Body as NewExpression;
            var ifelse = new1.Arguments[1] as ConditionalExpression;
            var binary = ifelse.Test as BinaryExpression;
            var yu = ifelse.Test.GetType();
            var oioi  = Expression.Condition(ifelse.Test, ifelse.IfTrue, ifelse.IfFalse);
            var sel = regt_company.ToList();
            var left1 = from company in regt_company
                        join mapping in regt_appmapping on company.Key equals mapping.CompanyID into gj_company_mapping
                        from company_mapping in gj_company_mapping.DefaultIfEmpty()
                        where company_mapping != null
                        join app in regt_apps on company_mapping.AppID equals app.ID into gj_app_mapping
                        from app_mapping in gj_app_mapping.DefaultIfEmpty()
                        select new
                        {
                            company = company.Key,
                            app = app_mapping.ID
                        };
            foreach (var oo in left1)
            {

            }

            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var companys = regt_company.ToList();
            var left22 = from company in companys
                        join mapping in mappings on company.ID equals mapping.CompanyID into gj_company_mapping
                        from company_mapping in gj_company_mapping.DefaultIfEmpty()
                        where company_mapping != null
                        join app in apps on company_mapping.AppID equals app.ID into gj_app_mapping
                        from app_mapping in gj_app_mapping.DefaultIfEmpty()
                        select new
                        {
                            company = company.ID,
                            app = app_mapping.ID
                        };
            foreach (var oo in left22)
            {

            }

            //var left2 = regt_company.GroupJoin(regt_appmapping, a => a.ID, b => b.CompanyID, (c, d) => new { c, d })
            //    .SelectMany(e => e.d.DefaultIfEmpty(), (f, g) => new { f.c, g });

            ////remove no mapping appmapping
            //var appaming = regt_appmapping.GroupJoin(regt_apps, x => x.AppID, y => y.ID, (mapping, app) => new { mapping, app })
            //    .SelectMany(e => e.app.DefaultIfEmpty(), (mapping, app) => new { mapping.mapping, app })
            //    .Where(x => x.app == null)
            //    .Select(x => x.mapping);


            //var app1 = regt_apps.ToList();
            //var map = regt_appmapping.ToList();
            //var nu = map.GroupJoin(app1, x => x.AppID, y => y.ID, (mapping, app) => new { mapping, app })
            //    .Where(x => x.app.Any() == false).Select(x=>x.mapping);


            //var appaming1 = regt_appmapping.GroupJoin(regt_apps, x => x.AppID, y => y.ID, (mapping, app) => new { mapping, app })
            //    .Where(x => x.app.Any() == false).Select(x => x.mapping).RemoveAll();

            //.RemoveAll();

            //var groupjoin = regt_company.GroupJoin(regt_appmapping, a => a.ID, b => b.CompanyID, (c, d) => new { c, d });
            //var selectmany = groupjoin.SelectMany(e => e.d.DefaultIfEmpty(), (f, g) => new { comapny=f.c, mapping=g });


            //var nulldata_company = selectmany.Where(x => x.mapping == null).Select(x=>x.comapny);
            //nulldata_company.InsertTo(regt_appmapping, x => new AppMapping() { CompanyID=x.ID });
            ////removenall.RemoveAll();
            //foreach (var oo in groupjoin)
            //{

            //}

            var left2 = regt_company.GroupJoin(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => new { x, y })
                .SelectMany(a => a.y.DefaultIfEmpty(), (x, y) => new { x.x, y }).ToList();

            //var left1 = from company in regt_company
            //           join map in regt_appmapping on company.ID equals map.CompanyID into temp
            //           from mapresult in temp.DefaultIfEmpty()
            //           select new { company, mapresult };
            //left1.Where(x => x.mapresult == null).Select(x => x.company).RemoveAll();


            //var left11 = (from map in regt_appmapping
            //              join comapny in regt_company on map.CompanyID equals comapny.ID into companys
            //              from companyhr in companys.DefaultIfEmpty()
            //              join app in regt_apps on map.AppID equals app.ID into apps
            //              from apphr in apps.DefaultIfEmpty()
            //              select new { companyhr.Name, apphr.DisplayName });

            //var inner = from company in regt_company
            //            from appmapping in regt_appmapping
            //            from app in regt_apps
            //            where company.ID == appmapping.CompanyID && appmapping.AppID == app.ID
            //            select new { company.Name };
        }
    }

    public class AppData
    {
        public AppData()
        {

        }

        public AppData(string name)
        {
            this.Name = name;
        }
        public string Name { set; get; }
        public string Ver { set; get; }
        public string Uninstallstring { set; get; }
        public bool IsOfficial { set; get; }
    }


    public class BIOS
    {
        [RegSubKeyName]
        public string Key { set; get; }
        public string BaseBoardManufacturer { set; get; }
        public string BaseBoardProduct { set; get; }
        public string BaseBoardVersion { set; get; }
        public uint BiosMajorRelease { set; get; }
        public int BiosMinorRelease { set; get; }
        public string BIOSReleaseDate { set; get; }
        public string BIOSVendor { set; get; }
        public string BIOSVersion { set; get; }
        public int ECFirmwareMajorRelease { set; get; }
        public int ECFirmwareMinorRelease { set; get; }
        public string SystemFamily { set; get; }
        public string SystemManufacturer { set; get; }
        public string SystemProductName { set; get; }
        public string SystemSKU { set; get; }
        public string SystemVersion { set; get; }
    } 
}
