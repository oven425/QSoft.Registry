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
        static int BB<T>(T a, T b) where T : System.IComparable<T>
        {
            //System.IComparable<T>

            return Comparer<T>.Default.Compare(a, b);
        }

        static void Main(string[] args)
        {
            //int a = Comparer<TY>.Default.Compare(new TY(), new TY());
            //int b = Comparer<int>.Default.Compare(2, 1);
            //int c = Comparer<int>.Default.Compare(1, 1);
            BTree<int> btree = new BTree<int>();
            btree.Insert(1);
            btree.Insert(2);
            btree.Insert(3);
            btree.Insert(4);
            btree.Insert(5);
            btree.Insert(6);
            btree.Insert(7);
            btree.Insert(8);
            btree.Insert(9);
            //var isfind = btree.Find(5);
            btree.Traverse();
            
            Console.ReadLine();
        }
        
    }


    public class BTreeNodeBase
    {
        public BTreeNode Parent { set; get; }
        public List<int> Items { set; get; } = new List<int>();
    }
    public class BTreeNode
    {
        public BTreeNode Parent { set; get; }
        public List<int> Items { set; get; } = new List<int>();
        public List<BTreeNode> Nodes { set; get; } = new List<BTreeNode>();
        public BTreeNode Next { set; get; }
    }

    public class BTreeLeaf: BTreeNodeBase
    {
        public BTreeNode Next { set; get; }
    }

    public class BTree<T> where T: System.IComparable<T>
    {
        Comparer<T> m_Compare = Comparer<T>.Default;

        public void Traverse()
        {
            var node = this.Root;
            while (node.Nodes.Count != 0)
            {
                node = node.Nodes[0];
            }

            Stack<List<BTreeNode>> ll = new Stack<List<BTreeNode>>();
            List<BTreeNode> childs = new List<BTreeNode>();
            while (true)
            {
                childs.Add(node);
                node = node.Next;
                if (node == null)
                {
                    ll.Push(childs.ToList());
                    
                    break;
                }
            }
            while(true)
            {
                var parents = childs.GroupBy(x => x.Parent).Select(x => x.Key).ToList();
                ll.Push(parents);
                childs.Clear();
                childs.AddRange(parents);
                if(parents.Count == 1)
                {
                    break;
                }
            }
            StringBuilder strb = new StringBuilder();
            foreach(var levels in ll)
            {
                foreach(var item in levels)
                {
                    item.Items.ForEach(x => strb.Append($"{x},"));
                }
                strb.AppendLine();
            }
            System.Diagnostics.Trace.WriteLine(strb.ToString());
        }
        public BTreeNode Root { protected set; get; } = null;

        BTreeNode Insert(BTreeNode node, int data)
        {
            BTreeNode hr = null;
            int index = int.MaxValue;

            for (int i = 0; i < node.Items.Count; i++)
            {
                if (node.Items[i] == data)
                {
                    break;
                }
                else if (data < node.Items[i])
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
                    node.Items.Add(data);
                    hr = node;
                }
                
            }

            return hr;
        }

        public BTreeNode CheckDegree(BTreeNode node, int degree)
        {
            if (node.Items.Count > degree)
            {
                BTreeNode right = null;
                BTreeNode left = null;
                int count = node.Items.Count;
                int middle = node.Items.Count / degree;
                if (node == this.Root)
                {
                    if(node.Nodes.Count == 0)
                    {
                        left = new BTreeNode();
                        left.Items.AddRange(node.Items.Take(middle));

                        right = new BTreeNode();
                        right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                    }
                    else
                    {
                        int takecount = node.Nodes.Count / 2;
                        left = new BTreeNode();
                        left.Items.AddRange(node.Items.Take(middle));
                        left.Nodes.AddRange(node.Nodes.Take(takecount));
                        left.Nodes.ForEach(x => x.Parent = left);

                        right = new BTreeNode();
                        right.Items.AddRange(node.Items.Skip(middle+1).Take(count - middle-1));
                        right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                        right.Nodes.ForEach(x => x.Parent = right);

                        var center = new BTreeNode();
                        center.Items.AddRange(node.Items.Skip(middle).Take(1));
                        center.Nodes.Add(left);
                        center.Nodes.Add(right);
                        right.Parent = center;
                        left.Parent = center;
                        if (node == this.Root)
                        {
                            this.Root = center;
                        }
                        return center;
                    }
                }
                else if (node.Nodes.Count == 0)
                {
                    left = new BTreeNode();
                    left.Items.AddRange(node.Items.Take(middle));

                    right = new BTreeNode();
                    right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                }
                else
                {
                    int takecount = node.Nodes.Count / 2;
                    left = new BTreeNode();
                    left.Items.AddRange(node.Items.Take(middle));
                    left.Nodes.AddRange(node.Nodes.Take(takecount));
                    left.Nodes.ForEach(x => x.Parent = left);

                    right = new BTreeNode();
                    right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
                    var nodes = node.Nodes.Skip(takecount).Take(takecount);
                    right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                    right.Nodes.ForEach(x => x.Parent = right);

                    var center = node.Parent??new BTreeNode();
                    center.Nodes.Remove(node);
                    center.Items.AddRange(node.Items.Skip(middle).Take(1));
                    center.Nodes.Add(left);
                    center.Nodes.Add(right);
                    right.Parent = center;
                    left.Parent = center;
                    if (node == this.Root)
                    {
                        this.Root = center;
                    }
                    return center;
                }

                BTreeNode parent = node.Parent ?? new BTreeNode();
                //parent.Items.Add(node.Items[middle]);
                //if (parent.Items[middle - 1] == left.Items[0])
                //{
                //    left = new BTreeNode();
                //    left.Items.AddRange(node.Items.Take(middle));
                //    parent.Nodes.Insert(parent.Nodes.Count-1, left);
                //    parent.Nodes.Remove(parent.Nodes.Last());
                //    //parent.Left.Next = left;
                //    //parent.Nodes.Insert(0, left);
                //}
                //else
                //{
                //    //parent.Left = left;
                //    parent.Nodes.Insert(0, left);
                //}

                //parent.Right = right;
                if(parent.Items.Count == 0)
                {
                    parent.Items.Add(node.Items[middle]);
                    parent.Nodes.Add(left);
                    parent.Nodes.Add(right);
                    left.Parent = parent;
                    left.Next = right;
                    right.Parent = parent;
                }
                else
                {
                    int findindex = int.MaxValue;
                    for (int i = 0; i < parent.Items.Count; i++)
                    {
                        if (parent.Items[i] == node.Items[middle])
                        {
                            findindex = i;
                            break;
                        }
                        else if (parent.Items[i] > node.Items[middle])
                        {
                            findindex = i;
                            break;
                        }
                    }
                    parent.Nodes.Remove(node);

                    if (findindex == int.MaxValue)
                    {
                        parent.Items.Add(node.Items[middle]);
                    }
                    else
                    {
                        parent.Items.Insert(findindex, node.Items[middle]);
                    }
                    if (findindex == int.MaxValue)
                    {
                        parent.Nodes.Add(left);
                    }
                    else
                    {
                        parent.Nodes.Insert(findindex, left);
                    }
                    if (findindex == int.MaxValue)
                    {
                        parent.Nodes.Add(right);
                    }
                    else
                    {
                        parent.Nodes.Insert(findindex + 1, left);
                    }
                    for (int i = 0; i < parent.Nodes.Count; i++)
                    {
                        if (i + 1 == parent.Nodes.Count)
                        {
                            parent.Nodes[i].Next = null;
                        }
                        else
                        {
                            parent.Nodes[i].Next = parent.Nodes[i + 1];
                        }
                        parent.Nodes[i].Parent = parent;
                    }
                }
                
                
                if (this.Root == node)
                {
                    this.Root = parent;
                }

                
                return parent;
            }
            return node;
        }

        public void Insert(int data)
        {
            if(this.Root == null)
            {
                this.Root = new BTreeNode();
                this.Root.Items.Add(data);
            }
            else
            {
                var node = this.Insert(this.Root, data);
                
                while(true)
                {
                    var parent_node = this.CheckDegree(node, 2);
                    if(parent_node == node)
                    {
                        break;
                    }
                    node = parent_node;
                }
                
                
            }
        }

        //public bool Find(int data)
        //{
        //    return this.Find(this.Root, data);
        //}

        //bool Find(BTreeNode node, int data)
        //{
        //    int index = 0;
        //    for (; index < node.Items.Count; index++)
        //    {
        //        if (node.Items[index] == data)
        //        {
        //            if (node.Next != null && node.Parent != null)
        //            {
        //                return true;
        //            }
        //            else if(node.Nodes.Count == 0)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                if (index+1 == node.Nodes.Count-1)
        //                {
        //                    return this.Find(node.Nodes.Last(), data);
        //                }
        //                return this.Find(node.Nodes[index], data);
        //            }
        //        }
        //        else if (node.Items[index] > data)
        //        {
        //            if (node.Nodes.Count == 0)
        //            {
        //                return false;
        //            }
        //            else if (index == 0)
        //            {
        //                return this.Find(node.Nodes.First(), data);
        //            }
        //            break;
        //        }
        //    }
        //    return false;
        //}


    }



}
