using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System.Linq;

namespace LikeDB
{
    [TestClass]
    public class LikeDB
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

        [TestMethod]
        public void CreateDB()
        {
            regt_apps.RemoveAll();
            regt_apps.Insert(Enumerable.Range(1, 100).Select(x => new App() { ID = x, Name = $"App{x}", Version = new Version(x, x, x, x), Size = x + 1 }));
            regt_apps.Insert(new List<App>()
            {
                new App(){ID=1, Name="Camera", Version=new Version(1,1,1,1), Size=123 },
                new App(){ID=2, Name="Motion", Version=new Version(2,2,2,2), Size=123 },
                new App(){ID=3, Name="VLC.exe", Version=new Version(3,3,3,3), Size=123 },
                new App(){ID=4, Name="Joystick.cpl", Version=new Version(4,4,4,4), Size=123 },
                new App(){ID=5, Name="IPBoroadcast.exe", Version=new Version(5,5,5,5), Size=123 },
            });
            regt_company.RemoveAll();
            var companydatas = Enumerable.Range(1, 100).Select(x => new Company() { Key=x,ID=x,Name=$"Name_{x}", Address=$"Address_{x}" });
            regt_company.Insert(companydatas);
            //regt_company.Insert(new List<Company>()
            //{
            //    new Company(){ Key=1, ID=1, Name = "One" ,Address="Address_one"},
            //    new Company(){ Key=2, ID=2, Name = "two" ,Address="Address_two"},
            //    new Company(){ Key=3, ID=3, Name = "three" ,Address="Address_three"},
            //    new Company(){ Key=4, ID=4, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=5, ID=5, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=6, ID=6, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=7, ID=7, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=8, ID=8, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=9, ID=9, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=10, ID=10, Name = "Four" ,Address="Address_Four"},
            //    new Company(){ Key=11, ID=11, Name = "Four" ,Address="Address_Four"},
            //});

            regt_appmapping.RemoveAll();
            regt_appmapping.Insert(new List<AppMapping>()
            {
                new AppMapping(){AppID=1, CompanyID=1},
                new AppMapping(){AppID=2, CompanyID=1},
                new AppMapping(){AppID=3, CompanyID=2},
                new AppMapping(){AppID=4, CompanyID=2},
            });
        }

        [TestMethod]
        public void Join()
        {
            var join1 = regt_apps.Join(regt_appmapping, app => app.ID, mapping => mapping.AppID, (x,y)=>x);
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var join2 = apps.Join(mappings, app => app.ID, mapping => mapping.AppID, (x, y) => x);
            Check(join1, join2);
        }

        [TestMethod]
        public void Join1()
        {
            var join1 = regt_company.Join(regt_appmapping, company => company.Key, mapping => mapping.CompanyID, (x, y) => x);
            var companys = regt_company.ToList();
            var mappings = regt_appmapping.ToList();
            var join2 = companys.Join(mappings, company => company.Key, mapping => mapping.AppID, (x, y) => x);
            Check(join1, join2);
        }

        bool Check<T>(T src, T dst)
        {
            bool result = true;
            var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
            foreach(var pp in pps)
            {
                bool ch = false;
                var typecode = Type.GetTypeCode(pp.PropertyType);
                if(typecode == TypeCode.Object)
                {
                    if(pp.PropertyType == typeof(Version))
                    {

                    }
                    else
                    {
                        ch = true;
                        dynamic s = pp.GetValue(src);
                        dynamic d = pp.GetValue(dst);
                        if(s == null && d==null)
                        {

                        }
                        else
                        {
                            Check(s, d);
                        }
                    }
                }
                if(ch==false)
                {
                    dynamic s = pp.GetValue(src);
                    dynamic d = pp.GetValue(dst);
                    Assert.AreEqual(s, d, $"{s}!={d}");
                }
                
            }

            return result;
        }

        bool Check<T>(IEnumerable<T> src, IEnumerable<T> dst)
        {
            bool result = true;
            if(src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()}!=dst:{dst.Count()}");
                return false;
            }
            var zip = src.Zip(dst, (x, y) => new { src = x, dst = y });
            foreach(var oo in zip)
            {
                Check(oo.src, oo.dst);
            }

            return result;
        }

        [TestMethod]
        public void LeftJoin()
        {
            var gj1 = regt_apps.GroupJoin(regt_appmapping, app => app.ID, mapping => mapping.AppID, (app, mapping) => new {app, mapping });
            var left1 = gj1.SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping });
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var gj2 = apps.GroupJoin(mappings, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping });
            var left2 = gj2.SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping});
            Check(left1, left2);
        }

        [TestMethod]
        public void LeftJoin_Syntax()
        {
            var left1 = from app in regt_apps
                        join mapping in regt_appmapping on app.ID equals mapping.AppID into gj
                        from mapping in gj.DefaultIfEmpty()
                        select new { app, mapping };
            //var apps = regt_apps.ToList();
            //var mappings = regt_appmapping.ToList();
            //var left2 = from app in apps
            //            join mapping in mappings on app.ID equals mapping.AppID into gj
            //            from mapping in gj.DefaultIfEmpty()
            //            select new { app, mapping };


            //Check(left1, left2);
        }

        [TestMethod]
        public void LeftJoin_Syntax1()
        {
            var left1 = from company in regt_company
                        join mapping in regt_appmapping on company.ID equals mapping.CompanyID into gj_company_mapping
                        from company_mapping in gj_company_mapping.DefaultIfEmpty()
                        where company_mapping != null
                        join app in regt_apps on company_mapping.AppID equals app.ID into gj_app_mapping
                        from app_mapping in gj_app_mapping.DefaultIfEmpty()
                        select new
                        {
                            company = company.ID,
                            app = app_mapping.ID
                        };
            foreach (var oo in left1)
            {

            }
            //var apps = regt_apps.ToList();
            //var mappings = regt_appmapping.ToList();
            //var companys = regt_company.ToList();
            //var left2 = from company in companys
            //            join mapping in mappings on company.ID equals mapping.CompanyID into gj_company_mapping
            //            from company_mapping in gj_company_mapping.DefaultIfEmpty()
            //            where company_mapping != null
            //            join app in apps on company_mapping.AppID equals app.ID into gj_app_mapping
            //            from app_mapping in gj_app_mapping.DefaultIfEmpty()
            //            select new
            //            {
            //                company = company.ID,
            //                app = app_mapping.ID
            //            };



            //Check(left1, left2);
        }

        [TestMethod]
        public void RightJoin()
        {
            var gj1 = regt_appmapping.GroupJoin(regt_apps, mapping => mapping.AppID, app => app.ID, (mapping, app) => new { mapping, app });
            var left1 = gj1.SelectMany(x => x.app.DefaultIfEmpty(), (mapping, app) => new { mapping.mapping, app });
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var gj2 = mappings.GroupJoin(apps, mapping => mapping.AppID, app => app.ID, (mapping, app) => new { mapping, app });
            var left2 = gj2.SelectMany(x => x.app.DefaultIfEmpty(), (mapping, app) => new { mapping.mapping, app });
            Check(left1, left2);
        }

    }

    public class App
    {
        public int ID { set; get; }
        [RegPropertyName(Name = "DisplayName")]
        public string Name { set; get; }
        [RegPropertyName(Name = "DisplayVersion")]
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
        [RegPropertyName(Name = "ID")]
        public int ID { set; get; }
        public string Address { set; get; }

    }

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }
}
