using Microsoft.Win32;
using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Test1
{
    public class Test1
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

        public void Test()
        {
            try
            {
                //var left1 = regt_apps.GroupJoin(regt_appmapping, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping })
                //    .SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app,mapping });
                //var arrsy = left1.ToArray();

                var left1 = regt_apps.GroupJoin(regt_appmapping, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping })
                .SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping });
                //var nomapping = left1.Where(x => x.mapping == null).Select(x => x.app);
                //var apps = regt_apps.ToList();
                //var mappings = regt_appmapping.ToList();
                //var gj2 = apps.GroupJoin(mappings, app => app.ID, mapping => mapping.AppID, (app, mapping) => new { app, mapping });
                //var left2 = gj2.SelectMany(x => x.mapping.DefaultIfEmpty(), (app, mapping) => new { app.app, mapping });
                for (int i = 0; i < left1.Count(); i++)
                {
                    System.Diagnostics.Trace.WriteLine($"{i}");
                    left1.ElementAt(i);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }




    public class App
    {
        public int ID { set; get; }
        [RegPropertyName(Name = "Name")]
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
