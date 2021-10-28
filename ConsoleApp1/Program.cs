#define Queryable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;

namespace ConsoleApp1
{
    public class InstalledApp
    {
        public string DisplayName { set; get; }
        public string DisplayVersion { set; get; }
        public int? EstimatedSize { set; get; }
        public InstalledApp()
        {
            DisplayName = "AA";
        }
        public string A()
        {
            return this.DisplayName;
        }
    }

    public class Provider
    {
        public IQueryable<RegistryKey> Refresh()
        {
            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);
            return queryreg.ToList().AsQueryable();
        }
    }


    class Program
    {
        static IQueryable<RegistryKey> Refresh()
        {
            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);
            return queryreg.ToList().AsQueryable();
        }
        static void Main(string[] args)
        {
            List<AppData> apps = new List<AppData>();
            apps.Add(new AppData() { Name = "A", IsOfficial = true });
            apps.Add(new AppData() { Name = "AA", IsOfficial = false });
            //apps.Add(new App() { Name = "B", Offical = true });
            //apps.Add(new App() { Name = "BB", Offical = false });
            //apps.Add(new App() { Name = "C", Offical = true });
            //apps.Add(new App() { Name = "CC", Offical = false });
            //apps.Add(new App() { Name = "D", Offical = true });
            //apps.Add(new App() { Name = "DD", Offical = false });

#if Queryable
        var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);

            Provider provider = new Provider();
            var queryable = queryreg.ToList().AsQueryable();
            //var rr = provider.Refresh().GroupBy(x => x.GetValue<string>("DisplayName"), y => new Test()
            //{
            //    DisplayName = y.GetValue<string>("DisplayName"),
            //    DisplayVersion = y.GetValue<string>("DisplayVersion"),
            //    EstimatedSize = y.GetValue<int>("EstimatedSize")
            //});
            //var rr = queryable.GroupBy(x=>new { DisplayName=x.GetValue<string>("DisplayName"), EstimatedSize = x.GetValue<int>("EstimatedSize") });
            //var rr = queryable.Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => y);
            //var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), x => new { DisplayName =x.GetValue<string>("DisplayName"), DisplayVersion = x.GetValue<string>("DisplayVersion") });

            var rr = queryable.Select((x,index) => new {x=new InstalledApp() {DisplayName = x.GetValue<string>("DisplayName") }, index});

            //var groupbys = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "GroupBy");

            //var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);

            var ttype = rr.GetType();
            MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            var pppps = methodcall.Method.GetParameters();
            var gens = methodcall.Method.GetGenericArguments();
            var iuiu = methodcall.Method.GetGenericArguments();
            var unary = methodcall.Arguments[1] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;

            //ttype = lambda.Body.GetType();
            //var memberinit = lambda.Body as MemberInitExpression;
            //foreach (var binding in memberinit.Bindings)
            //{
            //    var assign = binding as MemberAssignment;
            //    var method = assign.Expression as MethodCallExpression;
            //    var unary1 = assign.Expression as UnaryExpression;
            //    if (unary1 != null)
            //    {
            //        ttype = unary1.Operand.GetType();
            //        var mme = unary1.Operand as MethodCallExpression;

            //    }


            //}
            //var newexpr = memberinit.NewExpression;


            //var pps = typeof(Test).GetProperties();
            //var ccs = typeof(Test).GetConstructors();
            //var param = Expression.Parameter(typeof(RegistryKey), "y");
            //List<MemberAssignment> bindings = new List<MemberAssignment>();
            //foreach (var pp in pps)
            //{
            //    Expression name = null;
            //    if (pp.PropertyType.Name.Contains("Nullable"))
            //    {

            //        name = Expression.Constant(pp.Name, typeof(string));
            //        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
            //        UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
            //        var binding = Expression.Bind(pp, unary1);
            //        bindings.Add(binding);
            //    }
            //    else
            //    {
            //        name = Expression.Constant(pp.Name, typeof(string));
            //        var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), param, name);
            //        var binding = Expression.Bind(pp, method);
            //        bindings.Add(binding);
            //    }
            //}

            //memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);
            //lambda = Expression.Lambda(memberinit, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            //var methodcall_param_0 = Expression.Constant(queryable);
            //methodcall = Expression.Call(methodcall.Method, methodcall_param_0, unary);
            //var gener = methodcall.Method.GetGenericArguments();
            //var ssl = queryable.Provider.CreateQuery<Test>(methodcall);
            //foreach(var oo in ssl)
            //{

            //}

            //ttype = methodcall.Arguments[2].GetType();


            //ttype = methodcall_param_0.GetType();

            //body_param_1 = Expression.Constant("DisplayName");
            //body_param_0 = Expression.Parameter(typeof(RegistryKey), "x");
            //body = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), body_param_0, body_param_1);
            //param = Expression.Parameter(typeof(RegistryKey), "x");
            //param = body_param_0;
            //lambda = Expression.Lambda(body, param);
            //unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            //var met = groupbys.FirstOrDefault().MakeGenericMethod(typeof(RegistryKey), typeof(string));
            //var vvvvv = methodcall.Method.GetGenericArguments();
            //methodcall = Expression.Call(met, Expression.Constant(queryable), unary);
            //var excute = queryable.Provider.CreateQuery(methodcall);


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


            var regt = new RegQuery<InstalledApp>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                        x.View = RegistryView.Registry64;
                    });
            var orderbydesc = regt.OrderByDescending(x => x.EstimatedSize);
            var oderby = regt.OrderBy(x => x.EstimatedSize);

            //.Where(x => x.DisplayName == "A");
            //.Where(x => string.IsNullOrWhiteSpace(x.DisplayVersion));
            //.Where(x => string.IsNullOrWhiteSpace(x.DisplayVersion) == true);
            //.Where(x => x.DisplayName.Contains("A"));
            var where = regt.Where(x => x.DisplayName != "").Select(x => x);

            //no support
            //.Where(x => x.A() != "");



            //.GroupBy(x => x.DisplayName);
            //.GroupBy(x => x.DisplayName, x => x.EstimatedSize);
            //.GroupBy(x => x.EstimatedSize);
            //.GroupBy(x => new { x.DisplayName, x.DisplayVersion });
            //.GroupBy(x => new { x.DisplayName, x.DisplayVersion }, x => x.DisplayName);
            //.Where(x => x.DisplayName != "").OrderBy(x => x.EstimatedSize).GroupBy(x => x.DisplayVersion, x => x.EstimatedSize);
            //.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => new { x.DisplayName, x.EstimatedSize, y.IsOfficial });
            //.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => new AppData { Name=x.DisplayName, IsOfficial= y.IsOfficial });
            //.GroupJoin(apps, x => x.DisplayName, y => y.Name, (x, y) => x);

            var select = regt.Select(x => x);
            //.Select(x => x.EstimatedSize);
            //.Select(x => new { x.DisplayName, x.DisplayVersion });
            //var select = regt.Select(x => new AppData() { Name = x.DisplayName });
            //.Select(x => new AppData(x.DisplayName));
            //.Select(x => new AppData(x.DisplayName) { Ver=x.DisplayVersion });
            //.Select(x => new { x.DisplayName});
            //var select = regt.Select((x, index) =>new { x, index });
            foreach(var oo in select)
            {
                
            }
            ////var zip = regt.Zip(apps, (reg, app) => new { reg.DisplayName, app.Name });
            ////foreach (var oo in zip)
            ////{

            ////}



            //var regt = new RegQuery<InstalledApp>()
            //    .useSetting(x =>
            //    {
            //        x.Hive = RegistryHive.LocalMachine;
            //        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
            //    });
            //var first1 = regt.First();
            //var first2 = regt.First(x => x.DisplayName != "");
            //var last1 = regt.Last();
            //var last2 = regt.Last(x => x.DisplayName != "");

            //var take = regt.Take(10);
            //var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");

            //var count1 = regt.Count();
            //var count2 = regt.Count(x => x.DisplayName == "AA");

            //var all = regt.All(x => x.DisplayName != "");
            //var any = regt.Any(x => x.EstimatedSize > 0);
            //var reverse = regt.Reverse();
            //var average = regt.Average(x => x.EstimatedSize);
            //var sum = regt.Sum(x => x.EstimatedSize);
            //var skip1 = regt.Skip(1);
            //var skipwhile = regt.SkipWhile(x => x.DisplayName == "B");
            //var min = regt.Min(x => x.EstimatedSize);
            //var max = regt.Max(x => x.EstimatedSize);

            //var loopup = regt.ToLookup(x => x.DisplayName);
            //var tolist = regt.ToList();
            //var toarray = regt.ToArray();
            //var dictonary = regt.ToDictionary(x => x.EstimatedSize);
            //var single = regt.Single(x => x.DisplayName == "A");
            //var singledefault = regt.SingleOrDefault(x => x.DisplayName == "A");
            List<InstalledApp> tests = new List<InstalledApp>();
            for(int i=0; i<3; i++)
            {
                tests.Add(new ConsoleApp1.InstalledApp() { EstimatedSize = i, DisplayName = i.ToString() });
            }
#else

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

#endif

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
}
