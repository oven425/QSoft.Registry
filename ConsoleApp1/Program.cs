using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinqFileSystemProvider;
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;

namespace ConsoleApp1
{
    public class CTabble1
    {
        
        public string Key1 { set; get; }
        public string Name1 { set; get; }
    }
    public class CTable2
    {
        public string Key2 { set; get; }
        public string Name2 { set; get; }
    }



    public class ExpressionVisitorA : ExpressionVisitor
    {
        IQueryable<Test> m_Datas;
        public ExpressionVisitorA(IQueryable<Test> data)
        {
            this.m_Datas = data;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if(node.Type == typeof(RegQuery<Test>))
            {
                return Expression.Constant(this.m_Datas);
            }
            else
            {
                return node;
            }
        }
    }

    public class Test
    {
        public string DisplayName { set; get; }
        public string DisplayVersion { set; get; }
        public int? EstimatedSize { set; get; }
    }

    class Program
    {
        private static Expression<Func<T, bool>> FuncToExpression<T>(Func<T, bool> f)
        {
            return x => f(x);
        }

        static public void Set(RegistryKey reg)
        {
            //reg.SetValue("a", "a");
            //reg.SetValue("b", "b");
            //return true;
        }

        static void Main(string[] args)
        {


            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);
            Func<int, bool> func = a => a == 1;
            bool b1 = func(1);

            

            

            var queryable = queryreg.ToList().AsQueryable();

            Action<RegistryKey> action = reg =>
            {
                reg.SetValue("a", "a");
                reg.SetValue("b", "b");
            };
            var method_set = typeof(Program).GetMethod("Set");
            var pp = Expression.Parameter(typeof(RegistryKey), "x");
            var methid_expression = Expression.Call(method_set, pp);
            var lambda = Expression.Lambda(methid_expression, pp);
            var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            var methodcall_param_0 = Expression.Constant(queryable);
            var update = typeof(RegQueryEx).GetMethods().Where(x=>x.Name == "Update1");
            var update1 = update.FirstOrDefault().MakeGenericMethod(typeof(RegistryKey));


            var methodcall1 = Expression.Call(update1, methodcall_param_0);



            int aa = queryable.Provider.Execute<int>(methodcall1);

            //var rr = queryable.Update1(lambda);

            //foreach (var oo in rr)
            //{

            //}

            //var wheres = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Where" && x.GetParameters().Length == 2);
            //var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);

            //var ttype = rr.GetType();
            //MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            //var methodcall_param_0 = methodcall.Arguments[0];
            //var methodcall_param_1 = methodcall.Arguments[1];
            //var unary = methodcall_param_1 as UnaryExpression;
            //var lambda = unary.Operand as LambdaExpression;
            //var param = lambda.Parameters[0] as ParameterExpression;
            //var select_methodcall = lambda.Body as NewExpression;
            //ttype = select_methodcall.Arguments[0].GetType();
            //var select_methodcall_0 = select_methodcall.Arguments[0] as MethodCallExpression;

            //var select_methodcall_1 = select_methodcall.Arguments[1] as ConstantExpression;

            //ttype = methodcall_param_0.GetType();

            //select_methodcall_1 = Expression.Constant("DisplayName");
            //select_methodcall_0 = Expression.Parameter(typeof(RegistryKey), "x");
            //select_methodcall = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), select_methodcall_0, select_methodcall_1);
            //var arg1 = Expression.Parameter(typeof(RegistryKey), "x");
            //arg1 = select_methodcall_0;

            //param = Expression.Parameter(typeof(RegistryKey), "x");
            //param = arg1;
            //lambda = Expression.Lambda(select_methodcall, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            //var tte = queryreg.ToList().AsQueryable();
            //methodcall_param_0 = Expression.Constant(tte);
            //var selects = methodcall.Method.ReflectedType.GetMethods().Where(x => x.Name == methodcall.Method.Name);
            //var select1 = selects.First().MakeGenericMethod(typeof(RegistryKey), typeof(string));
            //var methodcall1 = Expression.Call(select1, methodcall_param_0, unary);

