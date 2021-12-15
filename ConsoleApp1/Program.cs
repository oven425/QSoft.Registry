#define Queryable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

    public class App
    {
        [RegPropertyName(Name = "DisplayName")]
        public string Name { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public string Version { set; get; }
        [RegIgnore]
        public int Size { set; get; }
    }

    public class InstalledApp
    {
        [RegSubKeyName]
        public string Key { set; get; }
        public string DisplayName { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public Version Version { set; get; }
        public int? EstimatedSize { set; get; }
        [RegIgnore]
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
            return $"DisplayName:{DisplayName} DisplayVersion:{Version} EstimatedSize:{EstimatedSize} IsOfficial:{IsOfficial}";
        }
    }

    class Program
    {
        public static T test<T>(object src)
        {
            return (T)Convert.ChangeType(src, typeof(T));
        }
        static void Main(string[] args)
        {
            var tt1 = test<int>(5);
            var tt2 = test<uint>(5);
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

            
            string full = @"HKEY_LOCAL_MACHINE\";
            //Regex regex1 = new Regex(@"^(?<base>\w+)[\\](\.+)[\\]$(?<path>.+)", RegexOptions.Compiled);
            //Regex regex1 = new Regex(@"^(.+)(?<=\\)(?<path>.*)", RegexOptions.Compiled);
            //var match = regex1.Match(full);
            //if(match.Success)
            //{

            //}
            //var sss = full.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            //var ss1 = sss.Skip(1).Take(sss.Length - 2);

            //var ssssss = ss1.Aggregate((x, y) => $"{x}\\{y}");


            //RegistryKey regbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //var reg1 = regbase.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);
            //var parent = regbase.GetParent();
            //Regex regex = new Regex(@"[\\]([A-Za-z0-9]+)$");
            //Regex regex = new Regex(@"^(?<base>\w+)[\\]");


            var queryreg = RegistryHive.LocalMachine.OpenView64(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A", false);


            List<RegistryKey> ll = new List<RegistryKey>();
            foreach (var subkeyname in queryreg.GetSubKeyNames())
            {
                RegistryKey reg = queryreg.OpenSubKey(subkeyname, false);
                ll.Add(reg);
            }
            var queryable = ll.AsQueryable();
            //var o_pp = Expression.Parameter(typeof(RegistryKey), "x");
            //var o1 = typeof(InstalledApp).ToData(o_pp);
            //var o_func = Expression.Lambda<Func<RegistryKey, InstalledApp>>(o1, o_pp).Compile();
            //var o2 = o_func(queryable.First());


            


            var rr = queryable.GroupBy(x => x.GetValue<string>("DisplayName"), (x, y) => new { x, y = y.Select(xuu => new InstalledApp() { }) });

            var ttype = rr.GetType();
            MethodCallExpression methodcall = rr.Expression as MethodCallExpression;
            var unary = methodcall.Arguments[2] as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;


            var newexpr = lambda.Body as NewExpression;
            methodcall = newexpr.Arguments[1] as MethodCallExpression;
            var ggw = methodcall.Method.GetGenericArguments();
            lambda = methodcall.Arguments[1] as LambdaExpression;
            ttype = methodcall.Arguments[1].GetType();

            //var bios_reg = new RegQuery<BIOS>()
            //   .useSetting(x =>
            //   {
            //       x.Hive = RegistryHive.LocalMachine;
            //       x.SubKey = @"HARDWARE\DESCRIPTION\System";
            //       x.View = RegistryView.Registry64;
            //   });
            //var bios = bios_reg.FirstOrDefault(x=>x.Key.Contains("BIOS"));

            var regt = new RegQuery<InstalledApp>()
                .useSetting(x =>
                    {
                        x.Hive = RegistryHive.LocalMachine;
                        x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                        x.View = RegistryView.Registry64;
                    });

            var regt1 = new RegQuery<InstalledApp>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.LocalMachine;
                    x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
                    x.View = RegistryView.Registry64;
                });

            var aaaa = regt.Except(regt1.Take(2));
            foreach(var oo in aaaa)
            {

            }

            var fir = regt.First();
            //var ttk = regt.TakeWhile((x, index) => index == 0);
            //int revv = regt.RemoveAll();
            var tuple = regt.Select(x => Tuple.Create(x.DisplayName, x.EstimatedSize));
            var tuple1 = regt.Select((x,idx) => Tuple.Create(x.DisplayName, idx));
            var ssssz = regt.Where(x=>x.DisplayName=="AA").RemoveAll();
            var group1 = regt.GroupBy(x => x.DisplayName, (key, reg) => reg);
            var avg = regt.Where(x => int.Parse(x.EstimatedSize.ToString()).ToString("X2") == "6E").Select(x=>x.DisplayName);
            foreach(var oo in avg)
            {

            }
            //var group4 = regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => data.Count());
            List<int> src1 = new List<int> { 1, 2, 3, 7, 7 };
            List<int> src2 = new List<int>() { 3, 4, 5 };
            //var dst1 = src1.Except(src2);
            //var dst2 = src1.Intersect(src2);
            //var dst3 = src1.Union(src2);
            //var dst4 = src1.Distinct();

            var dst1 = src1.Except(src1);
            var dst2 = src1.Intersect(src1);
            var dst3 = src1.Union(src1);


            //List<InstalledApp> src1 = new List<InstalledApp>
            //{
            //    new InstalledApp() { DisplayName = "1" },
            //    new InstalledApp() { DisplayName = "2" },
            //    new InstalledApp() { DisplayName = "3" },
            //    new InstalledApp() { DisplayName = "7" },
            //    new InstalledApp() { DisplayName = "7" },
            //};
            //List<InstalledApp> src2 = new List<InstalledApp>
            //{
            //    new InstalledApp() { DisplayName = "3" },
            //    new InstalledApp() { DisplayName = "4" },
            //    new InstalledApp() { DisplayName = "5" },
            //};
            //var dst1 = src1.Except(src2, new InstallAppCompare());
            //var dst2 = src1.Intersect(src2, new InstallAppCompare());
            //var dst3 = src1.Union(src2, new InstallAppCompare());
            //var dst4 = src1.Distinct(new InstallAppCompare());


            //var group1 = regt.GroupBy(x => x.DisplayName); //1146
            //var group2 = regt.GroupBy(x => x.DisplayName, y => y.DisplayName);//1074
            //var group3 = regt.GroupBy(x => x.DisplayName, (key, reg) => key);//1043
            //var group4 = regt.GroupBy(x => x.DisplayName, (key, reg) => reg);//1043
            //var group5 = regt.GroupBy(x => x.DisplayName, (key, reg) => new { reg });//1043
            //foreach(var item in group5)
            //{
            //    foreach(var oo in item.reg)
            //    {

            //    }
            //}



            var group7 = regt.GroupBy(x => x.DisplayName, y=>y.EstimatedSize, (x,e)=>new { x,e});//1006

            //var fi = regt.FirstOrDefault();
            //try
            //{
            //    int update_count2 = regt.Where(x=>x.IsOfficial==true).Update(x => new { EstimatedSize = x.EstimatedSize + 100 });
            //}
            //catch(Exception ee)
            //{

            //}
            var og = regt.First();
            //int update_count12 = regt.Where(x => x.EstimatedSize > 130).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //var group2 = regt.GroupBy(x => x.DisplayName, (key, app) => new { key, app });
            var takewhile1 = regt.AsEnumerable();
            var a = (a:123, b:"123");
            int.TryParse("100", out var bbbb);
            //foreach (var oo in takewhile1)
            //{

            //}
            var aa = regt.GroupBy(x => x.Version, (ver, app)=>new { ver, app});
            regt.GroupBy(x=>x, (x,y)=>x).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //regt.Where(x => x.EstimatedSize > 130).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            //var where_select = regt.Where(x => x.EstimatedSize > 130).Select((x, index) => x);
            //var group1 = regt.GroupBy(x => x.DisplayName);
            //var group2 = regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize);
            //var group3 = regt.GroupBy(x => x.DisplayName, (y, z) => z);
            //foreach (var oo in group3)
            //{

            //}
            //var where = regt.Where(x => x.DisplayName != ""&& x.EstimatedSize>0);
            var zip = regt.Zip(installs, (reg, app) => reg);
            var select = regt.Select((x, index) => index+1);
            var any = regt.Any(x => x.EstimatedSize > 0);
            var first1 = regt.First();
            var first2 = regt.First(x => x.DisplayName != "");
            var last1 = regt.Last();
            var last2 = regt.Last(x => x.DisplayName != "");
            var take = regt.Take(10);
            var takewhile = regt.TakeWhile(x => x.DisplayName == "AA");

            var count1 = regt.Count();
            var count2 = regt.Count(x => x.DisplayName == "AA");
            var all = regt.All(x => x.DisplayName != "");
            
            var reverse = regt.Reverse();
            var average = regt.Average(x => x.EstimatedSize);
            var sum = regt.Sum(x => x.EstimatedSize);
            var skip1 = regt.Skip(1);
            var skipwhile = regt.SkipWhile(x => x.DisplayName == "B");
            var min = regt.Min(x => x.EstimatedSize);
            var max = regt.Max(x => x.EstimatedSize);

            var loopup = regt.ToLookup(x => x.DisplayName);
            var tolist = regt.ToList();
            var toarray = regt.ToArray();
            var dictonary = regt.ToDictionary(x => x.EstimatedSize);
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
