using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class DefaultDictionary<T1, T2> : IDictionary<T1,T2>
    {
        private Dictionary<T1, T2> Core;

        public static implicit operator DefaultDictionary<T1, T2>(Dictionary<T1, T2> core)
        {
            return new DefaultDictionary<T1, T2>(core);
        }

        public static implicit operator Dictionary<T1, T2>(DefaultDictionary<T1, T2> wrapper)
        {
            return wrapper.Core;
        }

        public DefaultDictionary(Dictionary<T1,T2> core)
        {
            this.Core = core;
        }

        public DefaultDictionary()
        {
            this.Core = new Dictionary<T1, T2>();
        }

        public void Add(T1 key, T2 value)
        {
            Core.Add(key, value);
        }

        public bool ContainsKey(T1 key)
        {
            return Core.ContainsKey(key);
        }

        public ICollection<T1> Keys
        {
            get { return Core.Keys; }
        }

        public bool Remove(T1 key)
        {
            return Core.Remove(key);
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return Core.TryGetValue(key, out value);
        }

        public ICollection<T2> Values
        {
            get { return Core.Values; }
        }

        public T2 this[T1 key]
        {
            get
            {
                if (Core.ContainsKey(key))
                    return Core[key];
                return default(T2);
            }
            set
            {
                Core[key] = value;
            }
        }

        public void Add(KeyValuePair<T1, T2> item)
        {
            Core.Add(item.Key,item.Value);
        }

        public void Clear()
        {
            Core.Clear();
        }

        public bool Contains(KeyValuePair<T1, T2> item)
        {
            return Core.Contains(item);
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Core.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<T1, T2> item)
        {
            return Core.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return Core.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Core.GetEnumerator();
        }
    }
}
