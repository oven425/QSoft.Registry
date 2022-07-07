using General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSoft.Registry.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public static class CheckEx
    {
        public static void Check<T>(IEnumerable<T> src, IEnumerable<T> dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for (int i = 0; i < src.Count(); i++)
            {
                Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        public static void Check<T>(T[] src, T[] dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for (int i = 0; i < src.Count(); i++)
            {
                Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        public static void Check<T>(this List<T> src, List<T> dst)
        {
            if (src.Count() != dst.Count())
            {
                Assert.Fail($"src:{src.Count()} dst:{dst.Count()}");
            }
            for (int i = 0; i < src.Count(); i++)
            {
                Check(src.ElementAt(i), dst.ElementAt(i));
            }
        }

        public static void Check<T>(T src, T dst)
        {
            var typecode = Type.GetTypeCode(typeof(T));
            if (src == null && dst == null)
            {

            }
            else if (typecode == TypeCode.String)
            {
                string str_src = src as string;
                string str_dst = dst as string;
                if (str_src != str_dst)
                {
                    Assert.Fail($"fail src:{str_src} dst:{str_dst}");
                }
            }
            else
            {
                var pps = typeof(T).GetProperties().Where(x => x.CanRead == true && x.GetCustomAttributes(true).Any(y => y is RegIgnore || y is RegSubKeyName) == false);
                foreach (var pp in pps)
                {

                    dynamic s = pp.GetValue(src);
                    dynamic d = pp.GetValue(dst);
                    if (pp.PropertyType == typeof(InstalledApp))
                    {
                        Check(s, d);
                    }
                    else if (pp.PropertyType.IsGenericType == true && pp.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        Check(s, d);
                    }
                    else
                    {
                        if (s != d)
                        {
                            Assert.Fail($"{pp.Name} fail src:{s} dst:{d}");
                        }
                    }


                }
            }
        }
    }
}
