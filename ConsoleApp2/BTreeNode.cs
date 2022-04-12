using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    
    public class BTreeNode<T>
    {
        public BTreeNode<T> Parent { set; get; }
        
        public List<BTreeNode<T>> Nodes { set; get; } = new List<BTreeNode<T>>();
        public BTreeNode<T> Next { set; get; }
#if NoItems
        public T Value { set; get; }
#else
        public List<T> Items { set; get; } = new List<T>();
#endif
    }

    public class BTreeLeaf<T>
    {

    }
}
