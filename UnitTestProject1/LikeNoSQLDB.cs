//using General;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.Win32;
//using QSoft.Registry.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LikeNoSQLDB
//{
//    [TestClass]
//    public class LikeNoSQLDB
//    {

//        RegQuery<AppMapping> regt_appmapping = new RegQuery<AppMapping>()
//            .useSetting(x =>
//            {
//                x.Hive = RegistryHive.CurrentConfig;
//                x.SubKey = @"UnitTestLikeDB\AppMapping";
//                x.View = RegistryView.Registry64;
//            });

//        RegQuery<Company> regt_company = new RegQuery<Company>()
//            .useSetting(x =>
//            {
//                x.Hive = RegistryHive.CurrentConfig;
//                x.SubKey = @"UnitTestLikeDB\Company";
//                x.View = RegistryView.Registry64;
//            });


//        RegQuery<App> regt_apps = new RegQuery<App>()
//            .useSetting(x =>
//            {
//                x.Hive = RegistryHive.CurrentConfig;
//                x.SubKey = @"UnitTestLikeDB\Apps";
//            }).useConverts(x => x.Add(new Version2String()));

//        [TestMethod]
//        public void BuildMockup()
//        {
//            regt_apps.RemoveAll();
//            //regt_apps.Insert(Enumerable.Range(1, 50000).Select(x => new App() { ID = x, DisplayName = $"App{x}", Version = new Version(x, x, x, x), Size = x + 1 }));
//            regt_apps.Insert(new List<App>()
//            {
//                new App(){ID=1, DisplayName="Camera", Version=new Version(1,1,1,1), Size=123 },
//                new App(){ID=2, DisplayName="Motion", Version=new Version(2,2,2,2), Size=123 },
//                new App(){ID=3, DisplayName="VLC.exe", Version=new Version(3,3,3,3), Size=123 },
//                new App(){ID=4, DisplayName="Joystick.cpl", Version=new Version(4,4,4,4), Size=123 },
//                new App(){ID=5, DisplayName="IPBoroadcast.exe", Version=new Version(5,5,5,5), Size=123 },
//                new App(){ID=6, DisplayName="PLC.exe", Version=new Version(6,6,6,6), Size=123 },
//            });
//            regt_company.RemoveAll();
//            //var companydatas = Enumerable.Range(1, 10).Select(x => new Company() { Key=x,Name=$"Name_{x}", Address=$"Address_{x}" });
//            //regt_company.Insert(companydatas);
//            regt_company.Insert(new List<Company>()
//            {
//                new Company(){ Key=1, Name = "One" ,Address="Address_one"},
//                new Company(){ Key=2, Name = "Two" ,Address="Address_two"},
//                new Company(){ Key=3, Name = "Three" ,Address="Address_three"},
//                new Company(){ Key=4, Name = "Four" ,Address="Address_four"},
//            });


//        }
//    }

//    public class App
//    {
//        public int ID { set; get; }
//        [RegPropertyName(Name = "Name")]
//        public string DisplayName { set; get; }
//        public Version Version { set; get; }
//        [RegIgnore]
//        public int Size { set; get; }
//    }

//    public class Company
//    {
//        [RegSubKeyName]
//        public int Key { set; get; }
//        [RegPropertyName(Name = "Name1")]
//        public string Name { set; get; }
//        public string Address { set; get; }
//    }

//    public class AppMapping
//    {
//        public int CompanyID { set; get; }
//        public int AppID { set; get; }
//    }
//}
