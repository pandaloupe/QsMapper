namespace Net.Arqsoft.QsMapper.QueryBuilder;

/// <summary>
/// Implements order by clause.
/// </summary>
public class OrderParameter
{
    /// <summary>
    /// Constructor
    /// </summary>
    public OrderParameter() { }

    /// <summary>
    /// Constructor with fieldname
    /// </summary>
    /// <param name="fieldName"></param>
    public OrderParameter(string fieldName)
    {
        FieldName = fieldName;
    }

    /// <summary>
    /// Constructor with fieldname and direction
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="descending"></param>
    public OrderParameter(string fieldName, bool descending) : this(fieldName)
    {
        SortDescending = descending;
    }

    /// <summary>
    /// Name of database column to order by
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Sort direction
    /// </summary>
    public bool SortDescending { get; set; }
}