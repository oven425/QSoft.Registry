#define Queryable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    }

    public class People
    {
        [RegIgnore]
        public string Name { set; get; }

        public int Weight { set; get; } 
        public int Height { set; get; }
        //public Phone phones { set; get; }
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

            //foreach (var pp in pps)
            //{
            //    var typecode = Type.GetTypeCode(pp.x.PropertyType);
            //    if (typecode == TypeCode.Object)
            //    {
            //        if (reg.GetSubKeyNames().Any(x => x == pp.x.Name) == true)
            //        {
            //            var methd = typeof(Program).GetMethods().Where(x => x.Name == "Build");
            //            var subobj = methd.First().MakeGenericMethod(pp.x.PropertyType).Invoke(null, new object[] { reg.OpenSubKey(pp.x.Name) });
            //            pp.x.SetValue(obj, subobj, null);
            //        }
            //    }
            //    else
            //    {
            //        var vv = typeof(RegistryKeyEx).GetMethods().Where(x => x.Name == "GetValue");
            //        var h = vv.First().MakeGenericMethod(pp.x.PropertyType).Invoke(null, new object[] { reg, pp.x.Name });
            //        pp.x.SetValue(obj, h, null);
            //    }
            //}

            return obj;
        }


        //https://www.codeproject.com/Articles/107477/Use-of-Expression-Trees-in-NET-for-Lambda-Decompos
        static void BuildExpr<T>(RegistryKey reg)
        {
            try
            {
                var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);
                var reg_getvalue = regexs.First();
                var testrxpr = typeof(T).GetProperties().AsQueryable().Where(x => x.PropertyType.IsGenericType == true && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    .Select(x=>x.PropertyType.GetGenericArguments()[0]);
                var cc = typeof(T).GetConstructors()[0];
                var newexpr = Expression.New(cc);
                var pps = typeof(T).GetProperties().AsQueryable().Where(x => x.CanWrite == true)
                .Select(x => new
                {
                    x,
                    attr = x.GetCustomAttributes(true).FirstOrDefault(y => y is RegSubKeyName || y is RegIgnore || y is RegPropertyName)
                }).Where((x,i)=>!(x.attr is RegIgnore));
                var enumerator1 = pps.GetEnumerator();
                while (true)
                {
                    if (enumerator1.MoveNext() == false)
                    {
                        break;
                    }
                    var current = enumerator1.Current;
                    var typecode = Type.GetTypeCode(current.x.PropertyType);
                    var property = current.x.PropertyType;
                    if (property.IsGenericType == true && property.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        property = property.GetGenericArguments()[0];
                        typecode = Type.GetTypeCode(property);
                    }
                    System.Diagnostics.Trace.WriteLine(typecode);
                }
                var vv = typeof(People).GetProperties().AsQueryable().Where(x => x.CanWrite == true);
                var m = (vv.Expression as MethodCallExpression);
                var t = Expression.Constant(typeof(T), typeof(Type));
                var mm = typeof(Type).GetMethod("GetProperties", new Type[] { });
                var getproperty_expr = Expression.Call(t, mm);
                Expression<Func<PropertyInfo[]>> ll = Expression.Lambda<Func<PropertyInfo[]>>(getproperty_expr);
                var oi = ll.Compile();
                var aaaa = oi();


                var parameter = Expression.Parameter(typeof(PropertyInfo), "x");
                var p = Expression.Property(parameter, "CanWrite");
                var binary = Expression.MakeBinary(ExpressionType.Equal, p, Expression.Constant(true, typeof(bool)));
                var lambda = Expression.Lambda(binary, parameter);

                var where_expr = ExpressionEx.Where(getproperty_expr, lambda);
                var t1 = Expression.Lambda<Func<IEnumerable<PropertyInfo>>>(where_expr);

                var aa = new List<Tuple<Type, string>>() { Tuple.Create(typeof(PropertyInfo), "x"), Tuple.Create(typeof(object), "attr")};
                var select_type= aa.BuildType(null);
                var pos = select_type.GetProperties().Where(x => x.CanWrite == true);
                var select_param = Expression.Parameter(typeof(PropertyInfo), "x");
                List<Expression> exprs = new List<Expression>();
                foreach(var po in pos)
                {
                    switch(po.Name)
                    {
                        case "x":
                            {
                                exprs.Add(select_param);
                            }
                            break;
                        case "attr":
                            {
                                var method = typeof(PropertyInfo).GetMethod("GetCustomAttributes", new Type[] {typeof(bool) });
                                var getcust_expr = Expression.Call(select_param, method, Expression.Constant(true));
                                var first_param = Expression.Parameter(typeof(object), "x");
                                var is_regsubkey = Expression.TypeIs(first_param, typeof(RegSubKeyName));
                                var is_regignore = Expression.TypeIs(first_param, typeof(RegIgnore));
                                var is_regproperty = Expression.TypeIs(first_param, typeof(RegPropertyName));
                                var or1 = Expression.MakeBinary(ExpressionType.Or, is_regsubkey, is_regignore);
                                or1 = Expression.MakeBinary(ExpressionType.Or, or1, is_regproperty);
                                var first_lambda = Expression.Lambda(or1, first_param);
                                var first_expr = ExpressionEx.FirstOrDefault(getcust_expr, first_lambda);
                                exprs.Add(first_expr);
                            }
                            break;
                    }
                }
                var ccs = select_type.GetConstructors();
                var owi = ccs[0].GetParameters();
                var expr_new = Expression.New(select_type.GetConstructors()[0], exprs);
                var select_lambda = Expression.Lambda(expr_new, select_param);
                var select_expr = ExpressionEx.Select(where_expr, select_lambda);

                var where_param = Expression.Parameter(select_expr.Type.GetGenericArguments()[0], "x");
                var where_param1 = Expression.Property(where_param, "attr");
                var where_binary = Expression.MakeBinary(ExpressionType.NotEqual, Expression.TypeIs(where_param1, typeof(RegIgnore)), Expression.Constant(true));

                var where_expr1 = ExpressionEx.Where(select_expr, Expression.Lambda(where_binary, where_param));
                //var uy = typeof(Func<>).MakeGenericType(select_expr.Type);
                //var mmmm = typeof(Expression).GetMethods().Where(x => x.Name == "Lambda"&&x.IsGenericMethod).First().MakeGenericMethod(uy);
                //var pp_func = Expression.Lambda<uy.GetType()>(where_expr1);
                //var sss = Expression.Lambda(where_expr1).Compile();
                //var pou = sss.DynamicInvoke();
                mm = null;

                ParameterExpression enumerator = Expression.Variable(typeof(IEnumerator<>).MakeGenericType(where_expr1.Type.GetGenericArguments()[0]), "enumerator");
                var getee = Expression.Call(where_expr1, where_expr1.Type.GetMethod("GetEnumerator"));
                BinaryExpression assignenumerator = Expression.Assign(enumerator, getee);


                MethodCallExpression movenext_expr = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));


                var currentelement = Expression.Parameter(where_expr1.Type.GetGenericArguments()[0], "current");
                var propertyelement = Expression.Parameter(typeof(Type), "property");
                var typecodeelement = Expression.Parameter(typeof(TypeCode), "typecode");
                var bindingselement = Expression.Parameter(typeof(List<MemberAssignment>), "bindings");
                var nameelement = Expression.Parameter(typeof(string), "name");
                var xelement = Expression.Parameter(typeof(PropertyInfo), "x");

                //var propertytype_var = Expression.Parameter(typeof(Type), "property");
                //var propertytype_expr = Expression.Property(Expression.Property(currentelement, "x"), "PropertyType");
                //var assignproperty = Expression.Assign(propertytype_var, propertytype_expr);

                var typecode_method = typeof(Type).GetMethod("GetTypeCode");
                var typecode_expr = Expression.Call(typecode_method, propertyelement);

                var cch1 = Expression.MakeBinary(ExpressionType.Equal, Expression.Property(propertyelement, "IsGenericType"), Expression.Constant(true));
                var oioi = Expression.Call(propertyelement, propertyelement.Type.GetMethod("GetGenericTypeDefinition"));
                var cch2 = Expression.MakeBinary(ExpressionType.Equal, oioi, Expression.Constant(typeof(Nullable<>)));

                oioi = Expression.Call(propertyelement, propertyelement.Type.GetMethod("GetGenericArguments"));
                var aaa1a = Expression.ArrayIndex(oioi, Expression.Constant(0));
                
                var ifsblock = Expression.Block(Expression.Assign(propertyelement, aaa1a), Expression.Assign(typecodeelement, typecode_expr));
                //property = current.x.PropertyType.GetGenericArguments()[0];
                //typecode = Type.GetTypeCode(property);

                var writeline_method = typeof(System.Diagnostics.Trace).GetMethod("WriteLine", new Type[] { typeof(object) });
                var if_nullable_expr = Expression.IfThen(Expression.MakeBinary(ExpressionType.AndAlso, cch1, cch2), ifsblock);

                var attr = Expression.Property(currentelement, "attr");
                var condition1 = Expression.MakeBinary(ExpressionType.NotEqual, attr, Expression.Constant(null, typeof(object)));

                var elsename_expr = Expression.Property(Expression.Property(currentelement, "x"), "Name");
                var ifregname_expr = Expression.IfThenElse(Expression.MakeBinary(ExpressionType.AndAlso, condition1, Expression.TypeIs(attr, typeof(RegPropertyName))), Expression.TypeAs(attr, typeof(RegPropertyName)), Expression.Assign(nameelement, elsename_expr));
                var ifregsuby_expr = Expression.IfThenElse(Expression.MakeBinary(ExpressionType.AndAlso, condition1, Expression.TypeIs(attr, typeof(RegSubKeyName))), Expression.TypeAs(attr, typeof(RegSubKeyName)), ifregname_expr);
                

                var methodcall = Expression.Call(null, writeline_method, typecodeelement.ToStringExpr());
                var callCurrent = Expression.Assign(currentelement, Expression.Property(enumerator, "Current"));
                var callProperty = Expression.Assign(propertyelement, Expression.Property(Expression.Property(currentelement, "x"), "PropertyType"));
                var callTyepCode = Expression.Assign(typecodeelement, typecode_expr);
                var mameCode = Expression.Assign(nameelement, Expression.Constant(null, typeof(string)));
                var xCode = Expression.Assign(xelement, Expression.Constant(null, typeof(PropertyInfo)));
                var bindingsCode = Expression.Assign(bindingselement, Expression.New(typeof(List<MemberAssignment>)));

                LabelTarget loop_expr = Expression.Label("break");
                var break_expr = Expression.Break(loop_expr);
                var movenext_expr_hr = Expression.NotEqual(movenext_expr, Expression.Constant(false));

                var getvalue_method = typeof(RegistryKey).GetMethod("GetValue", new Type[] { typeof(string)});
                var getvalue_expr = Expression.Call(Expression.Constant(reg, typeof(RegistryKey)), getvalue_method, nameelement);
                var writelinename_expr = Expression.Call(writeline_method, nameelement);
                var writeline_expr = Expression.Call(writeline_method, getvalue_expr);

                var bindingeee = Expression.Parameter(typeof(Expression), "method");
                var bindexprCode = Expression.Assign(bindingeee, Expression.Constant(null, typeof(Expression)));
                var add_method = typeof(List<MemberAssignment>).GetMethod("Add");
                //var bindings_expr = Expression.New(typeof(List<MemberAssignment>));
                var mmes = typeof(Expression).GetMethod("Bind", new Type[] { typeof(MemberInfo), typeof(Expression)});


                //Expression.Assign(bindingeee, getvalue_expr);
                var iiu = Expression.Constant(getvalue_expr);
                var bind_expr = Expression.Call(mmes, xelement,Expression.Constant(getvalue_expr));
                //var addbind_expr = Expression.Call(bindingselement, add_method, bind_expr);

                var build_block_expr = Expression.Block(new ParameterExpression[] { bindingeee }, bindexprCode, bind_expr);

                //var if_nullable_expr1 = Expression.IfThen(Expression.MakeBinary(ExpressionType.AndAlso, cch1, cch2), build_block_expr);

                //var if_buid_expr = Expression.IfThen(Expression.MakeBinary(ExpressionType.NotEqual, nameelement, Expression.Constant(null, typeof(string))), if_nullable_expr1);

                var dd = Expression.IfThenElse(movenext_expr_hr, Expression.Block(
                    new ParameterExpression[] { propertyelement, typecodeelement, nameelement, xelement }
                , callCurrent, callProperty, callTyepCode, xCode
                , if_nullable_expr, mameCode, ifregsuby_expr, bind_expr, writelinename_expr, writeline_expr, methodcall), break_expr);
                
                var loop = Expression.Loop(dd, loop_expr);
                var block = Expression.Block(new ParameterExpression[] { enumerator, currentelement, bindingselement }
                , assignenumerator,  loop, bindingsCode);

                var catch_ee = Expression.Parameter(typeof(Exception), "ee");
                var asdd = typeof(Trace).GetMethod("WriteLine", new Type[] { typeof(string)});
                var catch_ee_expr = Expression.Call(asdd, Expression.Property(catch_ee, "Message"));
                var catch_expr = Expression.Catch(catch_ee, catch_ee_expr);
                var trycatch_expr = Expression.TryCatch(block, catch_expr);
                var loopfunc = Expression.Lambda(trycatch_expr).Compile();
                loopfunc.DynamicInvoke();
                loopfunc = null;


                //ParameterExpression enumerableExpression = Expression.Parameter(typeof(IEnumerable<PropertyInfo>), "x");



                //ParameterExpression enumerator = Expression.Variable(typeof(IEnumerator<PropertyInfo>), "enumerator");
                //BinaryExpression assignenumerator = Expression.Assign(enumerator,
                //    Expression.Call(enumerableExpression, typeof(IEnumerable<PropertyInfo>).GetMethod("GetEnumerator")));


                //var currentelement = Expression.Parameter(typeof(PropertyInfo), "i");
                //var ss = typeof(System.Diagnostics.Trace).GetMethod("WriteLine", new Type[] { typeof(string) });
                //var pp = Expression.Property(currentelement, "Name");
                //var methodcall = Expression.Call(null, ss, pp);
                //var callCurrent = Expression.Assign(currentelement, Expression.Property(enumerator, "Current"));


                //MethodCallExpression movenext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

                //LabelTarget looplabel = Expression.Label("looplabel");
                //LabelTarget returnLabel = Expression.Label(typeof(bool), "retval");

                //BlockExpression block = Expression.Block(
                //        new ParameterExpression[] {enumerator, currentelement },
                //        assignenumerator,
                //        Expression.Loop(
                //            Expression.IfThenElse(
                //                Expression.NotEqual(movenext, Expression.Constant(false)),
                //                Expression.Block(callCurrent, methodcall),
                //                Expression.Break(looplabel)),
                //                looplabel));

                //Expression<Action<IEnumerable<PropertyInfo>>> lambda = Expression.Lambda<Action<IEnumerable<PropertyInfo>>>(block, enumerableExpression);

                //Action<IEnumerable<PropertyInfo>> dividesectionmethod = lambda.Compile();
                //dividesectionmethod(typeof(T).GetProperties());
            }
            catch (Exception ee)
            {
                System.Diagnostics.Trace.WriteLine(ee.Message);
            }
            
        }


        private static int AddMethod(int a, int b)
        {
            return a + b;
        }

        


        static private void Test()
        {
            Func<int, int, int> add1 = (a, b) => a + b;
            Func<int, int, int> add2 = AddMethod;

            var x = Expression.Parameter(typeof(int));
            var y = Expression.Parameter(typeof(int));
            var additionExpr = Expression.Add(x, y);
            Func<int, int, int> add3 =
                          Expression.Lambda<Func<int, int, int>>(
                              additionExpr, x, y).Compile();
            //the above steps cost a lot of time, relatively.

            //performance of these three should be identical
            int count = 9999999;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                add1(1, 2);
            }
            sw.Stop();
            var time = (double)count / sw.ElapsedMilliseconds;
            System.Diagnostics.Trace.WriteLine($"1. {time} {sw.ElapsedMilliseconds}");

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                add2(1, 2);
            }
            sw.Stop();
            time = (double)count / sw.ElapsedMilliseconds;
            System.Diagnostics.Trace.WriteLine($"2. {time} {sw.ElapsedMilliseconds}");

            sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                add3(1, 2);
            }
            sw.Stop();
            time = (double)count / sw.ElapsedMilliseconds;
            System.Diagnostics.Trace.WriteLine($"3. {time} {sw.ElapsedMilliseconds}");
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
                var peoples_query = peoplekeys.AsQueryable();
                BuildExpr<People>(peoplekeys.First());
                var peopels = peoples_query.Select(x => Build<People>(x));
                foreach (var oo in peopels)
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
