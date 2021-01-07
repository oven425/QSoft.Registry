using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using QSoft.Reg.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> ll = new List<int>();
            //ll.Where
            RegistryKey reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey uninstall = reg.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            //foreach (var oo in uninstall.GetSubKeyNames())
            //{
            //    RegistryKey subkey = uninstall.OpenSubKey(oo);
            //    string displayname = subkey.GetValue("DisplayName") as string;
            //    System.Diagnostics.Trace.WriteLine(displayname);
            //    //subkey.Dispose();
            //}
            //var vv = uninstall.Where(x => x.GetValue<string>("DisplayName")== "Intel(R) Processor Graphics");
            //foreach(var oo in vv)
            //{

            //}

            var vv = from regt in uninstall where reg.GetValue<string>("DisplayName") == "Intel(R) Processor Graphics" select regt;
            foreach (var oo in vv)
            {

            }
        }
    }
}
