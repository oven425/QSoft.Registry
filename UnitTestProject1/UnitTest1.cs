using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry;
using QSoft.Registry.Linq;
using UnitTest;

namespace General
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

    public class Company
    {
        public string Name { set; get; }
        public int ID { set; get; }

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
        public int? ID { set; get; }

        public InstalledApp() { }

        public InstalledApp(InstalledApp data)
        {
            var pps = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach(var pp in pps)
            {
                pp.SetValue(this, pp.GetValue(data));
            }
        }
    }

    public class Version2String : RegQueryConvert<Version>
    {

        public override Version ConvertBack(string dst)
        {
            return Version.Parse(dst);
        }

        public override string ConvertTo(Version src)
        {
            return src.ToString();
        }
    }

    public class AppMapping
    {
        public int CompanyID { set; get; }
        public int AppID { set; get; }
    }

    [TestClass]
    public class LinqToRegistry
    {
        List<InstalledApp> m_Tests = new List<InstalledApp>();
        List<AppData> m_Apps = new List<AppData>();
        public LinqToRegistry()
        {
            this.m_Tests = this.InstallApp_org();

            m_Apps.Add(new AppData() { Name = "A", IsOfficial = true });
            m_Apps.Add(new AppData() { Name = "AA", IsOfficial = false });
        }

        List<InstalledApp> InstallApp_org()
        {
            return regt.ToList();
            //List<InstalledApp> datas = new List<InstalledApp>();
            //datas.Add(new InstalledApp() { Key="AA", DisplayName = "AA", Version = new Version("1.1.1.1"), EstimatedSize = 10, ID=0});
            //datas.Add(new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.2"), EstimatedSize = 10, ID = 1 });
            //datas.Add(new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.3"), EstimatedSize = 10, ID = 2 });
            //datas.Add(new InstalledApp() { Key = "BB", DisplayName = "BB", Version = new Version("2.2.2.2"), EstimatedSize = 20, ID = 3 });
            //datas.Add(new InstalledApp() { Key = "CC", DisplayName = "CC", Version = new Version("3.3.3.3"), EstimatedSize = 30, ID = 4 });
            //datas.Add(new InstalledApp() { Key = "DD", DisplayName = "DD", Version = new Version("4.4.4.4"), EstimatedSize = 40, ID = 5 });
            //datas.Add(new InstalledApp() { Key = "EE", DisplayName = "EE", Version = new Version("5.5.5.5"), EstimatedSize = 50, ID = 6 });
            //datas.Add(new InstalledApp() { Key = "FF", DisplayName = "FF", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 7 });

            //return datas;
        }

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


        RegQuery<InstalledApp> regt = new RegQuery<InstalledApp>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\Apps";
            })
            .useConverts(new List<RegQueryConvert>()
            {
                new Version2String()
            });
        [TestCategory("Init")]
        [TestMethod]
        public void BuildMockup()
        {
            regt_company.RemoveAll();
            regt_company.Insert(new List<Company>()
            {
                new Company(){Name = "Company_A", ID=1},
                new Company(){Name = "Company_B", ID=2},
                new Company(){Name = "Company_C", ID=3},
                new Company(){Name = "Company_D", ID=4},
                new Company(){Name = "Company_E", ID=5},
            });
            regt.RemoveAll();
            regt.Insert(new List<InstalledApp>()
            {
                new InstalledApp() { Key="AA", DisplayName = "AA", Version = new Version("1.1.1.1"), EstimatedSize = 10, ID=0},
                new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.2"), EstimatedSize = 10, ID = 1 },
                new InstalledApp() { Key = "AA", DisplayName = "AA", Version = new Version("1.1.1.3"), EstimatedSize = 10, ID = 2 },
                new InstalledApp() { Key = "BB", DisplayName = "BB", Version = new Version("2.2.2.2"), EstimatedSize = 20, ID = 3 },
                new InstalledApp() { Key = "CC", DisplayName = "CC", Version = new Version("3.3.3.3"), EstimatedSize = 30, ID = 4 },
                new InstalledApp() { Key = "DD", DisplayName = "DD", Version = new Version("4.4.4.4"), EstimatedSize = 40, ID = 5 },
                new InstalledApp() { Key = "EE", DisplayName = "EE", Version = new Version("5.5.5.5"), EstimatedSize = 50, ID = 6 },
                new InstalledApp() { Key = "FF", DisplayName = "FF", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 7 },
                new InstalledApp() { Key = "FF", DisplayName = "GG", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 8 },
                new InstalledApp() { Key = "FF", DisplayName = "HH", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 9 },
                new InstalledApp() { Key = "FF", DisplayName = "II", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 10 },
                new InstalledApp() { Key = "FF", DisplayName = "JJ", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 11 },
                new InstalledApp() { Key = "FF", DisplayName = "KK", Version = new Version("6.6.6.6"), EstimatedSize = 60, ID = 12 },
            });
            this.m_Tests = regt.ToList();
            regt_appmapping.RemoveAll();
            regt_appmapping.Insert(new List<AppMapping>()
            {
                new AppMapping(){AppID = 0, CompanyID = 0},
                new AppMapping(){AppID = 1, CompanyID = 0},
                new AppMapping(){AppID = 2, CompanyID = 0},
                new AppMapping(){AppID = 3, CompanyID = 0},
                new AppMapping(){AppID = 4, CompanyID = 1}
            });
            //var comapnys = regt_company.Join(regt_appmapping, x => x.ID, y => y.CompanyID, (x, y) => y).Join(regt, x=>x.AppID, y=>y.ID,(x,y)=>y);
            //foreach(var oo in comapnys)
            //{

            //}
            //RegistryKey regbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //var reg = regbase.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);
            //try
            //{
            //    reg.DeleteSubKeyTree("1A");
            //}
            //catch(Exception ee)
            //{
            //    System.Diagnostics.Trace.WriteLine(ee.Message);
            //    System.Diagnostics.Trace.WriteLine(ee.StackTrace);
            //}


            //var propertys = typeof(InstalledApp).GetProperties().Where(x => x.CanRead == true&&x.GetCustomAttributes(true).Any(y=>y is RegIgnore || y is RegSubKeyName)==false);

            //var test1A = reg.CreateSubKey(@"1A", true);
            //foreach(var oo in this.m_Tests)
            //{
            //    var regd = test1A.CreateSubKey(oo.Key, true);
            //    foreach(var pp in propertys)
            //    {
            //        var doo = pp.GetValue(oo);
            //        if(doo!=null)
            //        {
            //            var regnames = pp.GetCustomAttributes(typeof(RegPropertyName), false) as RegPropertyName[];
            //            string subkeyname = pp.Name;
            //            if (regnames.Length > 0)
            //            {
            //                subkeyname = regnames[0].Name;
            //            }
            //            regd.SetValue(subkeyname, doo);
            //        }


            //    }
            //    //var parent = regd.GetParent();
            //    //parent.DeleteSubKeyTree(oo.DisplayName);
            //    regd.Close();
            //    regd.Dispose();
            //}

        }

        [TestMethod]
        public void GroupBy1_Select()
        {
            CheckEx.Check(this.m_Tests.GroupBy(x => x).Select(x=>x.Key), regt.GroupBy(x => x).Select(x => x.Key));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName).Select(x => x.Key), regt.GroupBy(x => x.DisplayName).Select(x => x.Key));
        }

        [TestMethod]
        public void GroupBy1()
        {
            CheckEx.Check(this.m_Tests.GroupBy(x => x), regt.GroupBy(x => x));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName), regt.GroupBy(x => x.DisplayName));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.EstimatedSize), regt.GroupBy(x => x.EstimatedSize));
        }

        [TestMethod]
        public void GroupBy2()
        {
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x=>x.DisplayName), regt.GroupBy(x => x.DisplayName, x => x.DisplayName));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x => x.EstimatedSize), regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x => new { x.DisplayName, x.EstimatedSize }), regt.GroupBy(x => x.DisplayName, x => new { x.DisplayName, x.EstimatedSize }));
        }

        [TestMethod]
        public void GroupBy3()
        {
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, (key, reg) => key), regt.GroupBy(x => x.DisplayName, (key, reg) => key));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, (key, reg) => reg), regt.GroupBy(x => x.DisplayName, (key, reg) => reg));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, (key, reg) => new { key }), regt.GroupBy(x => x.DisplayName, (key, reg) => new { key }));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, (key, reg) => new { reg }), regt.GroupBy(x => x.DisplayName, (key, reg) => new { reg }));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, (key, reg) => new { key.Length }), regt.GroupBy(x => x.DisplayName, (key, reg) => new { key.Length }));
        }

        [TestMethod]
        public void GroupBy4()
        {
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x=>x.EstimatedSize, (key, data) => data), regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => data));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => data.Count()), regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => data.Count()));
            CheckEx.Check(this.m_Tests.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => new { Key=key.ToString(), Count=data.Count() }), regt.GroupBy(x => x.DisplayName, x => x.EstimatedSize, (key, data) => new { Key = key.ToString(), Count = data.Count() }));
        }      

        //void Check<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> src, IEnumerable<IGrouping<TKey, TElement>> dst)
        //{
        //    if (src.Count() != dst.Count())
        //    {
        //        Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
        //    }

        //    for (int i = 0; i < src.Count(); i++)
        //    {
        //        dynamic key_src = src.ElementAt(i).Key;
        //        dynamic key_dst = dst.ElementAt(i).Key;
        //        Check(key_src, key_dst);
                
        //        int count_src = src.ElementAt(i).Count();
        //        int count_dst = dst.ElementAt(i).Count();
        //        Assert.IsTrue(count_src == count_dst, $"Count fail src:{count_src} dst:{count_dst}");
        //        for (int j = 0; j < count_src; j++)
        //        {
        //            this.Check(src.ElementAt(i).ElementAt(j), dst.ElementAt(i).ElementAt(j));
        //        }
        //    }
        //}

        [TestMethod]
        public void Take()
        {
            CheckEx.Check(this.m_Tests.Take(3), regt.Take(3));
            CheckEx.Check(this.m_Tests.Take(100), regt.Take(100));
        }

        [TestMethod]
        public void TakeWhile()
        {
            CheckEx.Check(this.m_Tests.TakeWhile(x => x.DisplayName == "AA"), regt.TakeWhile(x => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.TakeWhile(x => x.EstimatedSize.ToString() == "10"), regt.TakeWhile(x => x.EstimatedSize.ToString() == "10"));
            CheckEx.Check(this.m_Tests.TakeWhile(x => x.DisplayName == "AA".ToString()), regt.TakeWhile(x => x.DisplayName == "AA".ToString()));
            CheckEx.Check(this.m_Tests.TakeWhile(x => x.Key.Contains("AA".ToString())), regt.TakeWhile(x => x.Key.Contains("AA".ToString())));
        }

        [TestMethod]
        public void TakeWhile_Index()
        {
            CheckEx.Check(this.m_Tests.TakeWhile((x,index) => x.DisplayName == "AA"), regt.TakeWhile((x,index) => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.TakeWhile((x, index) => index == 0), regt.TakeWhile((x, index) => index == 0));
        }

        [TestMethod]
        public void Skip()
        {
            CheckEx.Check(this.m_Tests.Skip(3), regt.Skip(3));
        }

        [TestMethod]
        public void SkipWhile()
        {
            CheckEx.Check(this.m_Tests.SkipWhile(x=>x.EstimatedSize>20), regt.SkipWhile(x => x.EstimatedSize > 20));
        }

        [TestMethod]
        public void SkipWhile_Index()
        {
            CheckEx.Check(this.m_Tests.SkipWhile((x,index) => x.EstimatedSize > 20), regt.SkipWhile((x,index) => x.EstimatedSize > 20));
        }

        [TestMethod]
        public void Where()
        {
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName.ToString() == "AA".ToString()), regt.Where(x => x.DisplayName.ToString() == "AA".ToString()));
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName == "AA"), regt.Where(x => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName == $"{x.DisplayName}"), regt.Where(x => x.DisplayName == $"{x.DisplayName}"));
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName == $"{x.DisplayName}{x.DisplayName}"), regt.Where(x => x.DisplayName == $"{x.DisplayName}{x.DisplayName}"));
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA")), regt.Where(x => x.DisplayName.Contains("AA")));
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA") == true), regt.Where(x => x.DisplayName.Contains("AA") == true));
            CheckEx.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName)), regt.Where(x => string.IsNullOrEmpty(x.DisplayName)));
            CheckEx.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName) == true), regt.Where(x => string.IsNullOrEmpty(x.DisplayName) == true));

            CheckEx.Check(this.m_Tests.Where(x => x.EstimatedSize > 0), regt.Where(x => x.EstimatedSize > 0));
            CheckEx.Check(this.m_Tests.Where(x => x.EstimatedSize.ToString() == "10"), regt.Where(x => x.EstimatedSize.ToString() == "10"));
            CheckEx.Check(this.m_Tests.Where(x => x.EstimatedSize.ToString() == $"{x.EstimatedSize}"), regt.Where(x => x.EstimatedSize.ToString() == $"{x.EstimatedSize}"));
            CheckEx.Check(this.m_Tests.Where(x => x.EstimatedSize.ToString() == $"{x.EstimatedSize}"), regt.Where(x => x.EstimatedSize.ToString() == $"{x.EstimatedSize}"));
        }

        [TestMethod]
        public void Where_Index()
        {
            CheckEx.Check(this.m_Tests.Where((x, index) => index%2==0), regt.Where((x, index) => index % 2 == 0));
            CheckEx.Check(this.m_Tests.Where((x, index) => x.DisplayName == "AA"), regt.Where((x, index) => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.Where((x, index) => x.DisplayName == $"{x.DisplayName}"), regt.Where((x, index) => x.DisplayName == $"{x.DisplayName}"));
            CheckEx.Check(this.m_Tests.Where((x, index) => x.DisplayName.Contains("AA")), regt.Where((x, index) => x.DisplayName.Contains("AA")));
            CheckEx.Check(this.m_Tests.Where((x, index) => x.DisplayName.Contains("AA") == true), regt.Where((x, index) => x.DisplayName.Contains("AA") == true));
            CheckEx.Check(this.m_Tests.Where((x, index) => string.IsNullOrEmpty(x.DisplayName)), regt.Where((x, index) => string.IsNullOrEmpty(x.DisplayName)));
            CheckEx.Check(this.m_Tests.Where((x, index) => string.IsNullOrEmpty(x.DisplayName) == true), regt.Where((x, index) => string.IsNullOrEmpty(x.DisplayName) == true));
        }

        [TestMethod]
        public void Except()
        {
            //var tests = this.m_Tests.Take(2).Select(x=>new InstalledApp(x));
            //this.Check(this.m_Tests.Except(tests), regt.Except(tests));
            //this.Check(this.m_Tests.Except(tests, new InstallAppCompare()), regt.Except(tests, new InstallAppCompare()));
            var tests = this.m_Tests.Take(2);
            CheckEx.Check(this.m_Tests.Except(tests, new InstallAppCompare()), regt.Except(tests, new InstallAppCompare()));
        }

        [TestMethod]
        public void Intersect()
        {
            var tests = this.m_Tests.Take(2).Select(x => new InstalledApp(x));
            CheckEx.Check(this.m_Tests.Intersect(tests), regt.Intersect(tests));
            CheckEx.Check(this.m_Tests.Intersect(tests, new InstallAppCompare()), regt.Intersect(tests, new InstallAppCompare()));
        }

        [TestMethod]
        public void Union()
        {
            var tests = this.m_Tests.Take(2).Select(x => new InstalledApp(x));
            CheckEx.Check(this.m_Tests.Union(tests), regt.Union(tests));
            CheckEx.Check(this.m_Tests.Union(tests, new InstallAppCompare()), regt.Union(tests, new InstallAppCompare()));
        }

        [TestMethod]
        public void Join()
        {
            //var j1 = regt.Join(this.m_Apps, x => x.DisplayName, y => y.Name, (install, app) => install);
            //var j2 = this.m_Apps.Join(regt, x => x.Name, y => y.DisplayName, (test, install) => install);
            CheckEx.Check(this.m_Apps.Join(regt, x => x.Name, y => y.DisplayName, (test, install) => install), regt.Join(this.m_Apps, x => x.DisplayName, y => y.Name, (install, app) => install));
        }

        [TestMethod]
        public void Select_Min()
        {
            Assert.IsTrue(this.m_Tests.Select(x => x.DisplayName.Length).Min() == regt.Select(x => x.DisplayName.Length).Min(), "Select_Min fail");
        }

        [TestMethod]
        public void Select()
        {
            CheckEx.Check(this.m_Tests.Select(x => x), regt.Select(x => x));
            CheckEx.Check(this.m_Tests.Select(x => x.DisplayName), regt.Select(x => x.DisplayName));
            CheckEx.Check(this.m_Tests.Select(x => x.Version), regt.Select(x => x.Version));
            CheckEx.Check(this.m_Tests.Select(x => x.EstimatedSize), regt.Select(x => x.EstimatedSize));
            //this.Check(this.m_Tests.Select(x => x.IsOfficial), regt.Select(x => x.IsOfficial));
            CheckEx.Check(this.m_Tests.Select(x => new { Name = x.DisplayName }), regt.Select(x => new { Name = x.DisplayName }));
            CheckEx.Check(this.m_Tests.Select(x => new { Version = x.Version }), regt.Select(x => new { Version = x.Version }));
            CheckEx.Check(this.m_Tests.Select(x => new { Size = x.EstimatedSize }), regt.Select(x => new { Size = x.EstimatedSize }));
            //this.Check(this.m_Tests.Select(x => new { Official = x.IsOfficial }), regt.Select(x => new { Official = x.IsOfficial }));
            //this.Check(this.m_Tests.Select(x => $"IsOfficial{x.IsOfficial}"), regt.Select(x => $"IsOfficial{x.IsOfficial}"));
            //this.Check(this.m_Tests.Select(x => new AppData(x.DisplayName) { Ver = x.Version.ToString(), IsOfficial = (bool)x.IsOfficial }), regt.Select(x => new AppData(x.DisplayName) { IsOfficial = (bool)x.IsOfficial, Ver = x.Version.ToString() }));
        }

        [TestMethod]
        public void Select_Tuple()
        {
            CheckEx.Check(this.m_Tests.Select(x => Tuple.Create(x.DisplayName)), regt.Select(x => Tuple.Create(x.DisplayName)));
            //this.Check(this.m_Tests.Select(x => Tuple.Create(x.DisplayName, x.EstimatedSize)), regt.Select(x => Tuple.Create(x.DisplayName, x.EstimatedSize)));
        }

        [TestMethod]
        public void Select_Index()
        {
            //CheckEx.Check(this.m_Tests.Select((x, index) => x), regt.Select((x, index) => x));
            var aa = regt.Select((x, index) => new { x, index });
            CheckEx.Check(this.m_Tests.Select((x, index) => new { x, index }), regt.Select((x, index) => new { x, index }));
        }

        [TestMethod]
        public void Select_Index_Tuple()
        {
            CheckEx.Check(this.m_Tests.Select((x, index) => Tuple.Create(x.DisplayName)), regt.Select((x, index) => Tuple.Create(x.DisplayName)));
            CheckEx.Check(this.m_Tests.Select((x, index) => Tuple.Create(x.DisplayName, x.EstimatedSize)), regt.Select((x, index) => Tuple.Create(x.DisplayName, x.EstimatedSize)));
            //this.Check(this.m_Tests.Select((x, index) => Tuple.Create(x.DisplayName,index)), regt.Select((x, index) => Tuple.Create(x.DisplayName, index)));
            //this.Check(this.m_Tests.Select((x, index) => Tuple.Create(x.DisplayName, x.EstimatedSize, index)), regt.Select((x, index) => Tuple.Create(x.DisplayName, x.EstimatedSize, index)));
        }

        [TestMethod]
        public void Reverse()
        {
            List<InstalledApp> reverse = new List<InstalledApp>(this.m_Tests);
            reverse.Reverse();
            CheckEx.Check(reverse, regt.Reverse());
        }

        [TestMethod]
        public void OrderBy()
        {
            var orderby = regt.OrderBy(x => x.EstimatedSize);
            CheckEx.Check(this.m_Tests.OrderBy(x=>x.EstimatedSize), orderby);
            CheckEx.Check(this.m_Tests.OrderBy(x => x.DisplayName.Length), regt.OrderBy(x => x.DisplayName.Length));
        }

        [TestMethod]
        public void OrderBy_ThenBy()
        {
            var orderby = regt.OrderBy(x => x.EstimatedSize).ThenBy(x=>x.DisplayName.Length);
            CheckEx.Check(this.m_Tests.OrderBy(x => x.EstimatedSize).ThenBy(x=>x.DisplayName.Length), orderby);
        }

        [TestMethod]
        public void OrderByDescending()
        {
            CheckEx.Check(this.m_Tests.OrderByDescending(x => x.EstimatedSize), regt.OrderByDescending(x => x.EstimatedSize));
        }

        [TestMethod]
        public void OrderByDescending_ThenBy()
        {
            var aa = regt.OrderByDescending(x => x.EstimatedSize).ThenBy(x=>x.DisplayName.Length);
            
            CheckEx.Check(this.m_Tests.OrderByDescending(x => x.EstimatedSize).ThenBy(x=>x.DisplayName.Length)
                        , regt.OrderByDescending(x => x.EstimatedSize).ThenBy(x=>x.DisplayName.Length));
        }

        [TestMethod]
        public void Zip()
        {
            //var zip = regt.Zip(apps, (reg, app) => new { reg, app });
            //var zip = regt.Zip(apps, (reg, app) => reg);
            //var zip = regt.Zip(apps, (reg, app) => app);
            //var zip = regt.Zip(apps, (reg, app) => reg.DisplayName);
            //var zip = regt.Zip(this.m_Tests, (reg, app) => new { Name = app.DisplayName, reg.DisplayName });
            //var dd1 = regt.Zip(this.m_Apps, (reg, app) => reg);
            //var dd2 = this.m_Tests.Zip(regt, (app, reg) => reg);
            CheckEx.Check(regt.Zip(this.m_Tests, (reg, app) => reg), this.m_Tests.Zip(regt, (app, reg) => reg));
            CheckEx.Check(regt.Zip(this.m_Tests, (reg, app) => app), this.m_Tests.Zip(regt, (app, reg) => app));
            CheckEx.Check(regt.Zip(this.m_Tests, (reg, app) => app.DisplayName), this.m_Tests.Zip(regt, (app, reg) => app.DisplayName));
            CheckEx.Check(regt.Zip(this.m_Tests, (reg, app) => new { reg, app }), this.m_Tests.Zip(regt, (app, reg) => new { reg, app }));
            CheckEx.Check(regt.Zip(this.m_Apps, (reg, app) => reg), this.m_Apps.Zip(regt, (app, reg) => reg));
        }

        [TestMethod]
        public void ToList()
        {
            CheckEx.Check(this.m_Tests, regt.ToList());
            CheckEx.Check(this.m_Tests.Where(x=>x.DisplayName=="AA"), regt.Where(x => x.DisplayName == "AA").ToList());
            regt.ToArray();
        }

        [TestMethod]
        public void ToArray()
        {
            CheckEx.Check(this.m_Tests.ToArray(), regt.ToArray());
            CheckEx.Check(this.m_Tests.Where(x => x.DisplayName == "AA").ToArray(), regt.Where(x => x.DisplayName == "AA").ToArray());
        }

        [TestMethod]
        public void First()
        {
            var first1 = this.m_Tests.First();
            var first2 = regt.First();
            CheckEx.Check(this.m_Tests.First(), regt.First());
            CheckEx.Check(this.m_Tests.First(x=>x.DisplayName=="AA"), regt.First(x=>x.DisplayName=="AA"));
        }

        [TestMethod]
        public void Last()
        {
            CheckEx.Check(this.m_Tests.Last(), regt.Last());
            CheckEx.Check(this.m_Tests.Last(x => x.DisplayName == "AA"), regt.Last(x => x.DisplayName == "AA"));
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            CheckEx.Check(this.m_Tests.FirstOrDefault(), regt.FirstOrDefault());
            CheckEx.Check(this.m_Tests.FirstOrDefault(x => x.DisplayName == "AA"), regt.FirstOrDefault(x => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.FirstOrDefault(x => x.DisplayName.Contains("AA")), regt.FirstOrDefault(x => x.DisplayName.Contains("AA")));
        }

        [TestMethod]
        public void LastOrDefault()
        {
            CheckEx.Check(this.m_Tests.LastOrDefault(), regt.LastOrDefault());
            CheckEx.Check(this.m_Tests.LastOrDefault(x => x.DisplayName == "AA"), regt.LastOrDefault(x => x.DisplayName == "AA"));
            CheckEx.Check(this.m_Tests.LastOrDefault(x => x.DisplayName.Contains("AA")), regt.LastOrDefault(x => x.DisplayName.Contains("AA")));
            CheckEx.Check(this.m_Tests.LastOrDefault(x => x.DisplayName.Contains("AA") == true), regt.LastOrDefault(x => x.DisplayName.Contains("AA") == true));
        }

        [TestMethod]
        public void Any()
        {
            Assert.IsTrue(this.m_Tests.Any(x => x.DisplayName == "") == regt.Any(x => x.DisplayName == ""), "Any(x => x.DisplayName == ) fail");
        }

        [TestMethod]
        public void All()
        {

            Assert.IsTrue(this.m_Tests.All(x => x.DisplayName == "") == regt.All(x => x.DisplayName == ""), "All(x => x.DisplayName == ) fail");
            Assert.IsTrue(this.m_Tests.All(x => x.DisplayName == "" && x.EstimatedSize > 10) == regt.All(x => x.DisplayName == "" && x.EstimatedSize > 10), "All fail");
        }

        [TestMethod]
        public void Count()
        {
            Assert.IsTrue(this.m_Tests.Count() == regt.Count(), "Count fail");
            Assert.IsTrue(this.m_Tests.Count(x=>x.EstimatedSize>30) == regt.Count(x => x.EstimatedSize > 30), "Count fail");
            Assert.IsTrue(this.m_Tests.Count(x=>x.EstimatedSize>=40&&x.DisplayName=="") == regt.Count(x => x.EstimatedSize >= 40 && x.DisplayName == ""), "Count fail");
            Assert.IsTrue(this.m_Tests.Count(x=>x.Key.Length>=2) == regt.Count(x => x.Key.Length >= 2), "Count fail");
        }

        [TestMethod]
        public void LongCount()
        {
            Assert.IsTrue(this.m_Tests.LongCount() == regt.LongCount(), "LongCount fail");
            Assert.IsTrue(this.m_Tests.LongCount(x => x.EstimatedSize > 30) == regt.LongCount(x => x.EstimatedSize > 30), "LongCount fail");
            Assert.IsTrue(this.m_Tests.LongCount(x => x.EstimatedSize >= 40 && x.DisplayName == "") == regt.LongCount(x => x.EstimatedSize >= 40 && x.DisplayName == ""), "LongCount fail");
        }

        [TestMethod]
        public void ElementAt()
        {
            CheckEx.Check(this.m_Tests.ElementAt(0), regt.ElementAt(0));
            CheckEx.Check(this.m_Tests.ElementAt(1), regt.ElementAt(1));
            CheckEx.Check(this.m_Tests.ElementAt(2), regt.ElementAt(2));
        }

        [TestMethod]
        public void ElementAtOrDefault()
        {
            CheckEx.Check(this.m_Tests.ElementAtOrDefault(0), regt.ElementAtOrDefault(0));
            CheckEx.Check(this.m_Tests.ElementAtOrDefault(1), regt.ElementAtOrDefault(1));
            CheckEx.Check(this.m_Tests.ElementAtOrDefault(2), regt.ElementAtOrDefault(2));
            CheckEx.Check(this.m_Tests.ElementAtOrDefault(200), regt.ElementAtOrDefault(200));
            CheckEx.Check(this.m_Tests.ElementAtOrDefault(-1), regt.ElementAtOrDefault(-1));
        }

        [TestMethod]
        public void Max()
        {
            Assert.IsTrue(this.m_Tests.Max(x=>x.EstimatedSize) == regt.Max(x => x.EstimatedSize), "Max fail");
            //Assert.IsTrue(this.m_Tests.Max(x => x.DisplayName.Length) == regt.Max(x => x.DisplayName.Length), "Max fail");
        }

        [TestMethod]
        public void Min()
        {
            Assert.IsTrue(this.m_Tests.Min(x => x.EstimatedSize) == regt.Min(x => x.EstimatedSize), "Min fail");
        }

        [TestMethod]
        public void Sum()
        {
            Assert.IsTrue(this.m_Tests.Sum(x => x.EstimatedSize) == regt.Sum(x => x.EstimatedSize), "Sum fail");
        }

        [TestMethod]
        [TestCategory("Excute")]
        public void Average()
        {
            Assert.IsTrue(this.m_Tests.Average(x => x.EstimatedSize) == regt.Average(x => x.EstimatedSize), "Average fail");
            Assert.IsTrue(this.m_Tests.Average(x => x.DisplayName.Length) == regt.Average(x => x.DisplayName.Length), "Average fail");
            Assert.IsTrue(this.m_Tests.Average(x => x.Version.ToString().Length) == regt.Average(x => x.Version.ToString().Length), "Average fail");
        }

        [TestMethod]
        public void Update()
        {

            int update_count = regt.Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize + 100 });
            var count1 = regt;
            var count2 = this.m_Tests.Select(x => new InstalledApp(x) { EstimatedSize = x.EstimatedSize + 100 });
            CheckEx.Check(count1, count2);
            regt.Where(x => x.EstimatedSize > 130).Update(x => new InstalledApp() { EstimatedSize = x.EstimatedSize - 100 });
            var count3 = regt.Where(x => x.EstimatedSize > 130);
            count2 = this.m_Tests.Where(x => x.EstimatedSize > 130).Select(x => new InstalledApp(x) { EstimatedSize = x.EstimatedSize - 100 });
            CheckEx.Check(count3, count2);

            //update_count = regt.Update(x => new InstallApp() { DisplayName = $"{x.DisplayName}_{x.DisplayVersion}" });
            //count2 = this.m_Tests.Select(x => new InstallApp(x) { DisplayName = $"{x.DisplayName}_{x.DisplayVersion}" });
            //this.Check(count1, count2);
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
