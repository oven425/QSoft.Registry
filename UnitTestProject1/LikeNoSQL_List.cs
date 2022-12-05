﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class LinqToRegistry_SubKey_List
    {
        RegQuery<Building> regt_building = new RegQuery<Building>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\devices";
                x.View = RegistryView.Registry64;
            });
            //.useConverts(x =>
            //{
            //    x.Add(new Version2String());
            //});

        List<Building> m_Buildings;
        public LinqToRegistry_SubKey_List()
        {
           // this.m_Buildings = regt_building.ToList();
        }

        [TestMethod]
        public void BuildMockup()
        {
            var buildings = Enumerable.Range(1, 10).Select(building => new Building()
            {
                Name = $"Building_{building}",
                Floors = Enumerable.Range(1, building).Select(floor => new FloorData()
                {
                    Level = floor,
                    Name = $"Floor_{building}_{floor}",
                    Areas = Enumerable.Range(1, floor).Select(area => new AreaData()
                    {
                        Name = $"Area_{building}_{floor}_{area}",
                        Data = new Rect() 
                        {
                            Point = new Point() 
                            {
                                X=area, 
                                Y=area 
                            },
                            Size = new Size()
                            {
                                Width = area, 
                                Height = area
                            }
                        },
                        Devices = Enumerable.Range(1, area).Select(device=>new Device()
                        {
                            Name = $"Device_{building}_{floor}_{area}_{device}",
                            Size = new Size()
                            {
                                Width = area,
                                Height = area
                            }

                        }).ToList()
                    }).ToList()
                }).ToList()
            });
            regt_building.RemoveAll();
            regt_building.Insert(buildings);
        }

        [TestMethod]
        public void Any()
        {
            var devices = regt_building.Select(x => x.Floors.Select(y => y.Areas.Select(z => z.Devices)));
        }
    }
}
