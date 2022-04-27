using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Node : INode
    {
        public int Key => Leafs==null?Nodes.Min(x=>x.Key):Leafs.Min(x=>x.Key);
        public List<INode> Nodes { set; get; } = new List<INode>();
        public List<ILeaf> Leafs { set; get; } = null;
    }


    public class Leaf: ILeaf
    {
        public int Key => Values.Min();
        public List<int> Values { set; get; } = new List<int>();
        public ILeaf Next { set; get; }
    }

    public interface ILeaf
    {
        int Key { get; }
        ILeaf Next { set; get; }
        List<int> Values { set; get; }
    }

    public interface INode
    {
        int Key { get; }
        List<INode> Nodes { set; get; }
        List<ILeaf> Leafs { set; get; }
    }

    public class Tree
    {
        public Node Root { set; get; }
    }



    class Program
    {
        static void Main(string[] args)
        {
            var li = Enumerable.Range(1, 9).ToList();
            int agree = 3;
            List<Leaf> leafs = new List<Leaf>();
            int skip = 0;
            while (true)
            {
                var values = li.Skip(skip).Take(agree - 1);
                Leaf leaf = new Leaf();
                leaf.Values.AddRange(values);
                if(leafs.Count > 0)
                {
                    leafs.Last().Next = leaf;
                }
                leafs.Add(leaf);
                if (values.Count() < (agree - 1))
                {
                    break;
                }
                skip = skip + (agree - 1);
            }
            List<List<Node>> tree = new List<List<Node>>();
            List<Node> nodes = new List<Node>();
            skip = 0;
            while (true)
            {
                Node node = new Node();
                node.Leafs = new List<ILeaf>();
                while(true)
                {
                    var leaf = leafs.Skip(skip).Take(agree - 1);

                    node.Leafs.AddRange(leaf);
                    skip = skip + (agree - 1);
                    if (node.Leafs.Count >= (agree - 1))
                    {
                        nodes.Add(node);
                        break;
                    }
                    else if (skip > leafs.Count)
                    {
                        break;
                    }
                }
                if(skip > leafs.Count)
                {
                    break;
                }
            }
            
            
            
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



            //btree.Insert(9);
            //btree.Insert(8);
            //btree.Insert(7);
            //btree.Insert(6);
            //btree.Insert(5);
            //btree.Insert(4);
            //btree.Insert(3);
            //btree.Insert(2);
            //btree.Insert(1);

            //btree.Insert(1);
            //btree.Insert(3);
            //btree.Insert(5);
            //btree.Insert(7);
            //btree.Insert(9);
            //btree.Insert(2);
            //btree.Insert(4);
            //btree.Insert(6);
            //btree.Insert(8);


            btree.Traverse();

            Console.ReadLine();
        }
        
    }

    //public class BTreeNode<T>
    //{
    //    public BTreeNode<T> Parent { set; get; }
    //    public List<T> Items { set; get; } = new List<T>();
    //    public List<BTreeNode<T>> Nodes { set; get; } = new List<BTreeNode<T>>();
    //    public BTreeNode<T> Next { set; get; }
    //}


    //public class BTree<T> where T: System.IComparable<T>
    //{
    //    Comparer<T> m_Compare = Comparer<T>.Default;

    //    public void Traverse()
    //    {
    //        var node = this.Root;
    //        while (node.Nodes.Count != 0)
    //        {
    //            node = node.Nodes[0];
    //        }

    //        Stack<List<BTreeNode<T>>> ll = new Stack<List<BTreeNode<T>>>();
    //        List<BTreeNode<T>> childs = new List<BTreeNode<T>>();
    //        while (true)
    //        {
    //            childs.Add(node);
    //            node = node.Next;
    //            if (node == null)
    //            {
    //                ll.Push(childs.ToList());
                    
    //                break;
    //            }
    //        }
    //        while(true)
    //        {
    //            var parents = childs.GroupBy(x => x.Parent).Select(x => x.Key).ToList();
    //            ll.Push(parents);
    //            childs.Clear();
    //            childs.AddRange(parents);
    //            if(parents.Count == 1)
    //            {
    //                break;
    //            }
    //        }
    //        StringBuilder strb = new StringBuilder();
    //        var op = ll.Aggregate(new StringBuilder(), (aa, bb) =>
    //        {
    //            aa.AppendLine(bb.Aggregate("", (hh, yy) =>
    //            {
    //                var str = yy.Items.Select(x=>x).Select(x => x.ToString()).Aggregate("", (x, y) => x + (string.IsNullOrEmpty(x) ? "" : ",") + y);
    //                if (string.IsNullOrEmpty(hh) == false)
    //                {
    //                    str = hh + "-" + str;
    //                }
    //                return str;
    //            }));
    //            return aa;
    //        });
    //        System.Diagnostics.Trace.WriteLine(op.ToString());
    //    }
    //    public BTreeNode<T> Root { protected set; get; } = null;

    //    BTreeNode<T> Insert(BTreeNode<T> node, T data)
    //    {
    //        BTreeNode<T> hr = null;
    //        int index = int.MaxValue;
    //        if (this.m_Compare.Compare(data, node.Items.First()) < 0)
    //        {
    //            index = int.MinValue;
    //        }
    //        else if(this.m_Compare.Compare(data, node.Items.Last()) > 0)
    //        {
    //            index = int.MaxValue;
    //        }
    //        else
    //        {
    //            for (int i = 0; i < node.Items.Count; i++)
    //            {
    //                if (this.m_Compare.Compare(node.Items[i], data) == 0)
    //                {
    //                    index = i;
    //                    break;
    //                }
    //                else if (this.m_Compare.Compare(data, node.Items[i]) < 0)
    //                {
    //                    index = i;
    //                    break;
    //                }
    //            }
    //        }
            

    //        if(index == int.MaxValue)
    //        {
    //            if (node.Nodes.Count > 0)
    //            {
    //                hr = this.Insert(node.Nodes.Last(), data);
    //            }
    //            else
    //            {
    //                node.Items.Add(data);
    //                hr = node;
    //            }
                
    //        }
    //        else if(index == int.MinValue)
    //        {
    //            if (node.Nodes.Count > 0)
    //            {
    //                hr = this.Insert(node.Nodes.First(), data);
    //            }
    //            else
    //            {
    //                node.Items.Insert(0, data);
    //                hr = node;
    //            }
    //        }
    //        else
    //        {
    //            node.Items.Insert(index, data);
    //            hr = node;
    //        }
    //            return hr;
    //    }

    //    public BTreeNode<T> CheckDegree(BTreeNode<T> node, int degree)
    //    {
    //        if (node.Items.Count > degree)
    //        {
    //            BTreeNode<T> right = null;
    //            BTreeNode<T> left = null;
    //            int count = node.Items.Count;
    //            int middle = node.Items.Count / degree;
    //            if (node == this.Root)
    //            {
    //                if(node.Nodes.Count == 0)
    //                {
    //                    left = new BTreeNode<T>();
    //                    left.Items.AddRange(node.Items.Take(middle));

    //                    right = new BTreeNode<T>();
    //                    right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
    //                }
    //                else
    //                {
    //                    int takecount = node.Nodes.Count / 2;
    //                    left = new BTreeNode<T>();
    //                    left.Items.AddRange(node.Items.Take(middle));
    //                    left.Nodes.AddRange(node.Nodes.Take(takecount));
    //                    left.Nodes.ForEach(x => x.Parent = left);

    //                    right = new BTreeNode<T>();
    //                    right.Items.AddRange(node.Items.Skip(middle+1).Take(count - middle-1));
    //                    right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
    //                    right.Nodes.ForEach(x => x.Parent = right);

    //                    var center = new BTreeNode<T>();
    //                    center.Items.AddRange(node.Items.Skip(middle).Take(1));
    //                    center.Nodes.Add(left);
    //                    center.Nodes.Add(right);
    //                    right.Parent = center;
    //                    left.Parent = center;
    //                    if (node == this.Root)
    //                    {
    //                        this.Root = center;
    //                    }
    //                    return center;
    //                }
    //            }
    //            else if (node.Nodes.Count == 0)
    //            {
    //                left = new BTreeNode<T>();
    //                left.Items.AddRange(node.Items.Take(middle));
    //                right = new BTreeNode<T>();
    //                right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
    //            }
    //            else
    //            {
    //                int takecount = node.Nodes.Count / 2;
    //                left = new BTreeNode<T>();
    //                left.Items.AddRange(node.Items.Take(middle));
    //                left.Nodes.AddRange(node.Nodes.Take(takecount));
    //                left.Nodes.ForEach(x => x.Parent = left);

    //                right = new BTreeNode<T>();
    //                right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
    //                var nodes = node.Nodes.Skip(takecount).Take(takecount);
    //                right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
    //                right.Nodes.ForEach(x => x.Parent = right);

    //                var center = node.Parent??new BTreeNode<T>();
    //                center.Nodes.Remove(node);
    //                center.Items.AddRange(node.Items.Skip(middle).Take(1));
    //                center.Nodes.Add(left);
    //                center.Nodes.Add(right);
    //                right.Parent = center;
    //                left.Parent = center;
    //                if (node == this.Root)
    //                {
    //                    this.Root = center;
    //                }
    //                return center;
    //            }

    //            BTreeNode<T> parent = node.Parent ?? new BTreeNode<T>();
    //            if(parent.Items.Count == 0)
    //            {
    //                parent.Items.Add(node.Items[middle]);
    //                parent.Nodes.Add(left);
    //                parent.Nodes.Add(right);
    //                left.Parent = parent;
    //                left.Next = right;
    //                right.Parent = parent;
    //            }
    //            else
    //            {
    //                int findindex = int.MaxValue;

    //                for (int i = 0; i < parent.Items.Count; i++)
    //                {
    //                    if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) == 0)
    //                    {
    //                        findindex = i;
    //                        break;
    //                    }
    //                    else if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) >=0)
    //                    {
    //                        findindex = i;
    //                        break;
    //                    }
    //                }
    //                parent.Nodes.Remove(node);
    //                if(left.Nodes.Count == 0)
    //                {
    //                    var lastnode = parent.Nodes.LastOrDefault();
    //                    left.Next = right;
    //                    right.Next = node.Next;
    //                    if(parent.Nodes.Count >0)
    //                    {
    //                        parent.Nodes.Last().Next = left;
    //                    }
                        
    //                }
    //                if (findindex == int.MaxValue)
    //                {
    //                    parent.Items.Add(node.Items[middle]);
    //                }
    //                else
    //                {
    //                    parent.Items.Insert(findindex, node.Items[middle]);
    //                }
    //                if (findindex == int.MaxValue)
    //                {
    //                    parent.Nodes.Add(left);
    //                }
    //                else
    //                {
    //                    parent.Nodes.Insert(findindex, left);
    //                }
    //                if (findindex == int.MaxValue)
    //                {
    //                    parent.Nodes.Add(right);
    //                }
    //                else
    //                {
    //                    parent.Nodes.Insert(findindex + 1, right);
    //                }

    //                for (int i = 0; i < parent.Nodes.Count; i++)
    //                {
    //                    //if (i + 1 == parent.Nodes.Count)
    //                    //{
    //                    //    parent.Nodes[i].Next = null;
    //                    //}
    //                    //else
    //                    //{
    //                    //    parent.Nodes[i].Next = parent.Nodes[i + 1];
    //                    //}
    //                    parent.Nodes[i].Parent = parent;
    //                }
    //            }
                
                
    //            if (this.Root == node)
    //            {
    //                this.Root = parent;
    //            }

                
    //            return parent;
    //        }
    //        return node;
    //    }

    //    public void Insert(T data)
    //    {
    //        if(this.Root == null)
    //        {
    //            this.Root = new BTreeNode<T>();
    //            this.Root.Items.Add(data);
    //        }
    //        else
    //        {
    //            var node = this.Insert(this.Root, data);
                
    //            while(true)
    //            {
    //                var parent_node = this.CheckDegree(node, 2);
    //                if(parent_node == node)
    //                {
    //                    break;
    //                }
    //                node = parent_node;
    //            }
                
                
    //        }
    //    }

    //    //public bool Find(int data)
    //    //{
    //    //    return this.Find(this.Root, data);
    //    //}

    //    //bool Find(BTreeNode<T> node, int data)
    //    //{
    //    //    int index = 0;
    //    //    for (; index < node.Items.Count; index++)
    //    //    {
    //    //        if (node.Items[index] == data)
    //    //        {
    //    //            if (node.Next != null && node.Parent != null)
    //    //            {
    //    //                return true;
    //    //            }
    //    //            else if(node.Nodes.Count == 0)
    //    //            {
    //    //                return true;
    //    //            }
    //    //            else
    //    //            {
    //    //                if (index+1 == node.Nodes.Count-1)
    //    //                {
    //    //                    return this.Find(node.Nodes.Last(), data);
    //    //                }
    //    //                return this.Find(node.Nodes[index], data);
    //    //            }
    //    //        }
    //    //        else if (node.Items[index] > data)
    //    //        {
    //    //            if (node.Nodes.Count == 0)
    //    //            {
    //    //                return false;
    //    //            }
    //    //            else if (index == 0)
    //    //            {
    //    //                return this.Find(node.Nodes.First(), data);
    //    //            }
    //    //            break;
    //    //        }
    //    //    }
    //    //    return false;
    //    //}


    //}



}
