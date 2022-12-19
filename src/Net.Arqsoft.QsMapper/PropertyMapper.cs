using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;
using log4net;

namespace Net.Arqsoft.QsMapper
{
    /// <summary>
    /// Copies database values to object properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyMapper<T> where T : new()
    {
        private readonly IDictionary<string, PropertyInfo> _mappedProperties = new Dictionary<string, PropertyInfo>();
        private readonly Type _type = typeof(T);
        private readonly ICatalog _catalog;
        private readonly ILog _log = LogManager.GetLogger(typeof(PropertyMapper<>));

        /// <summary>
        /// Create a new Property Mapper for the current catalog.
        /// </summary>
        /// <param name="catalog"></param>
        public PropertyMapper(ICatalog catalog)
        {
            _catalog = catalog;
        }

        /// <summary>
        /// Copy values from datareader to new object.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public T Map(IDataReader data)
        {
            var o = new T();

            Map(o, data);
            return o;
        }

        /// <summary>
        /// Copy values from datareader to new object.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public T Map(DataRow row)
        {
            var o = new T();

            Map(o, row);
            return o;
        }

        /// <summary>
        /// Copy values from datareader to existing object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="data"></param>
        public void Map(object o, IDataRecord data)
        {
            for (var i = 0; i < data.FieldCount; i++)
            {
                var columnName = data.GetName(i);
                var value = data.GetValue(i);

                try
                {
                    Map(o, columnName, value);
                }
                catch (Exception ex)
                {
                    _log.Error($"Property mapping failed for column '{columnName}', value '{value}'.", ex);
                }
            }
        }

        private void Map(object o, string columnName, object value)
        {
            if (columnName == typeof(T).Name + "Id") columnName = "Id";

            var isSameType = o.GetType() == _type;

            if (isSameType)
            {
                if (!_mappedProperties.ContainsKey(columnName))
                {
                    _mappedProperties.Add(columnName, _type.GetProperty(columnName));
                }
            }

            var property = isSameType
                ? _mappedProperties[columnName]
                : o.GetType().GetProperty(columnName);

            if (value == DBNull.Value || value == null)
            {
                return;
            }

            if (columnName.Contains('.'))
            {
                if (MapComplexProperty(property, o, columnName, value))
                {
                    return;
                }
            }

            if (columnName.EndsWith("Id"))
            {
                var fieldName = columnName.Substring(0, columnName.Length - 2) + ".Id";
                if (MapComplexProperty(property, o, fieldName, value))
                {
                    return;
                }
            }

            if (property == null && MapperSettings.LogUnmappedProperties)
            {
                _log.Warn($"Property '{columnName}' could not be resolved on object of type {o.GetType().FullName}.");
                return;
            }

            if (property?.CanWrite == true)
            {
                property.SetValue(o, value, null);
            }
            else if (MapperSettings.LogUnmappedProperties)
            {
                _log.Warn($"Property '{columnName}' on object of type {o.GetType().FullName} is read only.");
                return;
            }
        }

        public void Map(object o, DataRow row)
        {
            var table = row.Table;
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var columnName = table.Columns[i].ColumnName;
                var value = row[i];
                Map(o, columnName, value);
            }
        }

        /// <summary>
        /// Generate an object list from a data reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IList<T> MapAll(IDataReader reader)
        {
            var result = new List<T>();
            while (reader.Read())
            {
                result.Add(Map(reader));
            }

            reader.Close();
            return result;
        }

        /// <summary>
        /// Generate an object list from a data table.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IList<T> MapAll(DataTable table)
        {
            var result = new List<T>();
            for (var row = 0; row < table.Rows.Count; row++)
            {
                result.Add(Map(table.Rows[row]));
            }

            return result;
        }

        private bool MapComplexProperty(PropertyInfo property, object o, string propertyName, object value)
        {
            var propParts = propertyName.Split('.');
            return MapComplexProperty(property, o, propParts, value);
        }

        private bool MapComplexProperty(PropertyInfo property, object o, string[] propertyNameParts, object value)
        {
            var propParts = propertyNameParts;
            var propName = propParts[0];
            var childProp = o.GetType().GetProperty(propName);
            if (childProp == null)
            {
                return false; //no such property on object
            }

            // create instance for object type property if not present
            if (childProp.GetValue(o, null) == null)
            {
                var prop = Activator.CreateInstance(childProp.PropertyType);

                childProp.SetValue(o, prop, null);
            }

            var propInstance = childProp.GetValue(o, null);
            if (propParts.Length > 2)
            {
                return MapComplexProperty(property, propInstance, propParts.Skip(1).ToArray(), value);
            }

            // final property found
            property = property ?? childProp.PropertyType.GetProperty(propParts[1]);
            if (property == null)
            {
                // property cannot be resolved by name or not readable
                return false;
            }

            if (property.CanWrite)
            {
                property.SetValue(propInstance, value, null);
            }

            return true;
        }
    }
}