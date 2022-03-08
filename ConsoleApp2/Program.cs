using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    //study http://csharpexamples.com/c-binary-search-tree-implementation/
    class Program
    {
        static void Main(string[] args)
        {
            Node node = new Node { Value = 1 };

        }

        static void Insert()
        {
            
        }
    }

    public class BinaryTree
    {
        public Node Root { set; get; }
        public void Insert(int data)
        {
            if(this.Root == null)
            {
                this.Root = new Node() { Value = data };
                return;
            }
            while(true)
            {
                if(this.Root.Value >data)
                {

                }
                else
                {

                }
            }
        }
    }
    

    public class Node
    {
        public Node Left { set; get; }
        public Node Right { set; get; }
        public int Value { set; get; }
    }



}
