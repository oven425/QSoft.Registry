using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;

namespace UnitTestProject1
{
    public class InstallApp
    {
        public int? Index { set; get; }
        public string DisplayName { set; get; }
        public Version DisplayVersion { set; get; }
        public int? EstimatedSize { set; get; }
        public bool? IsOfficial { set; get; }

        public InstallApp() { }

        public InstallApp(InstallApp data)
        {
            var pps = typeof(InstallApp).GetProperties().Where(x => x.CanRead == true && x.CanWrite == true);
            foreach(var pp in pps)
            {
                pp.SetValue(this, pp.GetValue(data));
            }
        }
    }

    [TestClass]
    public class LinqToRegistry
    {
        List<InstallApp> m_Tests = new List<InstallApp>();
        List<AppData> m_Apps = new List<AppData>();
        public LinqToRegistry()
        {
            this.m_Tests = this.InstallApp_org();

            m_Apps.Add(new AppData() { Name = "A", IsOfficial = true });
            m_Apps.Add(new AppData() { Name = "AA", IsOfficial = false });
        }

        List<InstallApp> InstallApp_org()
        {
            List<InstallApp> datas = new List<InstallApp>();
            datas.Add(new InstallApp() { DisplayName = "AA", DisplayVersion = new Version("1.1.1.1"), EstimatedSize = 10, IsOfficial = true, Index = 0 });
            datas.Add(new InstallApp() { DisplayName = "BB", DisplayVersion = new Version("2.2.2.2"), EstimatedSize = 20, IsOfficial = false, Index = 1 });
            datas.Add(new InstallApp() { DisplayName = "CC", DisplayVersion = new Version("3.3.3.3"), EstimatedSize = 30, IsOfficial = true, Index = 2 });
            datas.Add(new InstallApp() { DisplayName = "DD", DisplayVersion = new Version("4.4.4.4"), EstimatedSize = 40, IsOfficial = false, Index = 3 });
            datas.Add(new InstallApp() { DisplayName = "EE", DisplayVersion = new Version("5.5.5.5"), EstimatedSize = 50, IsOfficial = true, Index = 4 });
            datas.Add(new InstallApp() { DisplayName = "FF", DisplayVersion = new Version("6.6.6.6"), EstimatedSize = 60, IsOfficial = false, Index = 5 });

            return datas;
        }

