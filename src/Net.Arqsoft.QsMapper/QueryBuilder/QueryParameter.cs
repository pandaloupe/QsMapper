using System;
using System.Collections;
using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper.QueryBuilder
{
    public class QueryParameter
    {
        public QueryParameter()
        {
            Operator = ComparisonOperator.Equal;
        }

        /// <summary>
        /// Field to evaluate
        /// </summary>
        public string FieldName { get; set; }
        public ComparisonOperator Operator { get; set; }
        /// <summary>
        /// Value to compare with
        /// </summary>
        public object CompareTo { get; set; }
        /// <summary>
        /// invert the operator using "not"
        /// </summary>
        public bool Invert { get; set; }
        /// <summary>
        /// uses "or" operator instead of default "and" to concatenate
        /// conditions on the same level
        /// </summary>
        public bool IsOredCondition { get; set; }


        private IList<QueryParameter> _additionalParameters;
        public IList<QueryParameter> AdditionalParameters
        {
            get { return _additionalParameters ?? (_additionalParameters = new List<QueryParameter>()); }
            set { _additionalParameters = value; }
        }

        private void AddQueryParameter(QueryParameter param)
        {
            AdditionalParameters.Add(param);
        }

        //Fluent catalog
        public QueryParameter Field(string fieldname)
        {
            FieldName = fieldname;
            return this;
        }

        public QueryParameter Not
        {
            get
            {
                Invert = true;
                return this;
            }
        }

        public QueryParameter IsEqualTo(object value)
        {
            Operator = ComparisonOperator.Equal;
            CompareTo = value;
            return this;
        }

        public QueryParameter Contains(object value)
        {
            Operator = ComparisonOperator.Like;
            CompareTo = $"%{value}%";
            return this;
        }

        public QueryParameter IsGreaterThan(object value)
        {
            Operator = ComparisonOperator.Greater;
            CompareTo = value;
            return this;
        }
        public QueryParameter IsGreaterOrEqual(object value)
        {
            Operator = ComparisonOperator.GreaterOrEqual;
            CompareTo = value;
            return this;
        }
        public QueryParameter IsLessThan(object value)
        {
            Operator = ComparisonOperator.Less;
            CompareTo = value;
            return this;
        }
        public QueryParameter IsLessOrEqual(object value)
        {
            Operator = ComparisonOperator.LessOrEqual;
            CompareTo = value;
            return this;
        }
        public QueryParameter IsLike(object value)
        {
            Operator = ComparisonOperator.Like;
            CompareTo = value;
            return this;
        }

        /// <summary>
        /// Implements logic for sql in operator.
        /// If Elements in value enumeration are of type IntegerBasedEntity, it's Ids are used for comparison.
        /// </summary>
        /// <param name="value">IEnumerable of possible values.</param>
        /// <returns>self (for fluent syntax)</returns>
        public QueryParameter IsIn(IEnumerable value)
        {
            Operator = ComparisonOperator.In;

            CompareTo = value;
            return this;
        }

        /// <summary>
        /// Implements logic for sql in operator.
        /// </summary>
        /// <param name="values">List of possible values.</param>
        /// <returns>self (for fluent syntax)</returns>
        public QueryParameter IsIn(params object[] values)
        {
            Operator = ComparisonOperator.In;

            CompareTo = values;

            return this;
        }

        public QueryParameter IsNull()
        {
            Operator = ComparisonOperator.IsNull;
            return this;
        }

        public RangePart IsBetween(object value)
        {
            Operator = ComparisonOperator.Between;
            return new RangePart(this, value);
        }

        public QueryParameter IsTrue()
        {
            Operator = ComparisonOperator.True;
            return this;
        }

        public QueryParameter IsFalse()
        {
            Operator = ComparisonOperator.False;
            return this;
        }
        public QueryParameter And(Func<QueryParameter, QueryParameter> condition)
        {
            var param = new QueryParameter();
            AddQueryParameter(condition(param));
            return this;
        }
        public QueryParameter Or(Func<QueryParameter, QueryParameter> condition)
        {
            var param = new QueryParameter { IsOredCondition = true };
            AddQueryParameter(condition(param));
            return this;
        }

        public string GetOperator()
        {
            switch (Operator)
            {
                case ComparisonOperator.Greater:
                    return Invert ? "<=" : ">";
                case ComparisonOperator.GreaterOrEqual:
                    return Invert ? "<" : ">=";
                case ComparisonOperator.Less:
                    return Invert ? ">=" : "<";
                case ComparisonOperator.LessOrEqual:
                    return Invert ? ">" : "<=";
                case ComparisonOperator.In:
                    return Invert ? "not in" : "in";
                case ComparisonOperator.Like:
                    return Invert ? "not like" : "like";
                case ComparisonOperator.IsNull:
                    return Invert ? "is not null" : "is null";
                case ComparisonOperator.Between:
                    return Invert ? "not between" : "between";
                default: //Equal
                    return Invert ? "!=" : "=";
            }
        }
    }
}
