using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class BTree<T> where T : System.IComparable<T>
    {
        Comparer<T> m_Compare = Comparer<T>.Default;

        public void Traverse()
        {
            var node = this.Root;
            while (node.Nodes.Count != 0)
            {
                node = node.Nodes[0];
            }

            Stack<List<BTreeNode<T>>> ll = new Stack<List<BTreeNode<T>>>();
            List<BTreeNode<T>> childs = new List<BTreeNode<T>>();
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
            while (true)
            {
                var parents = childs.GroupBy(x => x.Parent).Select(x => x.Key).ToList();
                ll.Push(parents);
                childs.Clear();
                childs.AddRange(parents);
                if (parents.Count == 1)
                {
                    break;
                }
            }
            StringBuilder strb = new StringBuilder();
            var op = ll.Aggregate(new StringBuilder(), (aa, bb) =>
            {
                aa.AppendLine(bb.Aggregate("", (hh, yy) =>
                {
#if NoItems
                    var str = yy.Nodes.Select(x => x.Value.ToString()).Aggregate("", (x, y) => x + (string.IsNullOrEmpty(x) ? "" : ",") + y);
#else
                    var str = yy.Items.Select(x => x).Select(x => x.ToString()).Aggregate("", (x, y) => x + (string.IsNullOrEmpty(x) ? "" : ",") + y);
#endif
                    if (string.IsNullOrEmpty(hh) == false)
                    {
                        str = hh + "-" + str;
                    }
                    return str;
                }));
                return aa;
            });
            System.Diagnostics.Trace.WriteLine(op.ToString());
        }
        public BTreeNode<T> Root { protected set; get; } = null;
#if NoItems
        BTreeNode<T> Insert(BTreeNode<T> node, T data)
        {
            BTreeNode<T> hr = null;
            int index = int.MaxValue;

            if (this.m_Compare.Compare(data, node.Nodes.First().Value) < 0)
            {
                index = int.MinValue;
            }
            else if (this.m_Compare.Compare(data, node.Nodes.Last().Value) > 0)
            {
                index = int.MaxValue;
            }
            else
            {
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    if (this.m_Compare.Compare(node.Nodes[i].Value, data) == 0)
                    {
                        index = i;
                        break;
                    }
                    else if (this.m_Compare.Compare(data, node.Nodes[i].Value) < 0)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index == int.MaxValue)
            {
                if(node == this.Root)
                {
                    node.Nodes.Add(new BTreeNode<T>() { Value = data });
                    hr = node;
                }
                else if (node.Nodes.Count > 0)
                {
                    hr = this.Insert(node.Nodes.Last(), data);
                }
                else
                {
                    hr = node;
                }

            }
            else if (index == int.MinValue)
            {
                if (node == this.Root)
                {
                    node.Nodes.Add(new BTreeNode<T>() { Value = data });
                    hr = node;
                }
                else if (node.Nodes.Count > 0)
                {
                    hr = this.Insert(node.Nodes.First(), data);
                }
                else
                {
                    hr = node;
                }
            }
            else
            {
                hr = node;
            }
            return hr;
        }

#else
        BTreeNode<T> Insert(BTreeNode<T> node, T data)
        {
            BTreeNode<T> hr = null;
            int index = int.MaxValue;

            if (this.m_Compare.Compare(data, node.Items.First()) < 0)
            {
                index = int.MinValue;
            }
            else if (this.m_Compare.Compare(data, node.Items.Last()) > 0)
            {
                index = int.MaxValue;
            }
            else
            {
                for (int i = 0; i < node.Items.Count; i++)
                {
                    if (this.m_Compare.Compare(node.Items[i], data) == 0)
                    {
                        index = i;
                        break;
                    }
                    else if (this.m_Compare.Compare(data, node.Items[i]) < 0)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index == int.MaxValue)
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
            else if (index == int.MinValue)
            {
                if (node.Nodes.Count > 0)
                {
                    hr = this.Insert(node.Nodes.First(), data);
                }
                else
                {
                    node.Items.Insert(0, data);
                    hr = node;
                }
            }
            else
            {
                node.Items.Insert(index, data);
                hr = node;
            }
            return hr;
        }

#endif
#if NoItems
        public BTreeNode<T> CheckDegree(BTreeNode<T> node, int degree)
        {
            if (node.Nodes.Count > degree)
            {
                BTreeNode<T> right = null;
                BTreeNode<T> left = null;
                int count = node.Nodes.Count;
                int middle = node.Nodes.Count / degree;
                if (node == this.Root)
                {
                    if (node.Nodes.Count == 0)
                    {
                        left = new BTreeNode<T>();
                        //left.Items.AddRange(node.Items.Take(middle));

                        right = new BTreeNode<T>();
                        //right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                    }
                    else
                    {
                        int takecount = node.Nodes.Count / 2;
                        left = new BTreeNode<T>();
                        //left.Items.AddRange(node.Items.Take(middle));
                        left.Nodes.AddRange(node.Nodes.Take(takecount));
                        left.Nodes.ForEach(x => x.Parent = left);

                        right = new BTreeNode<T>();
                        //right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
                        right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                        right.Nodes.ForEach(x => x.Parent = right);

                        var center = new BTreeNode<T>();
                        //center.Items.AddRange(node.Items.Skip(middle).Take(1));
                        center.Nodes.Add(left);
                        center.Nodes.Add(right);
                        right.Parent = center;
                        left.Parent = center;
                        left.Next = right;
                        if (node == this.Root)
                        {
                            this.Root = center;
                        }
                        return center;
                    }
                }
                else if (node.Nodes.Count == 0)
                {
                    left = new BTreeNode<T>();
                    //left.Items.AddRange(node.Items.Take(middle));
                    right = new BTreeNode<T>();
                    //right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                }
                else
                {
                    int takecount = node.Nodes.Count / 2;
                    left = new BTreeNode<T>();
                    //left.Items.AddRange(node.Items.Take(middle));
                    left.Nodes.AddRange(node.Nodes.Take(takecount));
                    left.Nodes.ForEach(x => x.Parent = left);

                    right = new BTreeNode<T>();
                    //right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
                    var nodes = node.Nodes.Skip(takecount).Take(takecount);
                    right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                    right.Nodes.ForEach(x => x.Parent = right);

                    var center = node.Parent ?? new BTreeNode<T>();
                    center.Nodes.Remove(node);
                    //center.Items.AddRange(node.Items.Skip(middle).Take(1));
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

                BTreeNode<T> parent = node.Parent ?? new BTreeNode<T>();
                //if (parent.Items.Count == 0)
                //{
                //    parent.Items.Add(node.Items[middle]);
                //    parent.Nodes.Add(left);
                //    parent.Nodes.Add(right);
                //    left.Parent = parent;
                //    left.Next = right;
                //    right.Parent = parent;
                //}
                //else
                //{
                //    int findindex = int.MaxValue;

                //    for (int i = 0; i < parent.Nodes.Count; i++)
                //    {
                //        if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) == 0)
                //        {
                //            findindex = i;
                //            break;
                //        }
                //        else if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) >= 0)
                //        {
                //            findindex = i;
                //            break;
                //        }
                //    }
                //    parent.Nodes.Remove(node);
                //    if (left.Nodes.Count == 0)
                //    {
                //        var lastnode = parent.Nodes.LastOrDefault();
                //        left.Next = right;
                //        right.Next = node.Next;
                //        if (parent.Nodes.Count > 0)
                //        {
                //            parent.Nodes.Last().Next = left;
                //        }

                //    }
                //    if (findindex == int.MaxValue)
                //    {
                //        //parent.Items.Add(node.Items[middle]);
                //    }
                //    else
                //    {
                //        //parent.Items.Insert(findindex, node.Items[middle]);
                //    }
                //    if (findindex == int.MaxValue)
                //    {
                //        parent.Nodes.Add(left);
                //    }
                //    else
                //    {
                //        parent.Nodes.Insert(findindex, left);
                //    }
                //    if (findindex == int.MaxValue)
                //    {
                //        parent.Nodes.Add(right);
                //    }
                //    else
                //    {
                //        parent.Nodes.Insert(findindex + 1, right);
                //    }

                //    for (int i = 0; i < parent.Nodes.Count; i++)
                //    {
                //        parent.Nodes[i].Parent = parent;
                //    }
                //}


                //if (this.Root == node)
                //{
                //    this.Root = parent;
                //}


                return parent;
            }
            return node;
        }
#else
        public int Degree { set; get; }
        public BTreeNode<T> CheckDegree(BTreeNode<T> node, int degree)
        {
            if (node.Items.Count > degree)
            {
                BTreeNode<T> right = null;
                BTreeNode<T> left = null;
                int count = node.Items.Count;
                int middle = node.Items.Count / degree;
                if (node == this.Root)
                {
                    if (node.Nodes.Count == 0)
                    {
                        left = new BTreeNode<T>();
                        left.Items.AddRange(node.Items.Take(middle));

                        right = new BTreeNode<T>();
                        right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                    }
                    else
                    {
                        int takecount = node.Nodes.Count / 2;
                        left = new BTreeNode<T>();
                        left.Items.AddRange(node.Items.Take(middle));
                        left.Nodes.AddRange(node.Nodes.Take(takecount));
                        left.Nodes.ForEach(x => x.Parent = left);

                        right = new BTreeNode<T>();
                        right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
                        right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                        right.Nodes.ForEach(x => x.Parent = right);

                        var center = new BTreeNode<T>();
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
                    left = new BTreeNode<T>();
                    left.Items.AddRange(node.Items.Take(middle));
                    right = new BTreeNode<T>();
                    right.Items.AddRange(node.Items.Skip(middle).Take(count - middle));
                }
                else
                {
                    int takecount = node.Nodes.Count / 2;
                    left = new BTreeNode<T>();
                    left.Items.AddRange(node.Items.Take(middle));
                    left.Nodes.AddRange(node.Nodes.Take(takecount));
                    left.Nodes.ForEach(x => x.Parent = left);

                    right = new BTreeNode<T>();
                    right.Items.AddRange(node.Items.Skip(middle + 1).Take(count - middle - 1));
                    var nodes = node.Nodes.Skip(takecount).Take(takecount);
                    right.Nodes.AddRange(node.Nodes.Skip(takecount).Take(takecount));
                    right.Nodes.ForEach(x => x.Parent = right);

                    var center = node.Parent ?? new BTreeNode<T>();
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

                BTreeNode<T> parent = node.Parent ?? new BTreeNode<T>();
                if (parent.Items.Count == 0)
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
                        if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) == 0)
                        {
                            findindex = i;
                            break;
                        }
                        else if (this.m_Compare.Compare(parent.Items[i], node.Items[middle]) >= 0)
                        {
                            findindex = i;
                            break;
                        }
                    }
                    parent.Nodes.Remove(node);
                    if (left.Nodes.Count == 0)
                    {
                        var lastnode = parent.Nodes.LastOrDefault();
                        left.Next = right;
                        right.Next = node.Next;
                        if (parent.Nodes.Count > 0)
                        {
                            parent.Nodes.Last().Next = left;
                        }

                    }
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
                        parent.Nodes.Insert(findindex + 1, right);
                    }

                    for (int i = 0; i < parent.Nodes.Count; i++)
                    {
                        //if (i + 1 == parent.Nodes.Count)
                        //{
                        //    parent.Nodes[i].Next = null;
                        //}
                        //else
                        //{
                        //    parent.Nodes[i].Next = parent.Nodes[i + 1];
                        //}
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
#endif

        public void Insert(T data)
        {
            if (this.Root == null)
            {
                this.Root = new BTreeNode<T>();
#if NoItems
                this.Root.Nodes.Add(new BTreeNode<T>() { Value = data });
#else
                this.Root.Items.Add(data);
#endif
            }
            else
            {
                var node = this.Insert(this.Root, data);

                while (true)
                {
                    var parent_node = this.CheckDegree(node, 2);
                    if (parent_node == node)
                    {
                        break;
                    }
                    node = parent_node;
                }


            }
        }

        
    }

}
