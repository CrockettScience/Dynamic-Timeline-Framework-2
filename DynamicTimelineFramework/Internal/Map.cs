using System;

namespace DynamicTimelineFramework.Internal
{
    internal class Map<TKey, TValue> {
        private const int DEFAULT_TABLE_SIZE = 101;

        private MapEntry<TKey, TValue>[] _mapTable;
        private int _occupied;
        private int _currentSize;

        public TValue this[TKey key]
        {
            get => _mapTable[FindPos(key)] == null ? default : _mapTable[FindPos(key)].value;

            set => Add(new MapEntry<TKey, TValue>(key, value));
        }

        public Map() {
            Clear();
        }

        public int Size() {
            return _currentSize;
        }

        public bool IsEmpty() {
            return _currentSize == 0;
        }

        public bool ContainsKey(TKey key) {
            if(key == null){
                return false;
            }

            var currentPos = FindPos(key);
            var other = _mapTable[currentPos] == null ? default : _mapTable[currentPos].key;

            return key.Equals(other) && _mapTable[currentPos].IsActive;
        }

        private void Add(MapEntry<TKey, TValue> entry) {
            var currentPos = FindPos(entry.key);

            if (_mapTable[currentPos] == null)
                _occupied++;

            _mapTable[currentPos] = entry;
            _currentSize++;

            if (_occupied > _mapTable.Length / 2)
                Rehash();

        }

        public void Remove(TKey key) {
            var currentPos = FindPos(key);

            if (_mapTable[currentPos] == null || !_mapTable[currentPos].IsActive)
                return;

            _mapTable[currentPos].IsActive = false;
            _currentSize--;
        }

        public void Clear() {
            AllocateArray(DEFAULT_TABLE_SIZE);
            _currentSize = 0;
            _occupied = 0;
        }

        private int FindPos(TKey key) {
            var offset = 1;
            var currentPos = Math.Abs(key.GetHashCode() % _mapTable.Length);

            while (_mapTable[currentPos] != null) {
                if (key.Equals(_mapTable[currentPos].key))
                    break;

                currentPos += offset;
                offset += 2;

                if (currentPos >= _mapTable.Length)
                    currentPos -= _mapTable.Length;
            }

            return currentPos;
        }

        private void Rehash() {
            var oldArray = _mapTable;

            AllocateArray(nextPrime(4 * _currentSize));
            _currentSize = 0;
            _occupied = 0;

            foreach (var entry in oldArray) {
                if (entry != null) {
                    Add(entry);
                }
            }
        }

        private static int nextPrime(int n) {
            if (n % 2 == 0)
                n++;

            for (; !isPrime(n); )
                n += 2;

            return n;
        }

        private static bool isPrime(int n) {
            if (n == 2 || n == 3)
                return true;

            if (n == 1 || n % 2 == 0)
                return false;

            for (var i = 3; i * i <= n; i += 2)
                if (n % i == 0)
                    return false;

            return true;
        }

        private void AllocateArray(int arraySize) {
            _mapTable = new MapEntry<TKey, TValue>[nextPrime(arraySize)];
        }

        private class MapEntry<K, V> {
            public V value;
            public K key;
            public bool IsActive;

            public MapEntry(K k, V v) {
                key = k;
                value = v;
                IsActive = true;
            }
        }
    }
}