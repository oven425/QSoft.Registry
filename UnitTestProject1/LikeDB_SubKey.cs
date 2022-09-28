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
        RegQuery<NetworkCard> regt_networkcards = new RegQuery<NetworkCard>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTestLikeDB_SubKey\NetworkCards";
                x.View = RegistryView.Registry64;
            });
        RegQuery<Mapping> regt_mapping = new RegQuery<Mapping>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTestLikeDB_SubKey\Mappings";
                x.View = RegistryView.Registry64;
            });

        [TestMethod]
        public void BuildMockup()
        {
            var computers = Enumerable.Range(1, 10).Select(x => new Computer()
            {
                Name = $"Computer_{x}",
                Network_MAC = $"{x}.{x}{x},{x}",
                Network = new NetworkCard()
                {
                    MAC = $"{x}.{x}{x},{x}",
                    Local = new Address()
                    {
                        IP = $"127.0.0.{x}",
                        Port = 800 + x
                    },
                    Remote = new Address()
                    {
                        IP = $"192.168.0.{x}",
                        Port = 900 + x
                    }
                }
            });
            regt_networkcards.RemoveAll();
            regt_networkcards.Insert(computers.Select(x => x.Network));

            regt_computer.RemoveAll();
            regt_computer.Insert(computers);

            regt_mapping.RemoveAll();
            regt_mapping.Insert(computers.Select(x => new Mapping()
            {
                ComputerName = x.Name,
                MAC = x.Network_MAC
            }));
        }

        [TestMethod]
        public void Join1()
        {
            var join = regt_computer.Join(regt_networkcards, x => x.Network_MAC, y => y.MAC, (x, y) => new Computer()
            {
                Name = x.Name
            });
        }
    }

    public class Address
    {
        public string IP { set; get; }
        public int Port { set; get; }
    }

    public class NetworkCard
    {
        [RegSubKeyName]
        public string MAC { set; get; }
        public Address Local { set; get; }
        public Address Remote { set; get; }
    }

    public class Computer
    {
        [RegSubKeyName]
        public string Name { set; get; }
        [RegIgnore]
        public NetworkCard Network { set; get; }
        public string Network_MAC { set; get; }
    }

    public class Mapping
    {
        public string ComputerName { set; get; }
        public string MAC { set; get; }
    }
}
