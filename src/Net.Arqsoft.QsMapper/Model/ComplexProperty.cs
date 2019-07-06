using System;
using System.Linq.Expressions;
using Net.Arqsoft.QsMapper.Util;

namespace Net.Arqsoft.QsMapper.Model
{
    /// <summary>
    /// Definition for an object that is mapped from a table field (as in n:1 relations).
    /// </summary>
    /// <typeparam name="T">Child class</typeparam>
    /// <typeparam name="T1">Parent class</typeparam>
    public class ComplexProperty<T, T1> : IComplexProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="field"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="fieldExpression"></param>
        public ComplexProperty(
            string field,
            Expression<Func<T, T1>> propertyExpression,
            Expression<Func<T1, object>> fieldExpression
        )
        {
            FieldName = field;
            PropertyExpression = propertyExpression;
            FieldExpression = fieldExpression;
        }

        /// <summary>
        /// Field name holding the parent reference in table.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Expression returning the parent property.
        /// </summary>
        public Expression<Func<T, T1>> PropertyExpression { get; set; }

        /// <summary>
        /// Expression returning the field in parent class.
        /// </summary>
        public Expression<Func<T1, object>> FieldExpression { get; set; }

        #region IComplexProperty Members

        /// <summary>
        /// Return object property from child class.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public object GetProperty(object item)
        {
            try
            {
                var typedItem = (T)item;
                var func = PropertyExpression.Compile();
                return func(typedItem);
            }
            catch (InvalidCastException)
            {
                var propName = ExpressionHelper.GetPropertyName(PropertyExpression);
                var property = item.GetType().GetProperty(propName);
                if (property == null) throw new InvalidCastException($"Member {propName} not present on source.");
                return property.GetValue(item, null);
            }
        }

        /// <summary>
        /// Get field value from parent (such as Person.Company.Name).
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public object GetValue(object item)
        {
            var property = GetProperty(item);
            if (property == null) return null;
            var func = FieldExpression.Compile();
            return func((T1)property);
        }

        #endregion
    }
}
