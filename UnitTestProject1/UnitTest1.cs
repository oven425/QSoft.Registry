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
        public string DisplayName { set; get; }
        public string DisplayVersion { set; get; }
        
    }

    

    [TestClass]
    public class LinqToRegistry
    {
        List<InstallApp> m_Test = new List<InstallApp>();
        IQueryable<InstallApp> regt = new RegQuery<InstallApp>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.LocalMachine;
                x.SubKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\1A";
            });
        [TestCategory("Init")]
        [TestMethod]
        public void BuildMock()
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

            
            

            var test1A = reg.CreateSubKey(@"1A", true);
            for(int i=0; i<10; i++)
            {
                var datareg = test1A.CreateSubKey($"Test{i}");
                datareg.SetValue("DisplayName", "AA");
            }
            //test.c

        }


        [TestMethod]
        public void Select()
        {
            var select = regt.Select(x => x);
            
        }

        [TestMethod]
        public void First()
        {
        }

        


    }
}