            //var excute = tte.Provider.CreateQuery<string>(methodcall1);
            //foreach (var oo in excute)
            //{

            //}



            //var rr = queryable.Where(x => x.GetValue<string>("DisplayName") != "").Select(x=>x.GetValue<string>("DisplayName"));
            //foreach(var oo in rr)
            //{

            //}

            //var wheres = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Where" && x.GetParameters().Length == 2);
            //var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);

            //var ttype = rr.GetType();
            //MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            //var methodcall_param_0 = methodcall.Arguments[0];
            //var methodcall_param_1 = methodcall.Arguments[1];
            //var unary = methodcall_param_1 as UnaryExpression;
            //var lambda = unary.Operand as LambdaExpression;
            //var param = lambda.Parameters[0] as ParameterExpression;
            //var binary = lambda.Body as BinaryExpression;
            //var right = binary.Right as ConstantExpression;
            //var left = binary.Left as MethodCallExpression;
            //var left_args_0 = left.Arguments[0] as ParameterExpression;
            //var left_args_1 = left.Arguments[1] as ConstantExpression;

            //ttype = methodcall_param_0.GetType();

            //left_args_1 = Expression.Constant("DisplayName");
            //left_args_0 = Expression.Parameter(typeof(RegistryKey), "x");
            //left = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), left_args_0, left_args_1);
            //var arg1 = Expression.Parameter(typeof(RegistryKey), "x");
            //arg1 = left_args_0;

            //right = Expression.Constant("");
            //binary = Expression.MakeBinary(ExpressionType.NotEqual, left, right);
            //param = Expression.Parameter(typeof(RegistryKey), "x");
            //param = arg1;
            //lambda = Expression.Lambda(binary, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            //var tte = queryreg.ToList().AsQueryable();
            //methodcall_param_0 = Expression.Constant(tte);
            //var methodcall1 = Expression.Call(wheres.ElementAt(0).MakeGenericMethod(typeof(RegistryKey)), methodcall_param_0, unary);
            //var excute = tte.Provider.CreateQuery<RegistryKey>(methodcall1);
            //foreach (var oo in excute)
            //{

            //}

            //var regexs = typeof(RegistryKey).GetMethods().Where(x => x.Name == "GetValue");
            //var rr = queryable.Where(x => x.GetValue("DisplayName") != null);
            //var wheres = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Where" && x.GetParameters().Length == 2);
            //var regexs = typeof(RegistryKey).GetMethods().Where(x => x.Name == "GetValue");
            //var ttype = rr.Expression.GetType();
            //MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            //var methodcall_param_0 = methodcall.Arguments[0];
            //var methodcall_param_1 = methodcall.Arguments[1];
            //var unary = methodcall_param_1 as UnaryExpression;
            //var lambda = unary.Operand as LambdaExpression;
            //var param = lambda.Parameters[0] as ParameterExpression;
            //var binary = lambda.Body as BinaryExpression;
            //var right = binary.Right as ConstantExpression;
            //var left = binary.Left as MethodCallExpression;
            //var left_args_0 = left.Arguments[0] as ConstantExpression;
            ////var left_args_1 = left.Arguments[1] as ConstantExpression;

            //ttype = methodcall_param_0.GetType();

            ////left_args_1 = Expression.Constant("DisplayName");
            //left_args_0 = Expression.Constant("DisplayName");
            ////left = Expression.Call(regexs.ElementAt(0), left_args_0);
            //var arg1 = Expression.Parameter(typeof(RegistryKey), "x");
            //left = Expression.Call(arg1, regexs.ElementAt(0), left_args_0);

