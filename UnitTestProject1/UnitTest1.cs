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
    }

    [TestClass]
    public class LinqToRegistry
    {
        List<InstallApp> m_Tests = new List<InstallApp>();
        public LinqToRegistry()
        {
            this.m_Tests.Add(new InstallApp() { DisplayName = "AA", DisplayVersion = new Version("1.1.1.1"), EstimatedSize = 10, Index = 0 });
            this.m_Tests.Add(new InstallApp() { DisplayName = "BB", DisplayVersion = new Version("2.2.2.2"), EstimatedSize = 20, Index = 1 });
            this.m_Tests.Add(new InstallApp() { DisplayName = "CC", DisplayVersion = new Version("3.3.3.3"), EstimatedSize = 30, Index = 2 });
            this.m_Tests.Add(new InstallApp() { DisplayName = "DD", DisplayVersion = new Version("4.4.4.4"), EstimatedSize = 40, Index = 3 });
            this.m_Tests.Add(new InstallApp() { DisplayName = "EE", DisplayVersion = new Version("5.5.5.5"), EstimatedSize = 50, Index = 4 });
            this.m_Tests.Add(new InstallApp() { DisplayName = "FF", DisplayVersion = new Version("6.6.6.6"), EstimatedSize = 60, Index = 5 });
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
        public void Where()
        {
            this.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA")), regt.Where(x => x.DisplayName.Contains("AA")));
            this.Check(this.m_Tests.Where(x => x.DisplayName.Contains("AA")==true), regt.Where(x => x.DisplayName.Contains("AA")==true));
            this.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName)), regt.Where(x => string.IsNullOrEmpty(x.DisplayName)));
            this.Check(this.m_Tests.Where(x => string.IsNullOrEmpty(x.DisplayName)==true), regt.Where(x => string.IsNullOrEmpty(x.DisplayName)==true));
        }

        [TestMethod]
        public void Select()
        {
            var select = regt.Select(x=>x);
            this.Check(this.m_Tests, select);
        }



        [TestMethod]
        public void First()
        {
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
        public void Max()
        {
            Assert.IsTrue(this.m_Tests.Max(x=>x.EstimatedSize) == regt.Max(x => x.EstimatedSize), "Max fail");
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


        void Check(IEnumerable<InstallApp> src, IEnumerable<InstallApp> dst)
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

        void Check<T>(T src, T dst)
        {
            if(src==null && dst==null)
            {

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
}
