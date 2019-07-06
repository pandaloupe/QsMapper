using System;
using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper
{
    /// <summary>
    /// Base Catalog returning a default TableMap if no specific map is registered for a type.
    /// Specific TableMaps may be specified in the constructor.
    /// </summary>
    public class Catalog : ICatalog
    {
        private readonly IDictionary<Type, object> _tableMaps;
        private readonly IDictionary<Type, object> _propertyMappers;

        /// <summary>
        /// Add specific TableMaps to own constructor.
        /// </summary>
        public Catalog()
        {
            _tableMaps = new Dictionary<Type, object>();
            _propertyMappers = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Used by GenericDao to determine the TableMap to use with a specific Type.
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>Specific Map if defined in Constructor or default TableMap.</returns>
        public TableMap<T> GetTableMap<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!_tableMaps.ContainsKey(type))
            {
                var map = new TableMap<T>();
                _tableMaps.Add(type, map);
                map.Catalog = this;
                return map;
            }

            var result = (TableMap<T>)_tableMaps[type];
            result.Catalog = this;
            return result;
        }

        /// <summary>
        /// Get a specific property mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PropertyMapper<T> GetPropertyMapper<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!_propertyMappers.ContainsKey(type))
            {
                var mapper = new PropertyMapper<T>();
                _propertyMappers.Add(type, mapper);
                return mapper;
            }

            return (PropertyMapper<T>)_propertyMappers[type];
        }

        /// <summary>
        /// Use this method to register any specific TableMaps.
        /// </summary>
        /// <typeparam name="T">Mapped Type of TableMap&lt;T&gt;</typeparam>
        public TableMap<T> RegisterMap<T>() where T : class, new()
        {
            var map = new TableMap<T>();
            _tableMaps.Add(typeof(T), map);
            return map;
        }
    }
}
