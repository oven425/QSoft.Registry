using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestData
{
    public class Camera
    {
        public Address Remote { set; get; }
        public Address Local { set; get; }
        public List<Record> Records { set; get; }
    }

    public class Record
    {
        public DateTime Begin { set; get; }
        public DateTime End { set; get; }
    }


}
