using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using LikeNoSQL;
using UnitTest;

namespace General
{
    [TestClass]
    public class LinqToRegistry_SubKey
    {
        //RegQuery<Company> regt_company = new RegQuery<Company>()
        //    .useSetting(x =>
        //    {
        //        x.Hive = RegistryHive.CurrentConfig;
        //        x.SubKey = @"UnitTest\Company";
        //        x.View = RegistryView.Registry64;
        //    });

        RegQuery<Device> regt_devices = new RegQuery<Device>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"devices";
                x.View = RegistryView.Registry64;
            });

        List<Device> m_Devices;
        public LinqToRegistry_SubKey()
        {
            this.m_Devices = regt_devices.ToList();
        }
        [TestMethod]
        public void CreateDB()
        {
            var devices = Enumerable.Range(1, 10).Select(x => new Device()
            {
                Key = $"{x}",
                Name = $"{x}F_AA",
                Size = new Size() { Width = x+100, Height = x+200 },
                Local = new Address()
                {
                    IP = $"127.0.0.{x}",
                    Port = 1000+(x>5?1:0),
                    Root = new Address.Auth()
                    {
                        Account = "root_local",
                        Password = "root_local"
                    },
                    Guest = new Address.Auth()
                    {
                        Account = "guest_local",
                        Password = "guest_local"
                    }
                },
                Remote = new Address()
                {
                    IP = $"192.168.10.{x}",
                    Port = 1001,
                    Root = new Address.Auth()
                    {
                        Account = "root_local",
                        Password = "root_local"
                    },
                    Guest = new Address.Auth()
                    {
                        Account = "guest_local",
                        Password = "guest_local"
                    }
                },
                CameraSetting = new CameraSetting()
                {
                    PIR = new PIR() { IsEnable = true, IsAuto = true },
                    WDR = new WDR() { IsEnable = true },
                    Brightness = new Brightness()
                    {
                        Range = new Range() { Min = x, Max = x+1000 },
                        Current = x+500,
                        CanEdit = true
                    }
                },
                Location = new Locationata()
                {
                    Name = "DD",
                    Floor = new FloorData()
                    {
                        Name = $"{x}F",
                        Area = new AreaData()
                        {
                            Name = "aaa",
                            Data = new Rect()
                            {
                                Point = new Point()
                                {
                                    X = x,
                                    Y = x*2
                                },
                                Size = new Size()
                                {
                                    Width = x+x,
                                    Height = (x+x)*2
                                }
                            }
                        }
                    }
                }
            });
            this.m_Devices = new List<Device>(devices);
            regt_devices.RemoveAll();
            regt_devices.Insert(devices);

        }

        [TestMethod]
        public void Select()
        {
            var reg = this.regt_devices.Select(x => x.Key);
            var org = this.m_Devices.Select(x => x.Key);
            CheckEx.Check(reg, org);
            var reg1 = this.regt_devices.Select(x => new { Key = x.Key });
            var org1 = this.m_Devices.Select(x => new { Key = x.Key });
            CheckEx.Check(reg1, org1);

        }

        [TestMethod]
        public void Select1()
        {
            var reg = this.regt_devices.Select((x,i) => x.Key);
            var org = this.m_Devices.Select((x, i) => x.Key);
            CheckEx.Check(reg, org);
            var reg1 = this.regt_devices.Select((x, i) => new {index = i, Key = x.Key });
            var org1 = this.m_Devices.Select((x, i) => new {index=i, Key = x.Key });
            CheckEx.Check(reg1, org1);
        }

    }





}