            //right = Expression.Constant(null);
            //binary = Expression.MakeBinary(ExpressionType.NotEqual, left, right);
            //param = Expression.Parameter(typeof(RegistryKey), "x");
            //param = arg1;
            //lambda = Expression.Lambda(binary, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            //var tte = queryreg.ToList().AsQueryable();
            //methodcall_param_0 = Expression.Constant(tte);
            //var methodcall1 = Expression.Call(wheres.ElementAt(0).MakeGenericMethod(typeof(RegistryKey)), methodcall_param_0, unary);
            //var excute = tte.Provider.CreateQuery(methodcall1);
            //foreach (var oo in excute)
            //{

            //}

            //var tte = queryreg.ToList().AsQueryable();
            //var rr = queryable.Select(x => x);
            //MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            //var methodcall_param_0 = methodcall.Arguments[0];
            //var methodcall_param_1 = methodcall.Arguments[1];
            //var unary = methodcall_param_1 as UnaryExpression;
            //var lambda = unary.Operand as LambdaExpression;
            //var param = lambda.Parameters[0] as ParameterExpression;
            //var body = lambda.Body as ParameterExpression;
            //var ttype = lambda.Body.GetType();

            //body = Expression.Parameter(typeof(RegistryKey),"x");
            //param = Expression.Parameter(typeof(RegistryKey),"x");
            ////param = body;
            //lambda = Expression.Lambda(body, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));
            //var selects = typeof(Queryable).GetMethods().Where(x => x.Name == "Select");
            //methodcall = Expression.Call(selects.ElementAt(0).MakeGenericMethod(typeof(RegistryKey), typeof(RegistryKey)), methodcall_param_0, unary);

            //var excute = tte.Provider.CreateQuery(methodcall);

            //Type ppf = typeof(RegQuery<Test>).GetGenericTypeDefinition();
            //Type[] typeArgs = { typeof(Test) };
            //Type d1 = typeof(RegQuery<>);
            //Type constructed = d1.MakeGenericType(typeArgs);
            //object o = Activator.CreateInstance(constructed);
            //object popo = new RegQuery<Test>();
            //var pppi = popo;

