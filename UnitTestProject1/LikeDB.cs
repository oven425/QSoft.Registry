using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System.Linq;
using General;
using UnitTest;

namespace LikeDB
{
    [TestClass]
    public class LikeDB
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
            }).useConverts(x=>x.Add(new Version2String()));


        [TestMethod]
        public void BuildMockup()
        {
            regt_apps.RemoveAll();
            //regt_apps.Insert(Enumerable.Range(1, 50000).Select(x => new App() { ID = x, DisplayName = $"App{x}", Version = new Version(x, x, x, x), Size = x + 1 }));
            regt_apps.Insert(new List<App>()
            {
                new App(){ID=1, DisplayName="Camera", Version=new Version(1,1,1,1), Size=123 },
                new App(){ID=2, DisplayName="Motion", Version=new Version(2,2,2,2), Size=123 },
                new App(){ID=3, DisplayName="VLC.exe", Version=new Version(3,3,3,3), Size=123 },
                new App(){ID=4, DisplayName="Joystick.cpl", Version=new Version(4,4,4,4), Size=123 },
                new App(){ID=5, DisplayName="IPBoroadcast.exe", Version=new Version(5,5,5,5), Size=123 },
                new App(){ID=6, DisplayName="PLC.exe", Version=new Version(6,6,6,6), Size=123 },
            });
            regt_company.RemoveAll();
            //var companydatas = Enumerable.Range(1, 10).Select(x => new Company() { Key=x,Name=$"Name_{x}", Address=$"Address_{x}" });
            //regt_company.Insert(companydatas);
            regt_company.Insert(new List<Company>()
            {
                new Company(){ Key=1, Name = "One" ,Address="Address_one"},
                new Company(){ Key=2, Name = "Two" ,Address="Address_two"},
                new Company(){ Key=3, Name = "Three" ,Address="Address_three"},
                new Company(){ Key=4, Name = "Four" ,Address="Address_four"},
            });

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
            var apps = join1.ToList();
            var mappings = regt_appmapping.ToList();
            var join2 = apps.Join(mappings, app => app.ID, mapping => mapping.AppID, (x, y) => x);
            CheckEx.Check(join1, join2);

        }

        [TestMethod]
        public void Join1()
        {
            var join1 = regt_company.Join(regt_appmapping, company => company.Key, mapping => mapping.CompanyID, (x, y) => x);
            var companys = regt_company.ToList();
            var mappings = regt_appmapping.ToList();
            var join2 = companys.Join(mappings, company => company.Key, mapping => mapping.CompanyID, (x, y) => x);
            CheckEx.Check(join1, join2);
        }

        //bool Check<T>(T src, T dst)
        //{
        //    bool result = true;
        //    var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
        //    foreach(var pp in pps)
        //    {
        //        bool ch = false;
        //        var typecode = Type.GetTypeCode(pp.PropertyType);
        //        if(typecode == TypeCode.Object)
        //        {
        //            if(pp.PropertyType == typeof(Version))
        //            {

        //            }
        //            else
        //            {
        //                ch = true;
        //                dynamic s = pp.GetValue(src);
        //                dynamic d = pp.GetValue(dst);
        //                if(s == null && d==null)
        //                {

        //                }
        //                else
        //                {
        //                    Check(s, d);
        //                }
        //            }
        //        }
        //        if(ch==false)
        //        {
        //            dynamic s = pp.GetValue(src);
        //            dynamic d = pp.GetValue(dst);
        //            Assert.AreEqual(s, d, $"{s}!={d}");
        //        }
                
        //    }

        //    return result;
        //}

        //bool Check<T>(IEnumerable<T> src, IEnumerable<T> dst)
        //{
        //    bool result = true;
        //    if(src.Count() != dst.Count())
        //    {
        //        Assert.Fail($"src:{src.Count()}!=dst:{dst.Count()}");
        //        return false;
        //    }
        //    var zip = src.Zip(dst, (x, y) => new { src = x, dst = y });
        //    foreach(var oo in zip)
        //    {
        //        Check(oo.src, oo.dst);
        //    }

        //    return result;
        //}

        [TestMethod]
        public void LeftJoin_2Table()
        {
            //var gj1 = regt_apps.GroupJoin(regt_appmapping, app => app.ID, mapping => mapping.AppID, (app, mapping) => new {app, mapping });
            var left1 = regt_apps.GroupJoin(regt_appmapping, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping});
            var nomapping = left1.Where(x => x.mapping == null).Select(x => x.app);
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var gj2 = apps.GroupJoin(mappings, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping });
            var left2 = gj2.SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping});
            CheckEx.Check(left1, left2);
        }

        [TestMethod]
        public void LeftJoin_2Table_Syntax()
        {
            var left1 = from app in regt_apps
                        join mapping in regt_appmapping on app.ID equals mapping.AppID into gj
                        from mapping in gj.DefaultIfEmpty()
                        select new { app, mapping };
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var left2 = from app in apps
                        join mapping in mappings on app.ID equals mapping.AppID into gj
                        from mapping in gj.DefaultIfEmpty()
                        select new { app, mapping };


            CheckEx.Check(left1, left2);
        }

        [TestMethod]
        public void LeftJoin_3Table()
        {
            var left1 = regt_company.GroupJoin(regt_appmapping, company => company.Key, mapping => mapping.CompanyID, (company, mapping) => new { company, mapping })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), (company, mapping) => new { company.company, mapping })
                .Where(x => x.mapping != null)
                .GroupJoin(regt_apps, mapping => mapping.mapping.AppID, app => app.ID, (x, y) => new { x.company, app = y })
                .SelectMany(x => x.app.DefaultIfEmpty(), (x, y) => new { Company = x.company.Name, App = y.DisplayName });
            //foreach(var oo in left1)
            //{

            //}
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var companys = regt_company.ToList();
            var left2 = companys.GroupJoin(mappings, company => company.Key, mapping => mapping.CompanyID, (company, mapping) => new { company, mapping })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), (company, mapping) => new { company.company, mapping })
                .Where(x => x.mapping != null)
                .GroupJoin(apps, mapping => mapping.mapping.AppID, app => app.ID, (x, y) => new { x.company, app = y })
                .SelectMany(x => x.app.DefaultIfEmpty(), (x, y) => new { Company = x.company.Name, App = y.DisplayName });
            CheckEx.Check(left1, left2);
        }

        [TestMethod]
        public void LeftJoin_3Table_Syntax()
        {
            var left1 = from company in regt_company
                        join mapping in regt_appmapping on company.Key equals mapping.CompanyID into gj_company_mapping
                        from company_mapping in gj_company_mapping.DefaultIfEmpty()
                        where company_mapping != null
                        join app in regt_apps on company_mapping.AppID equals app.ID into gj_app_mapping
                        from app_mapping in gj_app_mapping.DefaultIfEmpty()
                        select new
                        {
                            company = company,
                            app = app_mapping
                        };
            //foreach (var oo in left1)
            //{

            //}
            var apps = regt_apps.ToList();
            var mappings = regt_appmapping.ToList();
            var companys = regt_company.ToList();
            var left2 = from company in companys
                        join mapping in mappings on company.Key equals mapping.CompanyID into gj_company_mapping
                        from company_mapping in gj_company_mapping.DefaultIfEmpty()
                        where company_mapping != null
                        join app in apps on company_mapping.AppID equals app.ID into gj_app_mapping
                        from app_mapping in gj_app_mapping.DefaultIfEmpty()
                        select new
                        {
                            company = company,
                            app = app_mapping
                        };



            CheckEx.Check(left1, left2);
        }

        [TestMethod]
        public void Delete()
        {
            //var allcount = regt_company.Count();
            //int count = regt_company.Where(x => x.Key > 1).RemoveAll();
            //var where = regt_company.Where(x => x.Key <= 1);
            //if(allcount - count != where.Count())
            //{
            //    Assert.Fail("remove fail");
            //}
            //regt_apps.Where(x => x.Version != new Version(1, 1, 1, 1)).RemoveAll();
            //var apps = regt_apps.ToList();
        }

        [TestMethod]
        public void Update()
        {
            regt_company.Update(x => new Company() { Address = $"{x.Name}_{x.Name}" });
            regt_company.Where(x => x.Name == "One").Update(x => new { Address = "Test" });
        }
    }

    public class App
    {
        public int ID { set; get; }
        [RegPropertyName(Name ="Name")]
        public string DisplayName { set; get; }
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

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }
}
