using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Net.Arqsoft.QsMapper.Model;
using Net.Arqsoft.QsMapper.QueryBuilder;

namespace Net.Arqsoft.QsMapper.Caching
{
    /// <summary>
    /// Caches items from default item query and tracks updates.
    /// Depends on triggers or other logging 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityCache<T> : IEntityCache<T> where T : IntegerBasedEntity, new()
    {
        private DateTime _internalCacheTime;
        private DateTime _cacheTime;
        private IDictionary<int, T> _items;
        private readonly IGenericDao _dao;

        /// <summary>
        /// supresses cache requests if the last update has taken place
        ///less than ... milliseconds ago
        /// </summary>
        public int CacheLatency { get; set; }

        /// <summary>
        /// will override latency and refresh anyhow
        /// </summary>
        public bool DataChanged { get; set; }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="dao">DAO instance</param>
        public EntityCache(IGenericDao dao)
        {
            _dao = dao;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dao">DAO instance</param>
        /// <param name="cacheLatency">time in ms cache should not be refreshed</param>
        public EntityCache(IGenericDao dao, int cacheLatency) : this(dao)
        {
            CacheLatency = cacheLatency;
        }

        ///<summary>
        /// Gets the currently cached item list.
        /// Cache is loaded/updated automatically in advance.
        ///</summary>
        public IList<T> Items
        {
            get
            {
                if (_items == null)
                {
                    InitCache();
                }
                else
                {
                    RefreshCache();
                }

                return _items.Values.ToList();
            }
        }

        private void LoadItems(IList<T> list)
        {
            _items = _items ?? new Dictionary<int, T>();
            foreach (var item in list)
            {
                if (_items.ContainsKey(item.Id))
                {
                    _items[item.Id].UpdateFrom(item);
                }
                else
                {
                    _items.Add(item.Id, item);
                }
            }
        }

        /// <summary>
        /// Returns item with a specific id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get(int id)
        {
            return _items.ContainsKey(id)
                ? _items[id]
                : null;
        }

        public IList<T> GetAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initialize, loading all items from database
        /// </summary>
        public void InitCache()
        {
            _cacheTime = _dao.GetCurrentDate();
            var list = _dao.Query<T>().ToList();
            LoadItems(list);
            _internalCacheTime = DateTime.Now;
            Debug.Print("EntityCache.InitCache : {0:#,##0} Objects of Type {1} cached.", _items.Count, typeof(T).Name);
        }

        /// <summary>
        /// Initialize, loading all items from database using an additional expression
        /// </summary>
        public void InitCache(Func<QueryParameter, QueryParameter> paramFunction)
        {
            _cacheTime = _dao.GetCurrentDate();
            var list = _dao.Query<T>()
                .Where(paramFunction)
                .ToList();

            LoadItems(list);
            _internalCacheTime = DateTime.Now;
            Debug.Print("EntityCache.InitCache : {0:#,##0} Objects of Type {1} cached.", _items.Count, typeof(T).Name);
        }

        /// <summary>
        /// Initialize using a list of items
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cacheTime"></param>
        public void InitCache(IList<T> list, DateTime? cacheTime = null)
        {
            _cacheTime = cacheTime ?? _dao.GetCurrentDate();
            LoadItems(list);
        }

        /// <summary>
        /// Replace cached items with new list
        /// </summary>
        /// <param name="list"></param>
        public void AddToCache(IList<T> list)
        {
            _cacheTime = _dao.GetCurrentDate();
            LoadItems(list);
        }

        /// <summary>
        /// Add or replace an item in the cache
        /// </summary>
        /// <param name="item"></param>
        public void UpdateItem(T item)
        {
            if (_items.ContainsKey(item.Id))
            {
                _items[item.Id].UpdateFrom(item);
            }
            else
            {
                _items.Add(item.Id, item);
            }
        }

        /// <summary>
        /// Add or replace an item in the cache
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(T item)
        {
            if (_items.ContainsKey(item.Id))
            {
                _items.Remove(item.Id);
            }
        }

        ///<summary>
        /// Update items that have change since last cache operation.
        /// Depends on LastChange field being part of the source query.
        ///</summary>
        public void RefreshCache(bool skipCheck = false)
        {
            if (skipCheck)
            {
                return;
            }

            if (!DataChanged && DateTime.Now.Subtract(_internalCacheTime).TotalMilliseconds < CacheLatency)
            {
                return;
            }

            DataChanged = false;

            var time = _dao.GetCurrentDate();
            var changedItems = _dao.Query<T>()
                .Where(x => x.Field("LastChange").IsGreaterThan(_cacheTime))
                .Or(x => x.Field("ReferenceChanged").IsGreaterThan(_cacheTime))
                .ToList();

            if (changedItems.Count > 0)
            {
                foreach (var i in changedItems)
                {
                    var updatedItem = i;
                    if (!_items.ContainsKey(updatedItem.Id) && !updatedItem.IsDeleted)
                    {
                        _items.Add(updatedItem.Id, updatedItem);
                    }
                    else if (_items.ContainsKey(updatedItem.Id))
                    {
                        _items[updatedItem.Id].UpdateFrom(updatedItem);
                    }
                }
            }

            _cacheTime = time;
            _internalCacheTime = DateTime.Now;
        }
    }
}