        IQueryable<InstallApp> regt = new RegQuery<InstallApp>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.LocalMachine;
                x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
            });
        [TestCategory("Init")]
        [TestMethod]
        public void BuildMockup()
        {
            RegistryKey regbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var reg = regbase.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);
            try
            {
                reg.DeleteSubKeyTree("1A");
            }
            catch(Exception ee)
            {
                System.Diagnostics.Trace.WriteLine(ee.Message);
                System.Diagnostics.Trace.WriteLine(ee.StackTrace);
            }


            var propertys = typeof(InstallApp).GetProperties().Where(x => x.CanRead == true);

            var test1A = reg.CreateSubKey(@"1A", true);
            foreach(var oo in this.m_Tests)
            {
                var regd = test1A.CreateSubKey(oo.DisplayName, true);
                foreach(var pp in propertys)
                {
                    var doo = pp.GetValue(oo);
                    if(doo!=null)
                    {
                        regd.SetValue(pp.Name, doo);
                    }
                }
                regd.Close();
                regd.Dispose();
            }

        }

        [TestMethod]
        public void GroupBy()
        {
            this.Check(this.m_Tests.GroupBy(x => x.DisplayName), regt.GroupBy(x => x.DisplayName));
            this.Check(this.m_Tests.GroupBy(x => x.DisplayVersion), regt.GroupBy(x => x.DisplayVersion));
            this.Check(this.m_Tests.GroupBy(x => x.IsOfficial), regt.GroupBy(x => x.IsOfficial));
            this.Check(this.m_Tests.GroupBy(x => new { x.IsOfficial, x.DisplayName }), regt.GroupBy(x => new { x.IsOfficial, x.DisplayName }));
            this.Check(this.m_Tests.GroupBy(x => x), regt.GroupBy(x => x));
            this.Check(this.m_Tests.GroupBy(x => x.DisplayName, x => x.DisplayName), regt.GroupBy(x => x.DisplayName, x => x.DisplayName));
            this.Check(this.m_Tests.GroupBy(x=>x), regt.GroupBy(x => x));
            this.Check(this.m_Tests.GroupBy(x => x).Select(x=>x), regt.GroupBy(x => x).Select(x => x));
            this.Check(this.m_Tests.GroupBy(x => x).Select(x => x.Key), regt.GroupBy(x => x).Select(x => x.Key));
            this.Check(this.m_Tests.GroupBy(x => x.DisplayName).Select(x => x.Key), regt.GroupBy(x => x.DisplayName).Select(x => x.Key));
        }        

        void Check<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> src, IEnumerable<IGrouping<TKey, TElement>> dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }

            for (int i = 0; i < src.Count(); i++)
            {
                dynamic key_src = src.ElementAt(i).Key;
                dynamic key_dst = dst.ElementAt(i).Key;
                Check(key_src, key_dst);
                
                int count_src = src.ElementAt(i).Count();
                int count_dst = dst.ElementAt(i).Count();
                Assert.IsTrue(count_src == count_dst, $"Count fail src:{count_src} dst:{count_dst}");
                for (int j = 0; j < count_src; j++)
                {
                    this.Check(src.ElementAt(i).ElementAt(j), dst.ElementAt(i).ElementAt(j));
                }
            }
        }

        [TestMethod]
        public void Take()
        {
            this.Check(this.m_Tests.Take(3), regt.Take(3));
            this.Check(this.m_Tests.Take(100), regt.Take(100));
        }

        [TestMethod]
        public void TakeWhile()
        {
            this.Check(this.m_Tests.TakeWhile(x=>x.DisplayName=="AA"), regt.TakeWhile(x => x.DisplayName == "AA"));
        }

        [TestMethod]
        public void TakeWhile_Index()
        {
            this.Check(this.m_Tests.TakeWhile((x,index) => x.DisplayName == "AA"), regt.TakeWhile((x,index) => x.DisplayName == "AA"));
            this.Check(this.m_Tests.TakeWhile((x, index) => index==0), regt.TakeWhile((x, index) => index == 0));
        }

        [TestMethod]
        public void Skip()
        {
            this.Check(this.m_Tests.Skip(3), regt.Skip(3));
        }

        [TestMethod]
        public void SkipWhile()
        {
            this.Check(this.m_Tests.SkipWhile(x=>x.EstimatedSize>20), regt.SkipWhile(x => x.EstimatedSize > 20));
        }

        [TestMethod]
        public void SkipWhile_Index()
        {
            this.Check(this.m_Tests.SkipWhile((x,index) => x.EstimatedSize > 20), regt.SkipWhile((x,index) => x.EstimatedSize > 20));
        }

        [TestMethod]
        public void Where()
        {
            this.Check(this.m_Tests.Where(x => x.DisplayName== "AA"), regt.Where(x => x.DisplayName == "AA"));
            this.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA")), regt.Where(x => x.DisplayName.Contains("AA")));
            this.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA")==true), regt.Where(x => x.DisplayName.Contains("AA")==true));
            this.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName)), regt.Where(x => string.IsNullOrEmpty(x.DisplayName)));
            this.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName)==true), regt.Where(x => string.IsNullOrEmpty(x.DisplayName)==true));
        }

        [TestMethod]
        public void Where_Index()
        {
            this.Check(this.m_Tests.Where((x, index) => index%2==0), regt.Where((x, index) => index % 2 == 0));
            this.Check(this.m_Tests.Where((x, index) => x.DisplayName == "AA"), regt.Where((x, index) => x.DisplayName == "AA"));
            this.Check(this.m_Tests.Where((x, index) => x.DisplayName.Contains("AA")), regt.Where((x, index) => x.DisplayName.Contains("AA")));
            this.Check(this.m_Tests.Where((x, index) => x.DisplayName.Contains("AA") == true), regt.Where((x, index) => x.DisplayName.Contains("AA") == true));
            this.Check(this.m_Tests.Where((x, index) => string.IsNullOrEmpty(x.DisplayName)), regt.Where((x, index) => string.IsNullOrEmpty(x.DisplayName)));
            this.Check(this.m_Tests.Where((x, index) => string.IsNullOrEmpty(x.DisplayName) == true), regt.Where((x, index) => string.IsNullOrEmpty(x.DisplayName) == true));
        }

        [TestMethod]
        public void Join()
        {
            var j1 = regt.Join(this.m_Apps, x => x.DisplayName, y => y.Name, (install, app) => install);
            var j2 = this.m_Apps.Join(regt, x => x.Name, y => y.DisplayName, (test, install) => install);
            this.Check(this.m_Apps.Join(regt, x => x.Name, y => y.DisplayName, (test, install) => install), regt.Join(this.m_Apps, x => x.DisplayName, y => y.Name, (install, app) => install));
        }

        //[TestMethod]
        //public void GroupJoin()
        //{
        //    //this.Check(this.m_Apps.GroupJoin(regt, x => x.Name, y => y.DisplayName, (test, install) => install), regt.GroupJoin(this.m_Apps, x => x.DisplayName, y => y.Name, (install, app) => install));
        //}

        [TestMethod]
        public void Select()
        {
            this.Check(this.m_Tests.Select(x => x), regt.Select(x => x));
            this.Check(this.m_Tests.Select(x => x.DisplayName), regt.Select(x => x.DisplayName));
            this.Check(this.m_Tests.Select(x => x.DisplayVersion), regt.Select(x => x.DisplayVersion));
            this.Check(this.m_Tests.Select(x => x.EstimatedSize), regt.Select(x => x.EstimatedSize));
            this.Check(this.m_Tests.Select(x => x.IsOfficial), regt.Select(x => x.IsOfficial));
            this.Check(this.m_Tests.Select(x => new { Name = x.DisplayName }), regt.Select(x => new { Name = x.DisplayName }));
            this.Check(this.m_Tests.Select(x => new { Version = x.DisplayVersion }), regt.Select(x => new { Version = x.DisplayVersion }));
            this.Check(this.m_Tests.Select(x => new { Size = x.EstimatedSize }), regt.Select(x => new { Size = x.EstimatedSize }));
            this.Check(this.m_Tests.Select(x => new { Official = x.IsOfficial }), regt.Select(x => new { Official = x.IsOfficial }));
        }

        [TestMethod]
        public void Select_Index()
        {
            var select = regt.Select((x,index) => x);
            foreach(var oo in select)
            {

            }
            this.Check(this.m_Tests.Select((x, index) => x), regt.Select((x, index) => x));
            //this.Check(this.m_Tests.Select((x, index) => new { x, index }), regt.Select((x, index) => new { x, index }));
        }

        [TestMethod]
        public void Reverse()
        {
            List<InstallApp> reverse = new List<InstallApp>(this.m_Tests);
            reverse.Reverse();
            this.Check(reverse, regt.Reverse());
        }

        [TestMethod]
        public void OrderBy()
        {
            var orderby = regt.OrderBy(x => x.EstimatedSize);
            this.Check(this.m_Tests.OrderBy(x=>x.EstimatedSize), orderby);
        }

        [TestMethod]
        public void OrderByDescending()
        {
            this.Check(this.m_Tests.OrderByDescending(x => x.EstimatedSize), regt.OrderByDescending(x => x.EstimatedSize));
        }

        [TestMethod]
        public void Zip()
        {
            //var zip = regt.Zip(apps, (reg, app) => new { reg, app });
            //var zip = regt.Zip(apps, (reg, app) => reg);
            //var zip = regt.Zip(apps, (reg, app) => app);
            //var zip = regt.Zip(apps, (reg, app) => reg.DisplayName);
            //var zip = regt.Zip(this.m_Tests, (reg, app) => new { Name=app.DisplayName, reg.DisplayName });
            //var dd1 = regt.Zip(this.m_Apps, (reg, app) => reg);
            //var dd2 = this.m_Tests.Zip(regt, (app, reg) => reg);
            this.Check(regt.Zip(this.m_Tests, (reg, app) => reg), this.m_Tests.Zip(regt, (app, reg) => reg));
            //this.Check(regt.Zip(this.m_Tests, (reg, app) => app), this.m_Tests.Zip(regt, (app, reg) => app));
            //this.Check(regt.Zip(this.m_Apps, (reg, app) => reg), this.m_Apps.Zip(regt, (app, reg) => reg));
        }

        [TestMethod]
        public void ToList()
        {
            this.Check(this.m_Tests, regt.ToList());
            this.Check(this.m_Tests.Where(x=>x.DisplayName=="AA"), regt.Where(x => x.DisplayName == "AA").ToList());
            regt.ToArray();
        }

        [TestMethod]
        public void ToArray()
        {
            this.Check(this.m_Tests.ToArray(), regt.ToArray());
            this.Check(this.m_Tests.Where(x => x.DisplayName == "AA").ToArray(), regt.Where(x => x.DisplayName == "AA").ToArray());
        }

        [TestMethod]
        public void First()
        {
            var first1 = this.m_Tests.First();
            var first2 = regt.First();
            this.Check(this.m_Tests.First(), regt.First());
            this.Check(this.m_Tests.First(x=>x.DisplayName=="AA"), regt.First(x=>x.DisplayName=="AA"));
        }

        [TestMethod]
        public void Last()
        {
            this.Check(this.m_Tests.Last(), regt.Last());
            this.Check(this.m_Tests.Last(x => x.DisplayName == "AA"), regt.Last(x => x.DisplayName == "AA"));
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            this.Check(this.m_Tests.FirstOrDefault(), regt.FirstOrDefault());
            this.Check(this.m_Tests.FirstOrDefault(x => x.DisplayName == "AA"), regt.FirstOrDefault(x => x.DisplayName == "AA"));
            this.Check(this.m_Tests.FirstOrDefault(x => x.DisplayName.Contains("AA")), regt.FirstOrDefault(x => x.DisplayName.Contains("AA")));
        }

        [TestMethod]
        public void LastOrDefault()
        {
            this.Check(this.m_Tests.LastOrDefault(), regt.LastOrDefault());
            this.Check(this.m_Tests.LastOrDefault(x => x.DisplayName == "AA"), regt.LastOrDefault(x => x.DisplayName == "AA"));
            this.Check(this.m_Tests.LastOrDefault(x => x.DisplayName.Contains("AA")), regt.LastOrDefault(x => x.DisplayName.Contains("AA")));
            this.Check(this.m_Tests.LastOrDefault(x => x.DisplayName.Contains("AA") == true), regt.LastOrDefault(x => x.DisplayName.Contains("AA") == true));
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
        }

        [TestMethod]
        public void Count()
        {
            Assert.IsTrue(this.m_Tests.Count() == regt.Count(), "Count fail");
            Assert.IsTrue(this.m_Tests.Count(x=>x.EstimatedSize>30) == regt.Count(x => x.EstimatedSize > 30), "Count fail");
            Assert.IsTrue(this.m_Tests.Count(x=>x.EstimatedSize>=40&&x.DisplayName=="") == regt.Count(x => x.EstimatedSize >= 40 && x.DisplayName == ""), "Count fail");
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
            this.Check(this.m_Tests.ElementAt(0), regt.ElementAt(0));
            this.Check(this.m_Tests.ElementAt(1), regt.ElementAt(1));
            this.Check(this.m_Tests.ElementAt(2), regt.ElementAt(2));
        }

        [TestMethod]
        public void ElementAtOrDefault()
        {
            this.Check(this.m_Tests.ElementAtOrDefault(0), regt.ElementAtOrDefault(0));
            this.Check(this.m_Tests.ElementAtOrDefault(1), regt.ElementAtOrDefault(1));
            this.Check(this.m_Tests.ElementAtOrDefault(2), regt.ElementAtOrDefault(2));
            this.Check(this.m_Tests.ElementAtOrDefault(200), regt.ElementAtOrDefault(200));
            this.Check(this.m_Tests.ElementAtOrDefault(-1), regt.ElementAtOrDefault(-1));
        }

        [TestMethod]
        public void Max()
        {
            Assert.IsTrue(this.m_Tests.Max(x=>x.EstimatedSize) == regt.Max(x => x.EstimatedSize), "Max fail");
            Assert.IsTrue(this.m_Tests.Max(x => x.DisplayName.Length) == regt.Max(x => x.DisplayName.Length), "Max fail");
        }

        [TestMethod]
        public void Min()
        {
            Assert.IsTrue(this.m_Tests.Min(x => x.EstimatedSize) == regt.Min(x => x.EstimatedSize), "Min fail");
            Assert.IsTrue(this.m_Tests.Select(x=>x.DisplayName.Length).Min() == regt.Select(x=>x.DisplayName.Length).Min(), "Min fail");
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
        }

        [TestMethod]
        public void Update()
        {
            int update_count = regt.Update(x => new InstallApp() { EstimatedSize = x.EstimatedSize + 100 });
            var count1 = regt;
            var count2 = this.m_Tests.Select(x => new InstallApp(x) { EstimatedSize = x.EstimatedSize + 100 });
            this.Check(count1, count2);

        }


        void Check<T>(IEnumerable<T> src, IEnumerable<T> dst)
        {
            if(src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for(int i=0; i<src.Count(); i++)
            {
                this.Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        void Check<T>(T[] src, T[] dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for (int i = 0; i < src.Count(); i++)
            {
                this.Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        void Check<T>(List<T> src, List<T> dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for (int i = 0; i < src.Count(); i++)
            {
                this.Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        void Check<T>(T src, T dst)
        {
            var typecode = Type.GetTypeCode(typeof(T));
            if(src==null && dst==null)
            {

            }
            else if(typecode == TypeCode.String)
            {
                string str_src = src as string;
                string str_dst = dst as string;
                if (str_src != str_dst)
                {
                    Assert.Fail($"fail src:{str_src} dst:{str_dst}");
                }
            }
            else
            {
                var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
                foreach(var pp in pps)
                {
                    dynamic s = pp.GetValue(src);
                    dynamic d = pp.GetValue(dst);
                    if(s != d)
                    {
                        Assert.Fail($"{pp.Name} fail src:{s} dst:{d}");
                    }
                }
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
}
