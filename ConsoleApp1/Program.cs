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
            if (object.ReferenceEquals(obj, null)) return 0;
            return obj.DisplayName == null ? 0 : obj.DisplayName.GetHashCode();
        }
    }
    public class InstalledApp
    {
        public string DisplayName { set; get; }
        public Version DisplayVersion { set; get; }
        public int? EstimatedSize { set; get; }
        public bool? IsOfficial { set; get; }
        public InstalledApp()
        {
            //DisplayName = "AA";
        }

        public int FC()
        {
            return 100;
        }
        public override string ToString()
        {
            return $"DisplayName:{DisplayName} DisplayVersion:{DisplayVersion} EstimatedSize:{EstimatedSize} IsOfficial:{IsOfficial}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var zip_method = typeof(Queryable).GetMember("Zip").First();
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

            List<InstalledApp> installs = new List<InstalledApp>();
            installs.Add(new InstalledApp() { DisplayName = "A", IsOfficial = true });
            installs.Add(new InstalledApp() { DisplayName = "AA", IsOfficial = false });
#if Queryable
            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);
            List<RegistryKey> ll = new List<RegistryKey>();
            foreach (var subkeyname in queryreg.GetSubKeyNames())
            {
                RegistryKey reg = queryreg.OpenSubKey(subkeyname, false);
                ll.Add(reg);
            }
            var queryable = ll.AsQueryable();
            //var rr = provider.Refresh().GroupBy(x => x.GetValue<string>("DisplayName"), y => new Test()
            //{
            //    DisplayName = y.GetValue<string>("DisplayName"),
            //    DisplayVersion = y.GetValue<string>("DisplayVersion"),
            //    EstimatedSize = y.GetValue<int>("EstimatedSize")
            //});
            //var rr = queryable.GroupBy(x=>new { DisplayName=x.GetValue<string>("DisplayName"), EstimatedSize = x.GetValue<int>("EstimatedSize") });
            //var rr = queryable.Join(apps, x => x.GetValue<string>("DisplayName"), y => y.Name, (x, y) => y);
            //var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), x => new { DisplayName =x.GetValue<string>("DisplayName"), DisplayVersion = x.GetValue<string>("DisplayVersion") });

            List<int> src1 = new List<int> { 1, 2, 3 };
            List<int> src2 = new List<int>() { 1,10 };
            var except1 = src1.Except(src2);
            var rr = queryable.Select(x => new DateTime(2021,10,10));
            //var rr = queryable.Select(x => new InstalledApp()
            //{
            //    DisplayName = x.GetValue<string>("DisplayName"),
            //    DisplayVersion = x.GetValue<Version>("DisplayVersion"),
            //    EstimatedSize = x.GetValue<int>("EstimatedSize"),
            //    IsOfficial = x.GetValue<bool>("IsOfficial")
            //}).Except(installs.Take(2), new InstallAppCompare());
            foreach (var ppp in rr)
            {

            }

            //var groupp = queryable.Join(apps, x => x.GetValue<string>(""), x => x.Name, (x, y) => new { a=x.GetValue<int>(""), b=y.IsOfficial});
            //foreach(var gr in groupp)
            //{

            //}
            //var groupbys = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "GroupBy");

            //var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);

            var ttype = rr.GetType();
            MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            var unary = methodcall.Arguments[1] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;
            
            ttype = lambda.Body.GetType();
            var newexpr = lambda.Body as NewExpression;
            //var binary = newexpr.Arguments[0] as BinaryExpression;
            //ttype = newexpr.Arguments[0].GetType();
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


            //var take1 = regt.Take(1).Select(x => new InstalledApp()).Where(x => x.DisplayName != "");
            //foreach (var oo in take1)
            //{

            //}
            //var take2 = regt.Take(2);
            //foreach (var oo in take2)
            //{

            //}
            //var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");
            //foreach (var oo in takewhile)
            //{

            //}
            //var update_where = regt.Where(x => x.DisplayName == "123456" + "789");
            //var update = update_where.Update(x => new InstalledApp() { DisplayName = "123456789", EstimatedSize = 100 });
            //var orderbydesc = regt.Where(x => x.DisplayName != "123456789").OrderByDescending(x => x.EstimatedSize).Count();
            //var oderby = regt.OrderBy(x => x.EstimatedSize);

            //var where = regt.Where(x => x.DisplayName != "").OrderByDescending(x => x.EstimatedSize);
            //var where = regt.OrderBy(x=>x.EstimatedSize).Where((x, index) => index%2==0);
            //var where = regt.Where((x, index) => x.DisplayName == "AA");
            //var where = regt.Where((x, index) => index % 2 == 0);
            //var where = regt.Where((x, index) => x.DisplayName == "AA" && index % 2 == 0);
            //var where = regt.Where(x => x.DisplayVersion.ToString() != "" && string.IsNullOrEmpty(x.DisplayName)==true)
            //    . Select(x=> new InstalledApp()).OrderBy(x=>x.EstimatedSize);
            //var where = regt.Where(x => string.IsNullOrEmpty(x.DisplayName));
            //var where = regt.Where((x, index) => x.DisplayName == $"{x.DisplayName}");
            //var where = regt.Where(x => x.DisplayVersion.ToString() == "1.1.1.1".ToString());
            //var where = regt.Where(x => x.DisplayName.Contains("AA"));
            var where = regt.Where(x => x.EstimatedSize == 100);
            foreach (var oo in where)
            {

            }
            //Distinct()、Except()、Intersect

            var tests = installs.Take(2);
            //var except = regt.Except(tests, new InstallAppCompare());
            //foreach (var oo in except)
            //{

            //}

            //var discinct = regt.Update(x=> $"{x.DisplayName}{x.DisplayName}");
            //foreach(var oo in discinct)
            //{

            //}

            //where1 = regt.Where(x => x.DisplayName == "A").OrderBy(x=>x.DisplayVersion);
            //foreach (var oo in where1)
            //{

            //}
            ////.Where(x => string.IsNullOrWhiteSpace(x.DisplayVersion));
            ////.Where(x => string.IsNullOrWhiteSpace(x.DisplayVersion) == true);
            ////.Where(x => x.DisplayName.Contains("A"));
            ////var where = regt.Where(x => x.DisplayName != "").Select(x => x);
            ////var where = regt.Where((x, index) => x.DisplayName != "");
            //var where = regt.Where(x => x.DisplayName.Contains("AA") == true);
            //foreach (var oo in where)
            //{

            //}
            //foreach (var oo in where)
            //{

            //}
            //no support
            //.Where(x => x.A() != "");


            //var groupby = regt.GroupBy(x => x);
            //var groupby = regt.GroupBy(x => x).Select(x => x.Key);
            //var groupby = regt.GroupBy(x => x.DisplayName);
            //var groupby = regt.GroupBy(x => x.EstimatedSize);
            //var groupby = regt.GroupBy(x => x.DisplayVersion);
            //var groupby = regt.GroupBy(x => new { x.DisplayName, x.DisplayVersion });
            //var groupby = regt.GroupBy(x => x).Select(x => x);
            //var groupby = regt.GroupBy(x => x).Select(x => x);
            //var groupby = regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize);
            //////.GroupBy(x => new { x.DisplayName, x.DisplayVersion });
            //////.GroupBy(x => new { x.DisplayName, x.DisplayVersion }, x => x.DisplayName);
            //foreach (var oo in groupby)
            //{

            //}
            //.Where(x => x.DisplayName != "").OrderBy(x => x.EstimatedSize).GroupBy(x => x.DisplayVersion, x => x.EstimatedSize);
            //var join = regt.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => x);
            //var join = regt.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => y);
            //var join3 = regt.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => new AppData() { Name = x.DisplayName });
            //var join = regt.Join(installs, x => x.DisplayName, y => y.DisplayName, (x, y) => y);
            //foreach (var oo in join)
            //{

            //}
            //.Join(apps, x => x.DisplayName, y => y.Name, (x, y) => new AppData { Name=x.DisplayName, IsOfficial= y.IsOfficial });
            //var groupjoin = regt.GroupJoin(apps, x => x.DisplayName, y => y.Name, (x, y) => x);
            //foreach(var oo in groupjoin)
            //{

            //}

            //var element = regt.ElementAt(0);
            //var elementatordefault = regt.ElementAtOrDefault(100);

            //var select = regt.Select(x => x);
            //.Select(x => x.EstimatedSize);
            //var select = regt.Select(x => new { x.DisplayName, x.DisplayVersion });
            //var select = regt.Select(x => new AppData() { Name = x.DisplayName });
            //var select = regt.Select(x => new AppData(x.DisplayName));
            //var select = regt.Select(x => x).Select(x => new { x.DisplayName }).Select(x => x.DisplayName);
            //var select = regt.Select(x => x);
            //var select = regt.Select(x=>x).Select(x => new { x.DisplayName}).Select(x=>new InstalledApp() { }).Where(x=>x.DisplayName=="");
            //var select = regt.Select((x, index) =>new { x, index });
            //foreach (var oo in select)
            //{

            //}

            //var fff = regt.All(x=>x.DisplayName == "");
            //var zip = regt.Zip(installs, (reg, app) => new { reg, app });
            //var zip = regt.Zip(installs, (reg, app) => app);
            //var zip = regt.Zip(installs, (reg, app) => reg);
            //var zip = regt.Zip(installs, (reg, app) => app.DisplayName);
            //var zip = regt.Zip(installs, (reg, app) => reg.DisplayName);
            //var zip = regt.Zip(installs, (reg, app) => new { reg.DisplayName, Name=app.DisplayName });
            //var zip = regt.Zip(apps, (reg, app) => app);
            //var zip = regt.Zip(apps, (reg, app) => reg.DisplayName);
            //var zip = apps.Zip(regt, (app, reg) => reg);
            //var zip = installs.Zip(regt, (app, reg) => reg);
            //var zip = regt.Zip(apps, (reg, app) => new { app.Name, reg.DisplayName });
            //foreach (var oo in zip)
            //{

            //}

            //apps.Zip(regt, (app, reg) => reg);


            //var update1 = regt.Where(x => x.DisplayName == "AA").Where(x => x.EstimatedSize == 10)
            //    .Update(x => new InstalledApp()
            //    {
            //        EstimatedSize = 100,
            //        DisplayVersion = new Version("123.123.123.123")
            //    });

            //var update2 = regt.Where(x => x.DisplayName == "AA").Where(x => x.EstimatedSize == 10)
            //    .Update(x => new InstalledApp()
            //    {
            //        EstimatedSize = x.EstimatedSize + 100,
            //        DisplayVersion = new Version("123.123.123.123")
            //    });

            //var update3 = regt.Update(x => new InstalledApp()
            //{
            //    EstimatedSize = x.EstimatedSize + 100,
            //    DisplayVersion = new Version("123.123.123.123"),
            //    DisplayName = x.DisplayVersion.ToString()
            //});
            //var min1 = regt.Select(x => x.DisplayName.Length).Min();
            //var min2 = regt.Select(x => x.EstimatedSize).Min();
            //var first1 = regt.First();
            //var first2 = regt.First(x => x.DisplayName == "BB");
            //var first3 = regt.Select(x => x).First(x => x.DisplayName == "AA");
            //var first4 = regt.Select(x => x).First();
            //var first2 = regt.First(x => x.DisplayName != "");
            //var firstordefault1 = regt.FirstOrDefault();
            //var firstordefault2 = regt.FirstOrDefault(x => x.DisplayName == "BB");
            //var firstordefault3 = regt.FirstOrDefault(x => x.DisplayName.Contains("BB"));
            //var firstordefault4 = regt.FirstOrDefault(x => x.DisplayName.Contains("BB")==true);
            //var firstordefault5 = regt.FirstOrDefault(x => x.DisplayName.Contains("BB") == false);
            //var last1 = regt.Last();
            //var last2 = regt.Last(x => x.DisplayName != "");



            //var count1 = regt.Where(x => x.DisplayName == "123456789").OrderBy(x=>x.EstimatedSize).Count();
            //var count2 = regt.Count(x => x.DisplayName == "AA");

            //var all = regt.All(x => x.DisplayName != "");
            //var any = regt.Any(x => x.EstimatedSize > 0);
            //var reverse = regt.Reverse();
            //var average = regt.Average(x => x.EstimatedSize);
            //var sum = regt.Sum(x => x.EstimatedSize);
            //var skip1 = regt.Skip(1);
            //var skipwhile = regt.SkipWhile(x => x.DisplayName == "B");
            //var min = regt.Min(x => x.EstimatedSize);
            var min = regt.Select(x => x.DisplayName.Length).Min();
            var max = regt.Max(x => x.EstimatedSize);
            //var max = regt.Max(x => x.DisplayName.Length);
            //var vers = regt.Select(x => x.DisplayVersion.ToString());
            var vers = regt.Select((x, index) => x);
            foreach(var oo in vers)
            {

            }
            //var max = regt.Max(x => x.DisplayVersion.ToString().Length);
            //var loopup = regt.ToLookup(x => x.DisplayName);
            //var tolist = regt.ToList();
            //var toarray = regt.ToArray();
            //var dictonary = regt.ToDictionary(x => x.EstimatedSize);
            //var single = regt.Single(x => x.DisplayName == "A");
            //var singledefault = regt.SingleOrDefault(x => x.DisplayName == "A");

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
