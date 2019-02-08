using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Betlln.Collections
{
    public class KeyValueBag<TValue>
        : IEnumerable<KeyValuePair<string, TValue>>
    {
        private readonly List<KeyValuePair<string, TValue>> _valueStore;

        public KeyValueBag()
        {
            _valueStore = new List<KeyValuePair<string, TValue>>();
        }

        public IEnumerable<string> Keys
        {
            get { return _valueStore.Select(x => x.Key); }
        }

        public IEnumerable<TValue> Values
        {
            get { return _valueStore.Select(x => x.Value); }
        }

        public uint Count
        {
            get { return (uint) _valueStore.Count; }
        }

        public bool ContainsKey(string keyName)
        {
            return FindCurrentIndex(keyName) != -1;
        }

        public void Add(string keyName, TValue value)
        {
            keyName = NormalizeKeyName(keyName);
            int currentIndex = FindCurrentIndex(keyName);

            KeyValuePair<string, TValue> item = new KeyValuePair<string, TValue>(keyName, value);
            if (currentIndex == -1)
            {
                _valueStore.Add(item);
            }
            else
            {
                _valueStore[currentIndex] = item;
            }
        }

        public TValue this[string keyName]
        {
            get
            {
                int currentIndex = FindCurrentIndex(keyName);
                if (currentIndex == -1)
                {
                    throw new KeyNotFoundException($"The key \'{keyName}\' does not exist.");
                }
                return _valueStore[currentIndex].Value;
            }
        }

        public bool TryGetValue(string keyName, out TValue value)
        {
            int index = FindCurrentIndex(keyName);
            if (index >= 0)
            {
                value = _valueStore[index].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        private int FindCurrentIndex(string keyName)
        {
            keyName = NormalizeKeyName(keyName);
            return _valueStore.FindIndex(x => x.Key.Equals(keyName, StringComparison.CurrentCultureIgnoreCase));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _valueStore.GetEnumerator();
        }

        public void Remove(string keyName)
        {
            int index = FindCurrentIndex(keyName);
            if (index >= 0)
            {
                _valueStore.RemoveAt(index);
            }
        }

        private static string NormalizeKeyName(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentNullException(nameof(keyName));
            }

            return keyName.Trim();
        }
    }
}