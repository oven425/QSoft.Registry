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
        async static Task Main(string[] args)
        {
            var ll = Enumerable.Range(1, 10).Select(x => new Data() { Size = x }).ToList();

            List<Task> tasks = new List<Task>();

            //tasks.Add(A(1));
            //tasks.Add(A(2));
            //tasks.Add(A(3));
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancelToken;
            var taskA = Task<string>.Run(() =>
            {
                throw new Exception("DoWork failed.");
                cts.Cancel();
                return "Run";
            })
            .ContinueWith(x =>
            {
                System.Diagnostics.Trace.WriteLine(x.Exception);
                cts.Cancel();
                return "OnlyOnFaulted";
            }, TaskContinuationOptions.OnlyOnFaulted)
            .ContinueWith((x) =>
            {
                return "A";
            });


            
            

            try
            {
                //var aa = await taskA;
                System.Diagnostics.Trace.WriteLine(await taskA);
            }
            catch(Exception ee)
            {

            }
            
            
            //tasks.Add(new Task(A(1));
            //tasks.Add(new Task(async () => await A(2)));
            //tasks.Add(new Task(async () => await A(3)));
            //tasks.ForEach(x => x.Start());
            //foreach (var oo in tasks)
            //{
            //    await oo;
            //    Task.WaitAll(oo);
            //}
            //Task.WaitAll(tasks.ToArray());
            //await Task.WhenAll(tasks);
            System.Diagnostics.Trace.WriteLine("end");
            Console.ReadLine();
        }


        async static Task<int> A(int data)
        {
            System.Diagnostics.Trace.WriteLine($"begin {data}");
            await Task.Delay(1000);
            //System.Threading.Thread.Sleep(1000);
            System.Diagnostics.Trace.WriteLine($"end {data}");
            return data * data;
        }

    }

    public static class TaskExtensions
    {
        public static Task<T> FailFastOnException<T>(this Task<T> task)
        {
            task.ContinueWith(c => 
            {
                Environment.FailFast("Task faulted", c.Exception);

            },
                TaskContinuationOptions.OnlyOnFaulted); // 例外發生時才執行。
            return task;
        }
        public static Task FailFastOnException(this Task task)
        {
            task.ContinueWith(c =>
            {
                Environment.FailFast("Task faulted", c.Exception);
            },
                TaskContinuationOptions.OnlyOnFaulted); // 例外發生時才執行。
            return task;
        }

        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => Console.WriteLine(c.Exception),
                TaskContinuationOptions.OnlyOnFaulted); // 例外發生時才執行。
            return task;
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
