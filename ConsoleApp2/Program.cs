using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            A a = new A();
            a.AA(10);
            B<string> b = new B<string>();
            b.AA("10");
        }
    }

    public class A
    {
        public void AA(int data)
        {

        }
    }

    public class B<T>:A
    {
        public void AA(T data)
        {

        }
    }

}
