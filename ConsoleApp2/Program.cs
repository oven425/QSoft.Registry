using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var ll = Enumerable.Range(1, 10).Select(x => new Data() { Size = x }).ToList();


            var t1 = Task.Run(async() =>
            {
                await Task.Delay(3000);
                return 1;
            });
            var t2 = Task.Run(async () =>
            {
                await Task.Delay(300);
                throw new Exception("AA");
            })
            .ContinueWith((x) => 
            {
                Console.WriteLine(x.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);

            Task.WhenAll(t1, t2).ContinueWith((x) =>
            {

            }).Wait();
        }


        
    }

    public class Data
    {
        public int Size { set; get; }
    }

    public class B
    {
        public B Front { set; get; }
        public B Back { set; get; }
    }
}
