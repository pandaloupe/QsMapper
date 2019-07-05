using System;
using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper
{
    internal class ItemProvider<T> : IItemProvider<T> where T : class
    {
        private readonly IDictionary<object, T> _cache;
        private readonly Func<T, object> _getId;
        public ItemProvider(Func<T, object> idMethod)
        {
            _getId = idMethod;
            _cache = new Dictionary<object, T>();
        }

        public T Get(object id)
        {
            // if type is not cached return new instance
            return _cache.ContainsKey(id)
                ? _cache[id]
                : null;
        }

        public void Put(T item)
        {
            var id = _getId(item);
            if (_cache.ContainsKey(id))
            {
                _cache[id] = item;
                return;
            }

            _cache.Add(id, item);
        }

        public void Remove(T item)
        {
            var id = _getId(item);
            if (!_cache.ContainsKey(id))
            {
                return;
            }

            _cache.Remove(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _cache.Values;
        }

        public void PutAll(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Put(item);
            }
        }
    }
}
