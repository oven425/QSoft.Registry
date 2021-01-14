using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using QSoft.Registry.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<CSS> ll = new List<CSS>() { new CSS(), new CSS() };
            var vvv = ll.Select(x => new { x.A, x.B });
            RegistryKey reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey uninstall = reg.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            //foreach (var oo in uninstall.GetSubKeyNames())
            //{
            //    RegistryKey subkey = uninstall.OpenSubKey(oo);
            //    string displayname = subkey.GetValue("DisplayName") as string;
            //    System.Diagnostics.Trace.WriteLine(displayname);
            //    //subkey.Dispose();
            //}
            var first = uninstall.FirstOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var last = uninstall.LastOrDefault(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics");
            var count = uninstall.Count();
            var count_1 = uninstall.Count(x => string.IsNullOrEmpty(x.GetValue<string>("DisplayName")) == false);
            var select = uninstall.Select(x => x.GetValue<string>("DisplayName"));
            var vv = uninstall.Where(x => x.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics" || x.GetValue<string>("DisplayName") == "");
            var dic = uninstall.ToDictionary(x => x.Name);
            var group = uninstall.GroupBy(x => x.GetValue<string>("DisplayName"));
            foreach(var key in group)
            {

            }


            var group1 = uninstall.ToList().GroupBy(x => x.GetValue<string>("DisplayName"));
            vv.GroupBy(x => x.Name);

            List<CSS> tt = new List<CSS>();
            for(int i=0; i<5; i++)
            {
                tt.Add(new CSS() { B=i});
                tt.Add(new CSS() { B = i });
                tt.Add(new CSS() { B = i });
                tt.Add(new CSS() { B = i });
                tt.Add(new CSS() { B = i });
            }
            
            
            var ttg = tt.GroupBy(x => x.B);
        }

        //public IEnumerable<int> Test()
        //{
        //    List<int> ll = IEnumerable<int>
        //}
    }

    public class CSS
    {
        public string A { set; get; } = "A";
        public int B { set; get; } = 10;
        public string C { set; get; } = "C";
    }


    //public class CAA
    //{
    //    public string Current
    //    {
    //        get { return _index >= 0 ? Items[_index] : null; }
    //    }

    //    public bool MoveNext()
    //    {
    //        if (_index < Items.Count - 1)
    //        {
    //            _index++;
    //            return true;
    //        }
    //        return false;
    //    }
    //    public CAA GetEnumerator()
    //    {
    //        return this;
    //    }
    //}

    public class CustomEnumerable
    {
        // A custom enumerator which has a Current property and a MoveNext() method, but does NOT implement IEnumerator.
        public class CustomEnumerator
        {
            private readonly CustomEnumerable _enumerable;
            private int _index = -1;

            public CustomEnumerator(CustomEnumerable enumerable)
            {
                _enumerable = enumerable;
            }

            private IList<string> Items
            {
                get { return _enumerable._Items; }
            }

            public string Current
            {
                get { return _index >= 0 ? Items[_index] : null; }
            }

            public bool MoveNext()
            {
                if (_index < Items.Count - 1)
                {
                    _index++;
                    return true;
                }
                return false;
            }
        }

        private IList<string> _Items;

        public CustomEnumerable(params string[] items)
        {
            _Items = new List<string>(items);
        }

        public CustomEnumerator GetEnumerator()
        {
            return new CustomEnumerator(this);
        }
    }
}
