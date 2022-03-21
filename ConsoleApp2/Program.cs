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
            var ll = new List<int>() {1,3,5,7 };

            int data = -1;
            int find = 0;
            for(int i= 0; i<ll.Count; i++)
            {
                if(ll[i] == data)
                {
                    find = i;
                    break;
                }
                else if(data< ll[i])
                {
                    find = i;
                    break;
                }
            }
            BTree btree = new BTree();
            btree.Insert(1);
            btree.Insert(2);
            btree.Insert(3);
            btree.Insert(4);

            var isfind = btree.Find(5);
            Console.ReadLine();
        }
    }

    public class BTreeItem
    {
        public int Value { set; get; }

        public override string ToString()
        {
            return $"Value:{this.Value}";
        }
    }

    public class BTreeNode
    {
        //public BTreeNode Left { set; get; }
        //public BTreeNode Right { set; get; }
        public BTreeNode Parent { set; get; }
        public List<BTreeItem> Items { set; get; } = new List<BTreeItem>();
        public List<BTreeNode> Nodes { set; get; } = new List<BTreeNode>();
        public BTreeNode Next { set; get; }
    }

    public class BTreeLeaf
    {
        public BTreeNode Parent { set; get; }
        public List<BTreeItem> Items { set; get; } = new List<BTreeItem>();
        public BTreeNode Next { set; get; }
    }


    public class BTree
    {
        public bool Find(int data)
        {
            return this.Find(this.Root, data);
        }

        bool Find(BTreeNode node, int data)
        {
            int index = 0;
            for (; index < node.Items.Count; index++)
            {
                if (node.Items[index].Value == data)
                {
                    if (node.Next != null && node.Parent != null)
                    {
                        return true;
                    }
                    else if(node.Nodes.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        if (index+1 == node.Nodes.Count-1)
                        {
                            return this.Find(node.Nodes.Last(), data);
                        }
                        return this.Find(node.Nodes[index], data);
                    }
                }
                else if (node.Items[index].Value > data)
                {
                    if (node.Nodes.Count == 0)
                    {
                        return false;
                    }
                    else if (index == 0)
                    {
                        return this.Find(node.Nodes.First(), data);
                    }
                    break;
                }
            }
            return false;
        }

        public BTreeNode Root { protected set; get; } = null;

        BTreeNode Insert(BTreeNode node, int data)
        {
            BTreeNode hr = null;
            int index = int.MaxValue;
            for (int i = 0; i < node.Items.Count; i++)
            {
                if (node.Items[i].Value == data)
                {
                    break;
                }
                else if (data < node.Items[i].Value)
                {
                    break;
                }
            }

            if(index == int.MaxValue)
            {
                if (node.Nodes.Count > 0)
                {
                    hr = this.Insert(node.Nodes.Last(), data);
                }
                else
                {
                    node.Items.Add(new BTreeItem() { Value = data });
                    hr = node;
                }
                
            }
            //var first = node.Items.FirstOrDefault(x => x.Value >= data);
            //if (first != null)
            //{
            //    var idx = node.Items.IndexOf(first);
            //    node.Items.Insert(idx, new BTreeItem() { Value = data });
            //    hr = node;
            //}
            //else if (first == null && node.Right != null)
            //{
            //    hr = this.Insert(node.Right, data);
            //}
            //else
            //{
            //    node.Items.Add(new BTreeItem() { Value = data });
            //    hr = node;
            //}
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

                BTreeNode parent = node.Parent ?? new BTreeNode();
                parent.Items.Add(node.Items[middle]);
                if (parent.Items[middle - 1] == left.Items[0])
                {
                    left = new BTreeNode();
                    left.Items.AddRange(node.Items.Take(middle));
                    parent.Nodes.Insert(parent.Nodes.Count-1, left);
                    parent.Nodes.Remove(parent.Nodes.Last());
                    //parent.Left.Next = left;
                    //parent.Nodes.Insert(0, left);
                }
                else
                {
                    //parent.Left = left;
                    parent.Nodes.Insert(0, left);
                }

                //parent.Right = right;
                parent.Nodes.Add(right);
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
