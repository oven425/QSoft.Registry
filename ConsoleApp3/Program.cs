using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titan.ODataClient;

namespace ConsoleApp3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var queryProvider = new CustomLinqProvider<Person1>();
            var query = queryProvider
                    .Where(x => x.IsActive == true)
                .Where(x => x.Id == 1 && x.Name == "Test" && (x.IsActive == true || x.Salary > 5000) && (x.IsActive || x.Salary > 5000))
                .Select(x => new { x.Name, x.Id })
                ;
            var result = query.Single();
        }
    }

    public class Person1
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }
}
