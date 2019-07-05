using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper {
    /// <summary>
    /// Interface for simple caching
    /// </summary>
    public interface IItemProvider<T> {
        /// <summary>
        /// Rerturn current item instance from Cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get(object id);

        /// <summary>
        /// Add or update an item in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Put(T item);

        /// <summary>
        /// Remove an item from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Remove(T item);

        /// <summary>
        /// Return all cached items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Add/update all instances of a given item list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        void PutAll(IEnumerable<T> items);
    }
}
