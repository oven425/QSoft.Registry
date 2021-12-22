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
                if(index < 0)
                {
                    return default(TValue);
                }
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
            List<int> indexs = new List<int>();
            DictionaryList<TKey, TValue> dd = new DictionaryList<TKey, TValue>();
            if (this.Count == 1)
            {
                if(this[0].Key.Equals(key) == true)
                {
                    dd.Add(this[0].Key, this[0].Value);
                    this.RemoveAt(0);
                }
            }
            else
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    if (this[i].Key.Equals(key))
                    {
                        //if(indexs.Count == 0)
                        //{
                        //    indexs.Add(i);
                        //    keyvalues.Insert(0, this.ElementAt(i));
                        //}
                        break;
                    }
                    indexs.Add(i);
                    keyvalues.Insert(0, this.ElementAt(i));
                }
                foreach (var oo in indexs)
                {
                    this.RemoveAt(oo);
                }

                
                foreach (var oo in keyvalues)
                {
                    dd.Add(oo.Key, oo.Value);
                }
            }
            
            return dd;
        }
    }
}
