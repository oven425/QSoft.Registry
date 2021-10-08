﻿#define Queryable
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

        static public bool  Set(RegistryKey reg)
        {
            reg.SetValue("a", "a");
            //reg.SetValue("b", "b");
            return true;
        }

        static public void Set(int reg)
        {
            //reg.SetValue("a", "a");
            //reg.SetValue("b", "b");
            //return true;
        }

        static void Main(string[] args)
        {

#if Queryable
            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);


            var queryable = queryreg.ToList().AsQueryable();
            //var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), y => new Test()
            //{
            //    DisplayName = y.GetValue<string>("DisplayName"),
            //    DisplayVersion = y.GetValue<string>("DisplayVersion"),
            //    EstimatedSize = y.GetValue<int>("EstimatedSize")
            //});
            var rr = queryable.Select(y => new Test()
            {
                DisplayName = y.GetValue<string>("DisplayName"),
                DisplayVersion = y.GetValue<string>("DisplayVersion"),
                EstimatedSize = y.GetValue<int>("EstimatedSize")
            });


            //var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), x => new { DisplayName =x.GetValue<string>("DisplayName"), DisplayVersion = x.GetValue<string>("DisplayVersion") });
            //var groupbys = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "GroupBy");

            var regexs = typeof(RegistryKeyEx).GetMethods().Where(x => "GetValue" == x.Name);

            var ttype = rr.GetType();
            MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            var unary = methodcall.Arguments[1] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;
            var memberinit = lambda.Body as MemberInitExpression;
            foreach (var binding in memberinit.Bindings)
            {
                var assign = binding as MemberAssignment;
                var method = assign.Expression as MethodCallExpression;
                var unary1 = assign.Expression as UnaryExpression;
                if(unary1 != null)
                {
                    ttype = unary1.Operand.GetType();
                    var mme = unary1.Operand as MethodCallExpression;

                }


            }
            var newexpr = memberinit.NewExpression;


            var pps = typeof(Test).GetProperties();
            var ccs = typeof(Test).GetConstructors();
            var param = Expression.Parameter(typeof(RegistryKey), "y");
            List<MemberAssignment> bindings = new List<MemberAssignment>();
            foreach (var pp in pps)
            {
                Expression name = null;
                if (pp.PropertyType.Name.Contains("Nullable"))
                {

                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(pp.PropertyType), param, name);
                    UnaryExpression unary1 = Expression.Convert(method, pp.PropertyType);
                    var binding = Expression.Bind(pp, unary1);
                    bindings.Add(binding);
                }
                else
                {
                    name = Expression.Constant(pp.Name, typeof(string));
                    var method = Expression.Call(regexs.ElementAt(0).MakeGenericMethod(typeof(string)), param, name);
                    var binding = Expression.Bind(pp, method);
                    bindings.Add(binding);
                }
            }

            memberinit = Expression.MemberInit(Expression.New(ccs[0]), bindings);
            lambda = Expression.Lambda(memberinit, param);
            unary = Expression.MakeUnary(ExpressionType.Quote, lambda, typeof(RegistryKey));

            var methodcall_param_0 = Expression.Constant(queryable);
            methodcall = Expression.Call(methodcall.Method, methodcall_param_0, unary);
            var gener = methodcall.Method.GetGenericArguments();
            var ssl = queryable.Provider.CreateQuery<Test>(methodcall);
            foreach(var oo in ssl)
            {

            }

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


            var regt = new RegQuery<Test>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                    })

            //.OrderBy(x => x.DisplayName);
            //.Single(x => x.DisplayName == "A");
            //.Where(x => x.DisplayName != "");
            .Select(x => x);
            //.Select(x => x.DisplayName);
            //.Select(x => new { x.DisplayName, x.DisplayVersion });
            //.GroupBy(x => x.DisplayName);
            //.GroupBy(x => new { x.DisplayName, x.DisplayVersion });

            foreach (var oo in regt)
            {

            }



            //var regt = new RegQuery<Test>()
            //    .useSetting(x =>
            //    {
            //        x.Hive = RegistryHive.LocalMachine;
            //        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
            //    });
            //var first1 = regt.First();
            //var first2 = regt.First(x => x.DisplayName != "");
            //var last1 = regt.Last();
            //var last2 = regt.Last(x => x.DisplayName != "");
            //var loopup = regt.ToLookup(x => x.DisplayName);
            //var take = regt.Take(10);
            //var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");
            //var tolist = regt.ToList();
            //var toarray = regt.ToArray();
            //var dictonary = regt.ToDictionary(x => x.DisplayName);
            //take = regt.Reverse().Take(4);
            //var count1 = regt.Count();
            //var count2 = regt.Count(x => x.DisplayName == "AA");



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
        public string Name { set; get; }
        public string Ver { set; get; }
        public string Uninstallstring { set; get; }
        public bool IsOfficial { set; get; }
    }
}
