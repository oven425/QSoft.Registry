using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            BTree btree = new BTree();
            btree.Insert(1);
            btree.Insert(2);
            btree.Insert(3);
            btree.Insert(4);
            Console.ReadLine();
        }
    }

    public class BTreeItem
    {
        public int Value { set; get; }
        //public BTreeNode Left { set; get; }
        //public BTreeNode Right { set; get; }

        public override string ToString()
        {
            return $"Value:{this.Value}";
        }
    }

    public class BTreeNode
    {
        public BTreeNode Left { set; get; }
        public BTreeNode Right { set; get; }
        public BTreeNode Next { set; get; }
        public List<BTreeItem> Items { set; get; } = new List<BTreeItem>();
        public BTreeNode Parent { set; get; }
    }


    public class BTree
    {
        BTreeNode Root = null;

        BTreeNode Insert(BTreeNode node, int data)
        {
            BTreeNode hr = null;

            var first = node.Items.FirstOrDefault(x => x.Value >= data);
            if (first != null)
            {
                var idx = node.Items.IndexOf(first);
                node.Items.Insert(idx, new BTreeItem() { Value = data });
                hr = node;
            }
            else if (first == null && node.Right != null)
            {
                hr = this.Insert(node.Right, data);
            }
            else
            {
                node.Items.Add(new BTreeItem() { Value = data });
                hr = node;
            }
            return hr;
        }

        public BTreeNode CheckDegree(BTreeNode node, int degree)
        {
            if (node.Items.Count > degree)
            {
                int count = node.Items.Count;
                int middle = node.Items.Count / degree;
                var left = new BTreeNode();
                left.Items.AddRange(node.Items.Take(middle));

                var right = new BTreeNode();
                right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));

                BTreeNode parent = node.Parent?? new BTreeNode();
                parent.Items.Add(node.Items[middle]);
                if(parent.Items[middle-1] == left.Items[0])
                {
                    left = new BTreeNode();
                    left.Items.Add(parent.Items[middle - 1]);
                    parent.Left.Next = left;
                }
                else
                {
                    parent.Left = left;
                }
                
                parent.Right = right;
                if (this.Root == node)
                {
                    this.Root = parent;
                }
                left.Parent = parent;
                left.Next = right;
                right.Parent = parent;
                return parent;
            }
            return node;
        }

        public void Insert(int data)
        {
            if(this.Root == null)
            {
                this.Root = new BTreeNode();
                this.Root.Items.Add(new BTreeItem() { Value = data });
            }
            else
            {
                var node = this.Insert(this.Root, data);
                
                this.CheckDegree(node, 2);
                
            }
        }

    }



}
