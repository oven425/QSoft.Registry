using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Registry
{
    public class DictionaryList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get
            {
                int index = this.FindLastIndex(x => x.Key.Equals(key));
                return this.ElementAt(index).Value;
            }
            set
            {
                if(this.Count>0)
                {
                    if(this.Last().Key.Equals(key) == false)
                    {
                        this.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                    else
                    {
                        this.RemoveAt(this.Count - 1);
                        this.Add(new KeyValuePair<TKey, TValue>(key, value));
                    }
                }
                else
                {
                    this.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
                
            }
        }

        public ICollection<TKey> Keys => this.Select(x => x.Key).ToList();

        public ICollection<TValue> Values => this.Select(x => x.Value).ToList();

        public void Add(TKey key, TValue value)
        {
            this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return this.Any(x => x.Key.Equals(key));
        }

        public bool Remove(TKey key)
        {
            for(int i=this.Count-1; i>=0; i--)
            {
                if(this[i].Key.Equals(key) == true)
                {
                    this.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public DictionaryList<TKey, TValue> Clone(TKey key)
        {
            List<KeyValuePair<TKey, TValue>> keyvalues = new List<KeyValuePair<TKey, TValue>>();
            //Dictionary<TKey, TValue> exprs = new Dictionary<TKey, TValue>();
            List<int> indexs = new List<int>();
            for (int i = this.Count-1; i >=0; i--)
            {
                if (this[i].Key.Equals(key))
                {
                    break;
                }
                //if (exprs != null)
                {
                    indexs.Add(i);
                    keyvalues.Insert(0, this.ElementAt(i));
                    //exprs[this.ElementAt(i).Key] = this.ElementAt(i).Value;
                }
            }
            foreach(var oo in indexs)
            {
                this.RemoveAt(oo);
            }

            DictionaryList<TKey, TValue> dd = new DictionaryList<TKey, TValue>();
            foreach (var oo in keyvalues)
            {
                dd.Add(oo.Key, oo.Value);
            }
            return dd;
            //return keyvalues.ToDictionary(x=>x.Key, x=>x.Value);
        }


        //public void Remove(Dictionary<TKey, TValue> datas)
        //{
        //    //foreach (var oo in datas)
        //    //{
        //    //    this.Remove(oo.Key);
        //    //}

        //}

        //public bool ContainsKey(TKey key)
        //{
        //    return this.Any(x => x.Key.Equals(key));
        //}

        //public void Add(TKey key, TValue value)
        //{
        //    throw new NotImplementedException();
        //}

        //bool IDictionary<TKey, TValue>.Remove(TKey key)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool TryGetValue(TKey key, out TValue value)
        //{
        //    throw new NotImplementedException();
        //}

    }
    public class DictionaryList1<TKey, TValue>
    {
        Dictionary<TKey, TValue> m_Dics = new Dictionary<TKey, TValue>();
        List<TKey> m_Keys = new List<TKey>();
        public TValue this[TKey key]
        {
            get
            {
                return this.m_Dics[key];
            }
            set
            {
                if(this.m_Dics.ContainsKey(key) == false)
                {
                    this.m_Keys.Add(key);
                }
                this.m_Dics[key] = value;
            }
        }

        public void Remove(TKey key)
        {
            this.m_Dics.Remove(key);
            this.m_Keys.Remove(key);
        }

        public void Remove(Dictionary<TKey, TValue> datas)
        {
            foreach(var oo in datas)
            {
                this.m_Dics.Remove(oo.Key);
                this.m_Keys.Remove(oo.Key);
            }
            
        }

        public int Count
        {
            get
            {
                return this.m_Keys.Count;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.m_Dics.ContainsKey(key);
        }

        public KeyValuePair<TKey, TValue> ElementAt(int index)
        {
            TKey key = this.m_Keys[index];
            TValue value = this.m_Dics[key];
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public Dictionary<TKey, TValue> Clone(TKey key)
        {
            Dictionary<TKey, TValue> exprs = null;
            for (int i = 0; i < this.Count; i++)
            {
                if (exprs != null)
                {
                    exprs[this.ElementAt(i).Key] = this.ElementAt(i).Value;
                }

                if (ElementAt(i).Key.Equals(key))
                {
                    exprs = new Dictionary<TKey, TValue>();
                }

            }
            return exprs;
        }
    }
}
