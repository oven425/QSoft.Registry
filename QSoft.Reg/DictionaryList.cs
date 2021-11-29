using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Registry
{
    public class DictionaryList<TKey, TValue>
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
