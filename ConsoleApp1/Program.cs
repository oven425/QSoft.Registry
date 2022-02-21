#define Queryable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
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
        
        public InstalledApp() { }

        public InstalledApp(InstalledApp data)
        {
            var pps = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach (var pp in pps)
            {
                pp.SetValue(this, pp.GetValue(data));
            }
        }
    }

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }

class Program
    {
        
        static object dyy = new { A = 1 };
        static void Main(string[] args)
        {
            //List<KeyValuePair<string, Type>> typekeys = new List<KeyValuePair<string, Type>>();
            //typekeys.Add(new KeyValuePair<string, Type>("Age", typeof(int)));
            //typekeys.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            //var oiop = LatticeUtils.AnonymousTypeUtils.CreateType(typekeys);

            //List<Tuple<Type, string>> ttypees = new List<Tuple<Type, string>>();
            //ttypees.Add(Tuple.Create(typeof(int), "Age"));
            //ttypees.Add(Tuple.Create(typeof(string), "Name"));
            //var ppo = ttypees.BuildType(null);
            //oiop = null;
            //List<string> testa = new List<string>() { "A", "B", "C" };
            ////testa.Clear();
            ////var oiou = testa.DefaultIfEmpty();
            //List<string> testb = new List<string>() { "a", "b","c","d","e" };
            //testb.Clear();
            //foreach (var a in testa)
            //{
            //    foreach (var b in testb)
            //    {
            //        System.Diagnostics.Trace.WriteLine($"{a}{b}");
            //    }
            //}
            //System.Diagnostics.Trace.WriteLine("---");
            //var testmany = testa.SelectMany(x => testb.DefaultIfEmpty(), (a, b) => new {a,b });
            //foreach (var oo in testmany)
            //{
            //    System.Diagnostics.Trace.WriteLine(oo);
            //}



            ////List<int> testlist = Enumerable.Range(1, 5).ToList();
            ////var qur = testlist.AsQueryable();
            ////foreach(var oo in testlist)
            ////{
            ////    if(oo==2)
            ////    {
            ////        testlist[0] = 100;
            ////    }
            ////}




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

#if Queryable



            var qi = Enumerable.Range(1, 10).AsQueryable();
            var qi_where = qi.Where(x => x > 10).Select(x => x);
            var qi_groupby = qi.GroupBy(x => x > 5);
            var regt = new RegQuery<InstalledApp>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                        x.View = RegistryView.Registry64;
                    });
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
