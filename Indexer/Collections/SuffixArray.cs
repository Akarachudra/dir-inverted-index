using System;
using System.Collections.Generic;

namespace Indexer.Collections
{
    public class SuffixArray<TKey, TValue>
    {
        private const int MinimumCapacity = 16;
        private readonly IComparer<TKey> insertComparer;
        private readonly IComparer<TKey> readComparer;
        private TKey[] keys;
        private TValue[] values;
        private int size;

        public SuffixArray(IComparer<TKey> insertComparer, IComparer<TKey> readComparer)
            : this(0, insertComparer, readComparer)
        {
        }

        public SuffixArray(int capacity, IComparer<TKey> insertComparer, IComparer<TKey> readComparer)
        {
            this.insertComparer = insertComparer;
            this.readComparer = readComparer;
            this.keys = new TKey[0];
            this.values = new TValue[0];
            this.Capacity = capacity;
        }

        public TKey[] Keys
        {
            get
            {
                var keyArray = new TKey[this.size];
                if (this.size > 0)
                {
                    Array.Copy(this.keys, 0, keyArray, 0, this.size);
                }

                return keyArray;
            }
        }

        public TValue[] Values
        {
            get
            {
                var valueArray = new TValue[this.size];
                if (this.size > 0)
                {
                    Array.Copy(this.values, 0, valueArray, 0, this.size);
                }

                return valueArray;
            }
        }

        public int Capacity
        {
            get => this.keys.Length;

            set
            {
                if (value < this.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Small capacity");
                }

                if (value != this.keys.Length)
                {
                    if (value > 0)
                    {
                        var newKeysArray = new TKey[value];
                        var newValuesArray = new TValue[value];
                        if (this.size > 0)
                        {
                            Array.Copy(this.keys, 0, newKeysArray, 0, this.size);
                            Array.Copy(this.values, 0, newValuesArray, 0, this.size);
                        }

                        this.keys = newKeysArray;
                        this.values = newValuesArray;
                    }
                    else
                    {
                        this.keys = new TKey[0];
                        this.values = new TValue[0];
                    }
                }
            }
        }

        public int Count => this.size;

        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = Array.BinarySearch(this.keys, 0, this.size, key, this.readComparer);
            if (index >= 0)
            {
                value = this.values[index];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var index = Array.BinarySearch(this.keys, 0, this.size, key, this.insertComparer);
            if (index >= 0)
            {
                return false;
            }

            index = ~index;

            if (this.size == this.keys.Length)
            {
                this.EnsureCapacity(this.size + 1);
            }

            if (index < this.size)
            {
                Array.Copy(this.keys, index, this.keys, index + 1, this.size - index);
                Array.Copy(this.values, index, this.values, index + 1, this.size - index);
            }

            this.size++;
            this.keys[index] = key;
            this.values[index] = value;

            return true;
        }

        private void EnsureCapacity(int minimumSize)
        {
            var newCapacity = this.keys.Length == 0 ? MinimumCapacity : this.keys.Length * 2;
            if ((uint)newCapacity > int.MaxValue)
            {
                newCapacity = int.MaxValue;
            }

            if (newCapacity < minimumSize)
            {
                newCapacity = minimumSize;
            }

            this.Capacity = newCapacity;
        }
    }
}