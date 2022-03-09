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
            BinaryTree btree = new BinaryTree();
            btree.Insert(10);
            btree.Insert(9);
            btree.Insert(8);
            Console.ReadLine();
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
            Node parent = this.Root;
            while(true)
            {
                if(parent.Value > data)
                {
                    if(parent.Left == null)
                    {
                        parent.Left = new Node() { Value = data };
                        break;
                    }
                    else
                    {
                        parent = parent.Left;
                    }
                }
                else
                {
                    if (parent.Right == null)
                    {
                        parent.Right = new Node() { Value = data };
                        break;
                    }
                    else
                    {
                        parent = parent.Right;
                    }
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
