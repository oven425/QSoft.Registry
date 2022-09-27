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
        public string Address { set; get; }
    }

    public class InstalledApp
    {
        public string DisplayName { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
        public Version Version { set; get; }
        public int? EstimatedSize { set; get; }
        public DateTime? Now { set; get; }
        //[RegIgnore]
        public bool? IsOfficial { set; get; }
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

    public class Phone
    {
        public string company { set; get; }
        public string number { set; get; }
        public Record First { set; get; }
        public Record Last { set; get; }
    }

    public class Record
    {
        public DateTime Start { set; get; }
        public DateTime Stop { set; get; }
    }

    public class People
    {
        public string Name { set; get; }
        [RegSubKeyName]
        public string Key { set; get; }
        public int? Weight { set; get; }
        [RegPropertyName(Name = "height")]
        public int Height { set; get; }

        [RegPropertyName(Name = "Phone1")]
        public Phone phone { set; get; }
        //public List<Phone> phones { set; get; }
    }

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/Converters/CastingConverter.cs
    public class Version2String : RegQueryConvert<Version>
    {
        public override string ConvertTo(Version src)
        {
            return src.ToString();
        }

        public override Version ConvertBack(string dst)
        {
            Version version;
            Version.TryParse(dst, out version);
            return version;
        }
    }

    public class Size2String : RegQueryConvert<Size>
    {
        public override Size ConvertBack(string dst)
        {
            var sl = dst.Split(',');
            int w = int.Parse(sl[0]);
            int h = int.Parse(sl[1]);
            return new Size() { Width = w, Height = h };
        }

        public override string ConvertTo(Size src)
        {
            return $"{src.Width},{src.Height}";
        }
    }

    class Ddaaa
    {
        public int? A1 { set; get; }
        public int A2 { set; get; }
        public Ddaaa(int dd)
        {
            A1 = dd;
            A2 = dd;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //TestDB();
                //Version2String vv = new Version2String();
                //vv.CanConvert(typeof(Version), typeof(string));
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




                Dictionary<Tuple<Type, Type>, object> dics = new Dictionary<Tuple<Type, Type>, object>();
                dics.Add(Tuple.Create(typeof(Version), typeof(string)), new Version2String());


                var ccs = typeof(Device).GetConstructors();
                var regt_devices = new RegQuery<Device>()
                    .useSetting(x =>
                    {
                        x.Hive = RegistryHive.CurrentConfig;
                        x.SubKey = @"devices";
                        x.View = RegistryView.Registry64;
                    })
                    .useConverts(x=>
                    {
                        x.Add(new Version2String());
                        //new Size2String()
                    });
                var first2 = regt_devices.Where(x => !string.IsNullOrEmpty(x.Remote.IP)).ToList();
                //regt_devices.Update(x => new
                //{
                //    Size = new
                //    {
                //        Width = 1111,
                //        Height = 2222
                //    }
                //});

                regt_devices.Update(x => new Device()
                {
                    Size = new Size()
                    {
                        Width = 101010,
                        Height = 212121,
                    }
                });

                //var llo = regt_devices.Select(x => new
                //{
                //    version = x.Version,
                //    sz = x.Location.Floor.Area.Data.Size,
                //    id = x.ID,
                //    localport = x.Local.Port,
                //    pir_auto = x.CameraSetting.PIR.IsAuto,
                //    pir_enable = x.CameraSetting.PIR.IsEnable,
                //    setting = new
                //    {
                //        aa = x.CameraSetting.Brightness,
                //        pir = new
                //        {
                //            auto = x.CameraSetting.PIR.IsAuto,
                //            enable = x.CameraSetting.PIR.IsEnable
                //        }
                //    }
                //});
                //var iiu = Expression.Constant(null);
                //var llo = regt_devices.Where(x => x.Version == new Version("1.1.1.1"));
                //var ffi = regt_devices.FirstOrDefault(x => x.Location.Floor.Area.Name == "Area_1");
                //var llo = regt_devices.Select(x => x.Location.Floor.Area.Data.Size);
                //foreach (var oo in llo)
                //{
                //}

                //regt_devices.Insert(new List<Device>()
                //{
                //    new Device()
                //    {
                //        Version = new Version("1.1.1.1"),
                //        Name = "1F_AA",
                //        Size = new Size(){Width=100,Height=100 },
                //        Local = new Address()
                //        {
                //            IP = "127.0.0.1",
                //            Port=1000,
                //            Root = new Address.Auth()
                //            {
                //                Account = "root_local",
                //                Password="root_local"
                //            },
                //            Guest = new Address.Auth()
                //            {
                //                Account = "guest_local",
                //                Password="guest_local"
                //            }
                //        },
                //        Remote = new Address()
                //        {
                //            IP="192.168.10.1",
                //            Port = 1001,
                //            Root = new Address.Auth()
                //            {
                //                Account = "root_local",
                //                Password="root_local"
                //            },
                //            Guest = new Address.Auth()
                //            {
                //                Account = "guest_local",
                //                Password="guest_local"
                //            }
                //        },
                //        CameraSetting = new CameraSetting()
                //        {
                //            PIR = new PIR(){ IsEnable=true, IsAuto=true },
                //            WDR = new WDR(){IsEnable=true},
                //            Brightness = new Brightness()
                //            {
                //                Range = new Range(){Min=0, Max=1000},
                //                Current = 500,
                //                CanEdit=true
                //            }
                //        },
                //        Location = new Locationata()
                //        {
                //            Name = "DD",
                //            Floor = new FloorData()
                //            {
                //                Name = "1F",
                //                Area = new AreaData()
                //                {
                //                    Name = "aaa",
                //                    Data = new Rect()
                //                    {
                //                        Point = new Point()
                //                        {
                //                            X = 100,
                //                            Y=200
                //                        },
                //                        Size = new Size()
                //                        {
                //                            Width = 111,
                //                            Height=222
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //});
                RegQuery<Company> regt_company = new RegQuery<Company>()
                    .useSetting(x =>
                    {
                        x.Hive = RegistryHive.CurrentConfig;
                        x.SubKey = @"UnitTestLikeDB\Company";
                        x.View = RegistryView.Registry64;
                    });
                regt_company.Update(x => new Company() { Address = $"{x.Name}_{x.Name}" });
            }
            catch (Exception ee)
            {
                System.Diagnostics.Trace.WriteLine(ee.Message);
            }


            InstalledApp installedapp = new InstalledApp();
            var pps = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach (var pp in pps)
            {
                var pv = pp.GetValue(installedapp, null);
            }


            var regt = new RegQuery<InstalledApp>()
                //.HasDefault1(x =>
                //{
                //    x.Now = DateTime.Now;
                //    x.DisplayName = "AA";
                //})
                //.HasDefault(() => new InstalledApp()
                //{
                //    Now = DateTime.Now,
                //    DisplayName = "A"
                //})
                .useSetting(x =>
                {
                    //x.Hive = RegistryHive.LocalMachine;
                    //x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                    //x.View = RegistryView.Registry64;
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTest\Apps";
                })
                .useConverts(x =>
                {
                    x.Add(new Version2String());
                });
            //var aa = regt.Select((x, index) => new { index = index+1, x });
            //foreach (var oo in aa)
            //{

            //}

            //var groupaa = regt.GroupBy(x => x.DisplayName, (key, reg) => new { reg });
            //foreach (var oo in groupaa)
            //{

            //}
            //var a2 = regt.ToList().Select(x => $"IsOfficial:{x.IsOfficial}");
            var a1 = regt.Where(x => x.DisplayName != "").FirstOrDefault();
            var a2 = regt.Where(x => x.DisplayName != "");
            //foreach (var oo in a1)
            //{

            //}
            var aa = regt.Average(x => x.DisplayName.Length);
            RegQuery<AppMapping> regt_appmapping = new RegQuery<AppMapping>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\AppMapping";
                x.View = RegistryView.Registry64;
            });
            RegQuery<App> regt_apps = new RegQuery<App>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\Apps";
            }).useConverts(x => x.Add(new Version2String()));
            var join1 = regt_apps.Join(regt_appmapping, app => app.ID, mapping => mapping.AppID, (x, y) => x);
            var apps = join1.ToList();

            //int update_count = regt.Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize + 100 });
            var o1o = regt.GroupBy(x => x);
            foreach (var oo in o1o)
            {

            }
            var llen = regt.Select(x => x.Version.ToString());
            foreach (var oo in llen)
            {

            }

            var aaaa = regt.Select(x => x.Version);
            foreach(var oo in aaaa)
            {

            }
            var sytnax = from oo in regt
                         where oo.DisplayName != null
                         let kk1 =oo.DisplayName.ToUpper()
                         let kk2 = oo.DisplayName.ToLower()
                         select oo;
            ////foreach(var oo in sytnax)
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
            //regt.RemoveAll();
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
                 x.SubKey = @"UnitTestLikeDB\AppMapping";
                 x.View = RegistryView.Registry64;
             });

            RegQuery<Company> regt_company = new RegQuery<Company>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTestLikeDB\Company";
                    x.View = RegistryView.Registry64;
                });


            RegQuery<App> regt_apps = new RegQuery<App>()
                .useSetting(x =>
                {
                    x.Hive = RegistryHive.CurrentConfig;
                    x.SubKey = @"UnitTestLikeDB\Apps";
                }).useConverts(x => x.Add(new Version2String()));
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

            var left1 = regt_company.GroupJoin(regt_appmapping, company => company.Key, mapping => mapping.CompanyID, (company, mapping) => new { company, mapping })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), (company, mapping) => new { company.company, mapping })
                .Where(x => x.mapping != null)
                .GroupJoin(regt_apps, mapping => mapping.mapping.AppID, app => app.ID, (x, y) => new { x.company, app = y })
                .SelectMany(x => x.app.DefaultIfEmpty(), (x, y) => new { Company = x.company.Name, App = y.Name });
            foreach (var oo in left1)
            {

            }


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
