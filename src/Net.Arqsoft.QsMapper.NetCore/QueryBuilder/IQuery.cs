namespace Net.Arqsoft.QsMapper.QueryBuilder;

/// <summary>
/// Interface for building up database queries
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IQuery<T> where T : class, new()
{
    /// <summary>
    /// Reduces the result to a certain number of records.
    /// </summary>
    /// <param name="i">Number of records to be returned.</param>
    /// <returns></returns>
    IQuery<T> Take(int i);

    /// <summary>
    /// Defines the table or view name to be used
    /// </summary>
    /// <param name="viewName"></param>
    /// <returns></returns>
    IQuery<T> From(string viewName);

    /// <summary>
    /// Defines the schema and table/view name to be used
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="viewName"></param>
    /// <returns></returns>
    IQuery<T> From(string schemaName, string viewName);

    /// <summary>
    /// Defines a query expression to be used on the view
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    IQuery<T> Where(Func<QueryParameter, QueryParameter> condition);

    /// <summary>
    /// Adds a query expression using a logical and
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    IQuery<T> And(Func<QueryParameter, QueryParameter> condition);

    /// <summary>
    /// Adds a query expression using a logicl or
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    IQuery<T> Or(Func<QueryParameter, QueryParameter> condition);

    /// <summary>
    /// Defines the primary order column name
    /// </summary>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    IQuery<T> OrderBy(string fieldname);

    /// <summary>
    /// Defines subsequent order column names
    /// </summary>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    IQuery<T> ThenBy(string fieldname);

    /// <summary>
    /// Forces descending order on the preceeding OrderBy or ThenBy column
    /// </summary>
    IQuery<T> Descending { get; }

    /// <summary>
    /// Forces ascending (default) order on the preceeding OrderBy or ThenBy column
    /// </summary>
    IQuery<T> Ascending { get; }

    /// <summary>
    /// Returns the first element of query result or null (Default not implemented)
    /// </summary>
    /// <returns></returns>
    T FirstOrDefault();

    /// <summary>
    /// Shorthand for Where(condition).FirstOrDefault();
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    T FirstOrDefault(Func<QueryParameter, QueryParameter> condition);

    /// <summary>
    /// Execute query and return mapped result
    /// </summary>
    /// <returns></returns>
    IList<T> ToList();

    /// <summary>
    /// Execute query, map to objects and attach all HasMany associations
    /// </summary>
    /// <returns></returns>
    IList<T> ToExtendedList();

    /// <summary>
    /// Execute query and return number of founc records
    /// </summary>
    /// <returns></returns>
    int Count();

    /// <summary>
    /// Get result and convert members explicitly to inherited class
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    IList<T1> ToListOf<T1>() where T1 : class;
}