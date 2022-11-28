using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestData;

namespace General
{
    [TestClass]
    public class LikeNoSQL_List
    {
        RegQuery<Device> regt_devices = new RegQuery<Device>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"devices";
                x.View = RegistryView.Registry64;
            });
            //.useConverts(x =>
            //{
            //    x.Add(new Version2String());
            //});

        List<Device> m_Devices;
        public LikeNoSQL_List()
        {
            this.m_Devices = regt_devices.ToList();
        }

        [TestMethod]
        public void BuildMockup()
        {

        }

        [TestMethod]
        public void Any()
        {
            Assert.IsTrue(this.m_Devices.Any(x => x.Local.Root.Account == "") == regt_devices.Any(x => x.Local.Root.Account == ""), ".Any(x => x.Local.Root.Account == \"\") fail");
        }
    }
}
