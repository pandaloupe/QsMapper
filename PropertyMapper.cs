using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data;

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
            var idColumn = GetIdColumn(data);
            var id = GetIdValue(data, idColumn);
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
            var id = row["Id"];
            var o = new T();

            Map(o, row);
            return o;
        }

        private static int GetIdValue(IDataRecord data, int idColumn)
        {
            if (idColumn < 0)
            {
                return 0;
            }

            var type = data.GetFieldType(idColumn);

            if (type == typeof(int))
            {
                return data.GetInt32(idColumn);
            }

            if (type == typeof(long))
            {
                return (int)data.GetInt64(idColumn);
            }

            if (type == typeof(short))
            {
                return data.GetInt16(idColumn);
            }

            if (type == typeof(byte))
            {
                return data.GetByte(idColumn);
            }

            // not convertible
            throw new Exception("Id is not convertible to int");
        }

        private static int GetIdColumn(IDataRecord data)
        {
            for (var i = 0; i < data.FieldCount; i++)
            {
                if (data.GetName(i) == "Id")
                {
                    return i;
                }
            }

            return -1;
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
                if (columnName == typeof(T).Name + "Id") columnName = "Id";

                if (!_mappedProperties.ContainsKey(columnName))
                {
                    _mappedProperties.Add(columnName, _type.GetProperty(columnName));
                }

                var property = _mappedProperties[columnName];

                var value = data.GetValue(i);
                if (value == DBNull.Value) continue;

                if (columnName.Contains('.'))
                {
                    MapComplexProperty(property, o, columnName, value);
                    continue;
                }

                if (columnName.EndsWith("Id"))
                {
                    var fieldName = columnName.Substring(0, columnName.Length - 2) + ".Id";
                    MapComplexProperty(property, o, fieldName, value);
                    //try to map as property anyway
                }

                if (property?.CanWrite == true)
                {
                    property?.SetValue(o, value, null);
                }
            }
        }

        public void Map(object o, DataRow row)
        {
            var table = row.Table;
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var columnName = table.Columns[i].ColumnName;
                if (columnName == typeof(T).Name + "Id")
                {
                    columnName = "Id";
                }

                if (!_mappedProperties.ContainsKey(columnName))
                {
                    _mappedProperties.Add(columnName, _type.GetProperty(columnName));
                }

                var property = _mappedProperties[columnName];

                var value = row[i];
                if (value == DBNull.Value)
                    continue;

                if (columnName.Contains('.'))
                {
                    MapComplexProperty(property, o, columnName, value);
                    continue;
                }

                if (columnName.EndsWith("Id"))
                {
                    var fieldName = columnName.Substring(0, columnName.Length - 2) + ".Id";
                    MapComplexProperty(property, o, fieldName, value);
                    //try to map as property anyway
                }

                if (property?.CanWrite == true)
                {
                    property?.SetValue(o, value, null);
                }
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

        private void MapComplexProperty(PropertyInfo property, object o, string propertyName, object value)
        {
            var propParts = propertyName.Split('.');
            MapComplexProperty(property, o, propParts, value);
        }

        private void MapComplexProperty(PropertyInfo property, object o, string[] propertyNameParts, object value)
        {
            var propParts = propertyNameParts;
            var propName = propParts[0];
            var childProp = o.GetType().GetProperty(propName);
            if (childProp == null)
            {
                return; //no such property on object
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
                MapComplexProperty(property, propInstance, propParts.Skip(1).ToArray(), value);
                return;
            }

            // final property found
            property = property ?? childProp.PropertyType.GetProperty(propParts[1]);
            if (property == null || !property.CanWrite)
            {
                // property cannot be resolved by name or not readable
                return;
            }

            property.SetValue(propInstance, value, null);
        }
    }
}
