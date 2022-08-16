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
            })
            .useConverts(x=> 
            {
                x.Add(new Version2String());
            });

        List<Device> m_Devices;
        public LinqToRegistry_SubKey()
        {
            this.m_Devices = regt_devices.ToList();
        }
        [TestMethod]
        public void BuildMockup()
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
                        Account = $"root_local_account_{x}",
                        Password = $"root_local_password_{x}"
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
                    Port = 2000+x,
                    Root = new Address.Auth()
                    {
                        Account = "root_remote",
                        Password = "root_remote"
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
                    Name = $"Location_{x}",
                    Floor = new FloorData()
                    {
                        Name = $"Floor_{x}",
                        Area = new AreaData()
                        {
                            Name = $"Area_{x}",
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
            CheckEx.Check(this.m_Devices.Select(x => x.Key), this.regt_devices.Select(x => x.Key));
            CheckEx.Check(this.m_Devices.Select(x => new { Key = x.Key }), this.regt_devices.Select(x => new { Key = x.Key }));
            CheckEx.Check(this.m_Devices.Select(x => x.CameraSetting.Brightness), this.regt_devices.Select(x => x.CameraSetting.Brightness));
            CheckEx.Check(this.m_Devices.Select(x => x.CameraSetting.Brightness.CanEdit), this.regt_devices.Select(x => x.CameraSetting.Brightness.CanEdit));
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            CheckEx.Check(this.m_Devices.FirstOrDefault(x => x.Location.Floor.Area.Name == "Area_1"),
                            regt_devices.FirstOrDefault(x => x.Location.Floor.Area.Name == "Area_1"));
            CheckEx.Check(this.m_Devices.FirstOrDefault(x => x.Size.Width > 0 && x.Size.Height > 0),
                            regt_devices.FirstOrDefault(x => x.Size.Width > 0 && x.Size.Height > 0));
        }

        [TestMethod]
        public void Where()
        {
            CheckEx.Check(this.m_Devices.Where(x => x.Location.Floor.Area.Name == "Area_1"), regt_devices.Where(x => x.Location.Floor.Area.Name == "Area_1"));
        }
    }





}
