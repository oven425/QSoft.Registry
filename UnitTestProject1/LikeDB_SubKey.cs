using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LikeDB
{
    [TestClass]
    public class LikeDB_SubKey
    {
        RegQuery<Computer> regt_computer = new RegQuery<Computer>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTestLikeDB_SubKey\Computers";
                x.View = RegistryView.Registry64;
            });

        RegQuery<Mapping> regt_mapping = new RegQuery<Mapping>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTestLikeDB_SubKey\Mappings";
                x.View = RegistryView.Registry64;
            });
        RegQuery<Address> regt_address = new RegQuery<Address>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTestLikeDB_SubKey\Addresss";
                x.View = RegistryView.Registry64;
            });

        [TestMethod]
        public void BuildMockup()
        {
            regt_address.RemoveAll();
            var addresss = Enumerable.Range(1, 10).Select(x=>new Address()
            {
                IP = $"127.0.0.{x}",
                Port=80+x
            });
            regt_address.Insert(addresss);
        }
    }

    public class Address
    {
        public string IP { set; get; }
        public int Port { set; get; }

    }

    public class NetworkCard
    {
        public Address Local { set; get; }
        public Address Remote { set; get; }
    }

    public class Computer
    {
        public string Name { set; get; }
        public NetworkCard Network { set; get; }
    }

    public class Mapping
    {
        public string ComputerName { set; get; }
        public Address Address { set; get; }
    }
}