            var regt = new RegQuery<Test>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                    }).Count();




            //.Select(x => new
            //{
            //    A = x.DisplayName,
            //    B = x.EstimatedSize
            //});
            //.Where(x => x.EstimatedSize==null).Count();
            //.Where(x => x.DisplayName == "CamON");
            //.Where(x => Test(x.DisplayName, ""));
            //.FirstOrDefault(x => x.DisplayName != ""&& x.DisplayVersion !=""&& x.DisplayName !="");
            //foreach (var oo in regt)
            //{
            //    oo.DisplayName = oo.DisplayName + "AA";
            //}


            //foreach (var oo in regt)
            //{

            //}
            //var query1 = from element in new FileSystemContext(System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
            //            where element.ElementType == ElementType.File && element.Path.EndsWith(".zip")
            //            orderby element.Path ascending
            //            select element;

            //var param = Expression.Parameter(typeof(FileSystemElement), "x");
            //var property = Expression.MakeMemberAccess(param, typeof(FileSystemElement).GetProperty("ElementType"));
            //var left = Expression.MakeUnary(ExpressionType.Convert, property, typeof(int));
            //ConstantExpression right = Expression.Constant(0);
            //var binary = Expression.MakeBinary(ExpressionType.Equal, left, right);
            //var lambda = Expression.Lambda(binary, param);
            //var unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(bool));
            //var method = Expression.Call()
            //var query = new FileSystemContext(System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
            //.Where(x => x.ElementType == ElementType.File)
            //.FirstOrDefault();
            //.Where(x => x.ElementType == ElementType.File)
            //.Where(x => x.Path != "")
            //.Select(x => x);

            //int index = 0;
            //foreach (var result in query)
            //{
            //    StringBuilder s = new StringBuilder();
            //    s.AppendFormat("Result {0} '{1}'", ++index, result.ToString());
            //    System.Console.WriteLine(s.ToString());
            //}

            //var query = new FileSystemContext(System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
            //.Where(x => x.ElementType == ElementType.File && x.Path.EndsWith(".zip"))
            //.OrderBy(x => x.Path)
            //.First();

            //電腦\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}
            var ll = RegistryHive.LocalMachine.OpenView64(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}", true);
            var uus = ll.Take(x=>x!= "Properties", writable: true).Where(y=>y.GetValue<string>("AdapterModel") == "Intel(R) Dual Band Wireless-AC 7265");
            foreach(var uu in uus)
            {
                uu.SetValue("Is6GhzBandSupported", 1);
            }
            System.Diagnostics.Trace.WriteLine(ll.GetValue<string>("AdapterModel"));
            List<CTabble1> table1s = new List<CTabble1>();
            table1s.Add(new CTabble1() { Key1 = "1", Name1 = "table1_1" });
            table1s.Add(new CTabble1() { Key1 = "2", Name1 = "table1_2" });
            table1s.Add(new CTabble1() { Key1 = "3", Name1 = "table1_3" });

            List<CTable2> table2s = new List<CTable2>();
            table2s.Add(new CTable2() { Key2 = "1", Name2 = "table2_1" });
            table2s.Add(new CTable2() { Key2 = "1", Name2 = "table2_1" });
            table2s.Add(new CTable2() { Key2 = "2", Name2 = "table2_2" });
            table2s.Add(new CTable2() { Key2 = "3", Name2 = "table2_3" });

            var table = table1s.Join(table2s, x => x.Key1, y => y.Key2, (x, y) => new { x, y });
            var regs = RegistryHive.LocalMachine.OpenView(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach(var reg in regs)
            {

            }
            //RegistryKey reg_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            //RegistryKey reg_64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey reg_32 = RegistryHive.LocalMachine.OpenView32(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey reg_64 = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

            //RegistryKey win_info = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            //string ReleaseId =  win_info.GetValue<string>("ReleaseId");
            RegistryKey win_info = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\Microsoft\");
            string ReleaseId = win_info.GetValue<string>(@"Windows NT\CurrentVersion","ReleaseId");
            //SOFTWARE\Microsoft\Windows NT\CurrentVersion
            RegistryKey uninstall = reg_64.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            //foreach (var oo in uninstall.GetSubKeyNames())
            //{
            //    RegistryKey subkey = uninstall.OpenSubKey(oo);
            //    object obj = subkey.GetValue("DisplayName");
            //    string displayname = subkey.GetValue("DisplayName") as string;
            //    System.Diagnostics.Trace.WriteLine(displayname);
            //    subkey.Dispose();
            //}

            List<AppData> apps = new List<AppData>();
            apps.Join(apps, x => x.Name, y => y.Name, (x,y)=> new {x, y }, StringComparer.OrdinalIgnoreCase);
            apps.Add(new AppData() { Name = "WinFlash" });
            apps.Add(new AppData() { Name = "WinFlash" });
            apps.Add(new AppData() { Name = "Dropbox 25 GB" });
            apps.Add(new AppData() { Name = "AnyDes", IsOfficial = true });
            //var apptest = apps.Join(apps, func=> { return true; }, (x, y) => new { x, y });
            //apptest.ToList();
            foreach (var app in apps)
            {
                var reg1 = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == app.Name);
                if (reg1 != null)
                {
                    //app.Ver = reg1.GetValue<string>("DisplayVersion");
                    //app.Uninstallstring = reg1.GetValue<string>("UninstallString");
                }
            }
            Func<AppData, RegistryKey, AppData> f = ((a, b) =>
            {
                a.Name = b.GetValue<string>("DisplayName");
                a.Uninstallstring = b.GetValue<string>("UninstallString");
                a.Ver = b.GetValue<string>("DisplayVersion");
                return a;
            });
            //var jj = uninstall.Where(x => x.GetValue<string>("DisplayName") != "").Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => new { x, y }).Select(x => f(x.y, x.x));
            //var jj = uninstall.Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => new { x, y });
            var jj = uninstall.Join(apps, reg => reg.GetValue<string>("DisplayName"), app => app.Name, (reg, app) => new { reg, app })
                .Select(so =>
                {
                    so.app.Uninstallstring = so.reg.GetValue<string>("UninstallString");
                    so.app.Ver = so.reg.GetValue<string>("DisplayVersion");
                    return so.app;
                });
            var existapps = uninstall.Join(apps, (reg, app) =>
            {
                bool hr = false;
                string dispay = reg.GetValue<string>("DisplayName");
                string uninstall_ = reg.GetValue<string>("Uninstall");
                string version = reg.GetValue<string>("Version");
                if (app.IsOfficial == true)
                {
                    hr = string.IsNullOrEmpty(dispay) == false && app.Name.Contains(dispay);
                }
                else
                {
                    hr = app.Name == dispay;
                }

                return hr;
            }, (reg, app) => new { reg, app })
                .Select(so =>
                {
                    so.app.Uninstallstring = so.reg.GetValue<string>("UninstallString");
                    so.app.Ver = so.reg.GetValue<string>("DisplayVersion");
                    return so.app;
                });
            int runcount = 50;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            for(int i=0; i< runcount; i++)
            {
                //foreach (var oo in uninstall.GetSubKeyNames())
                //{
                //    RegistryKey subkey = uninstall.OpenSubKey(oo);
                //    object obj = subkey.GetValue("DisplayName");
                //    //object obj1 = subkey.GetValue("UninstallString");
                //    //object obj2 = subkey.GetValue("DisplayVersion");
                //    //string displayname = subkey.GetValue("DisplayName") as string;
                //    //System.Diagnostics.Trace.WriteLine(displayname);
                //    subkey.Dispose();
                //}


                existapps.ToList();
            }
            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"{sw.ElapsedMilliseconds/ runcount}");
            //foreach (var oo in existapps)
            //{

            //}

            //var nonexist = apps.Except(existapps);
            var jjj = uninstall.Select(x => new { DisplayName = x.GetValue<string>("DisplayName"), DisplayVersion = x.GetValue<string>("DisplayVersion") });
            foreach (var oo in jjj)
            {
                //oo.DisplayName;
            }


            var first = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics" || x.GetValue<string>("DisplayName") == "");
            var last = uninstall.LastOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var count = uninstall.Count();
            var count_1 = uninstall.Count(x => string.IsNullOrEmpty(x.GetValue<string>("DisplayName")) == false);
            var select = uninstall.Select(x => x.GetValue<string>("DisplayName"));
            var where = uninstall.Where(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var list = uninstall.ToList();
            var dic = uninstall.ToDictionary(x => x.Name);
            var groups = uninstall.GroupBy(x => x.GetValue<string>("DisplayName"));
            foreach (var item in groups)
            {
                System.Diagnostics.Trace.WriteLine($"DisplayName:{item.Key} count:{item.Count()}");
                foreach (var oo in item)
                {
                    //System.Diagnostics.Trace.WriteLine($"DisplayName:{oo.GetValue<string>("DisplayName")}");
                }
            }
            var lookups = uninstall.ToLookup(x => x.GetValue<string>("DisplayName"));
            foreach (var item in lookups)
            {
                System.Diagnostics.Trace.WriteLine($"DisplayName:{item.Key} count:{item.Count()}");
                foreach (var oo in item)
                {
                    //System.Diagnostics.Trace.WriteLine($"DisplayName:{oo.GetValue<string>("DisplayName")}");
                }
            }
            var takes = uninstall.Take(3);



        }

        static bool Test(string left, string right)
        {
            return left == right;
        }

    }

    public class AppDataComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(string obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            return obj.GetHashCode();

        }
    }

    public class AppData
    {
        public string Name { set; get; }
        public string Ver { set; get; }
        public string Uninstallstring { set; get; }
        public bool IsOfficial { set; get; }
    }
}
