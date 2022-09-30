using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest;

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
            var computers = Enumerable.Range(1, 5).Select(x => new Computer()
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
            var join1 = regt_computer.Join(regt_networkcards, x => x.Network_MAC, y => y.MAC, (x, y) => new Computer()
            {
                Name = x.Name,
                Network = y
            });
            var computers = regt_computer.ToList();
            var networkcads = regt_networkcards.ToList();
            var join2 = computers.Join(networkcads, x => x.Network_MAC, y => y.MAC, (x, y) => new Computer()
            {
                Name = x.Name,
                Network = y
            });
            CheckEx.Check(join1, join2);
        }

        [TestMethod]
        public void Join2()
        {
            var join1 = regt_computer.Join(regt_networkcards, x => x.Network_MAC, y => y.MAC, (x, y) => new
            {
                x.Name,
                x.Network
            });
            var computers = regt_computer.ToList();
            var networkcads = regt_networkcards.ToList();
            var join2 = computers.Join(networkcads, x => x.Network_MAC, y => y.MAC, (x, y) => new
            {
                x.Name,
                x.Network
            });
            CheckEx.Check(join1, join2);
        }

        [TestMethod]
        public void LeftJoin_2Table()
        {
            var join1 = regt_computer.GroupJoin(regt_networkcards, x => x.Network_MAC, y => y.MAC, (computes, nets) => new
            {
                computes,
                nets
            }).SelectMany(x=>x.nets.DefaultIfEmpty(), (x,y)=>new {x.computes, x.nets});
            var computers = regt_computer.ToList();
            var networkcads = regt_networkcards.ToList();
            var join2 = computers.GroupJoin(networkcads, x => x.Network_MAC, y => y.MAC, (computes, nets) => new
            {
                computes,
                nets
            }).SelectMany(x => x.nets.DefaultIfEmpty(), (x, y) => new { x.computes, x.nets });
            CheckEx.Check(join1, join2);
        }

        [TestMethod]
        public void LeftJoin_3Table()
        {
            var join1 = regt_computer.GroupJoin(regt_networkcards, x => x.Network_MAC, y => y.MAC, (computes, nets) => new
            {
                computes,
                nets
            }).SelectMany(x => x.nets.DefaultIfEmpty(), (x, y) => new { x.computes, x.nets });
            var computers = regt_computer.ToList();
            var networkcads = regt_networkcards.ToList();
            var mapping = regt_mapping.ToList();
            //var join2 = computers.GroupJoin(networkcads, x => x.Network_MAC, y => y.MAC, (computes, nets) => new
            //{
            //    computes,
            //    nets
            //}).SelectMany(x => x.nets.DefaultIfEmpty(), (x, y) => new { x.computes, x.nets });
            //CheckEx.Check(join1, join2);
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
