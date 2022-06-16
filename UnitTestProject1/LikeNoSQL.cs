﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using QSoft.Registry.Linq;
using System.Linq;

namespace LikeNoSQL
{
    [TestClass]
    public class LikeNoSQL
    {
        RegQuery<Company> regt_company = new RegQuery<Company>()
            .useSetting(x =>
            {
                x.Hive = RegistryHive.CurrentConfig;
                x.SubKey = @"UnitTest\Company";
                x.View = RegistryView.Registry64;
            });

        [TestMethod]
        public void CreateDB()
        {
        }

        bool Check<T>(T src, T dst)
        {
            bool result = true;
            var pps = typeof(T).GetProperties().Where(x => x.CanRead == true);
            foreach (var pp in pps)
            {
                bool ch = false;
                var typecode = Type.GetTypeCode(pp.PropertyType);
                if (typecode == TypeCode.Object)
                {
                    if (pp.PropertyType == typeof(Version))
                    {

                    }
                    else
                    {
                        ch = true;
                        dynamic s = pp.GetValue(src);
                        dynamic d = pp.GetValue(dst);
                        if (s == null && d == null)
                        {

                        }
                        else
                        {
                            Check(s, d);
                        }
                    }
                }
                if (ch == false)
                {
                    dynamic s = pp.GetValue(src);
                    dynamic d = pp.GetValue(dst);
                    Assert.AreEqual(s, d, $"{s}!={d}");
                }

            }

            return result;
        }

        bool Check<T>(IEnumerable<T> src, IEnumerable<T> dst)
        {
            bool result = true;
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()}!=dst:{dst.Count()}");
                return false;
            }
            var zip = src.Zip(dst, (x, y) => new { src = x, dst = y });
            foreach (var oo in zip)
            {
                Check(oo.src, oo.dst);
            }

            return result;
        }

    }

    public class People
    {
        public string Name { set; get; }
    }

    public class App
    {
        public int ID { set; get; }
        [RegPropertyName(Name = "Name")]
        public string DisplayName { set; get; }
        public Version Version { set; get; }
        [RegIgnore]
        public int Size { set; get; }
    }

    public class Company
    {
        [RegSubKeyName]
        public int Key { set; get; }
        [RegPropertyName(Name = "Name1")]
        public string Name { set; get; }
        public string Address { set; get; }
        public Employee A { set; get; }
        public Employee B { set; get; }
        public Employee C { set; get; }
    }

    public class Employee
    {
        public string Name { set; get; }
        public int Age { set; get; }
        public App Word { set; get; }
        public App Excel { set; get; }
        public App PPt { set; get; }
        public App VSCode { set; get; }
    }

}
