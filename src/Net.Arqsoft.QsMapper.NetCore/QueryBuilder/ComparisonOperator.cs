namespace Net.Arqsoft.QsMapper.QueryBuilder;

/// <summary>
/// Defines operators to be used with the QueryBuilder
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// compare values using equality (=) operator
    /// </summary>
    Equal,

    /// <summary>
    /// compare strings using * as wildcard
    /// </summary>
    Like,

    /// <summary>
    /// compare numbers and dates using > operator
    /// </summary>
    Greater,

    /// <summary>
    /// combination of equal and greater
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// compare values using &lt; operator
    /// </summary>
    Less,

    /// <summary>
    /// compare values using &lt;= operator
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// compare to a range of values
    /// </summary>
    Between,

    /// <summary>
    /// compare to a collection of values
    /// </summary>
    In,

    /// <summary>
    /// compare to DBNull
    /// </summary>
    IsNull,

    /// <summary>
    /// Compare to DBTrue (1)
    /// </summary>
    True,

    /// <summary>
    /// Compare to DBFalse (0)
    /// </summary>
    False
}