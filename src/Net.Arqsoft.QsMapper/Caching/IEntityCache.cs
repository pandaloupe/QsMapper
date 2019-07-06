using System.Collections.Generic;
using Net.Arqsoft.QsMapper.Model;

namespace Net.Arqsoft.QsMapper.Caching
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntityCache<T> where T : IntegerBasedEntity, new()
    {
        /// <summary>
        /// 
        /// </summary>
        int CacheLatency { get; set; }

        /// <summary>
        /// Returns item with a specific id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>existing item or new instance with id</returns>
        T Get(int id);

        /// <summary>
        /// Returns all items
        /// </summary>
        /// <returns></returns>
        IList<T> Items { get; }

        /// <summary>
        /// Updates a cached item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void UpdateItem(T item);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void RemoveItem(T item);
    }
}