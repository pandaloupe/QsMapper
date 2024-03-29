﻿using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Text;
using log4net;
using Net.Arqsoft.QsMapper.Model;

namespace Net.Arqsoft.QsMapper.QueryBuilder;

/// <summary>
/// Common implementation for generic object queries.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseQuery<T> : IQuery<T> where T : class, new()
{

    private string? _baseQuery;
    private int _top;
    private OrderParameter? _currentOrderParameter;
    private readonly ILog _log = LogManager.GetLogger(typeof(BaseQuery<T>));

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="catalog"></param>
    public BaseQuery(ICatalog catalog)
    {
        _catalog = catalog;
        TableMap = catalog.GetTableMap<T>();
        PropertyMapper = catalog.GetPropertyMapper<T>();
    }

    private readonly ICatalog _catalog;

    /// <summary>
    /// Dao implementation.
    /// </summary>
    public IGenericDao? Dao { get; set; }

    /// <summary>
    /// Table map to be used.
    /// </summary>
    public TableMap<T> TableMap { get; set; }

    /// <summary>
    /// Property Mapper to be used.
    /// </summary>
    protected readonly PropertyMapper<T> PropertyMapper;

    private IList<QueryParameter>? _queryParameters;

    /// <summary>
    /// List of query parameters.
    /// </summary>
    public IList<QueryParameter>? QueryParameters
    {
        get => _queryParameters ??= new List<QueryParameter>();
        set => _queryParameters = value;
    }

    private IList<OrderParameter?>? _orderParameters;

    /// <summary>
    /// List of order parameters.
    /// </summary>
    public IList<OrderParameter?>? OrderParameters
    {
        get => _orderParameters ??= new List<OrderParameter?>();
        set => _orderParameters = value;
    }

    /// <summary>
    /// Build a new query instance.
    /// </summary>
    /// <returns></returns>
    public IQuery<T> NewQuery()
    {
        return NewQuery(null);
    }

    /// <summary>
    /// Build a new query instance from a certain table or view.
    /// </summary>
    /// <param name="tableOrView"></param>
    /// <returns></returns>
    public IQuery<T> NewQuery(string? tableOrView)
    {
        if (!string.IsNullOrEmpty(tableOrView))
        {
            From(tableOrView);
        }
        else
        {
            _baseQuery = null;
        }

        _top = 0;
        QueryParameters = null;
        OrderParameters = null;
        return this;
    }

    #region Fluent catalog methods

    public IQuery<T> Take(int i)
    {
        _top = i;
        return this;
    }

    /// <summary>
    /// Table or view to use.
    /// </summary>
    /// <param name="viewName"></param>
    /// <returns></returns>
    public IQuery<T> From(string viewName)
    {
        _baseQuery = "select * from " + viewName;
        return this;
    }

    /// <summary>
    /// Table or view to use (including schema name).
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="viewName"></param>
    /// <returns></returns>
    public IQuery<T> From(string schemaName, string viewName)
    {
        _baseQuery = $"select * from [{schemaName}].[{viewName}]";
        return this;
    }

    /// <summary>
    /// Add main condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public IQuery<T> Where(Func<QueryParameter, QueryParameter> condition)
    {
        QueryParameters = null;
        var param = new QueryParameter();
        AddQueryParameter(condition(param));
        return this;
    }

    /// <summary>
    /// Add a condition using logical and.
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public IQuery<T> And(Func<QueryParameter, QueryParameter> condition)
    {
        var param = new QueryParameter();
        AddQueryParameter(condition(param));
        return this;
    }

    /// <summary>
    /// Add a condition using logical or.
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public IQuery<T> Or(Func<QueryParameter, QueryParameter> condition)
    {
        var param = new QueryParameter { IsOredCondition = true };
        AddQueryParameter(condition(param));
        return this;
    }

    /// <summary>
    /// Add an ordering condition by fieldname.
    /// </summary>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    public IQuery<T> OrderBy(string fieldname)
    {
        _currentOrderParameter = new OrderParameter(fieldname);
        AddOrderParameter(_currentOrderParameter);
        return this;
    }

    /// <summary>
    /// Add a successive ordering condition.
    /// </summary>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    public IQuery<T> ThenBy(string fieldname)
    {
        return OrderBy(fieldname);
    }

    /// <summary>
    /// Invert the latest added ordering condition.
    /// </summary>
    public IQuery<T> Descending
    {
        get
        {
            _currentOrderParameter!.SortDescending = true;
            //prevent settings order direction anew
            _currentOrderParameter = null;
            return this;
        }
    }

    /// <summary>
    /// Sort ascending by latest added condition (default).
    /// </summary>
    public IQuery<T> Ascending
    {
        get
        {
            _currentOrderParameter!.SortDescending = false;
            //prevent settings order direction anew
            _currentOrderParameter = null;
            return this;
        }
    }

    #endregion

    private void AddOrderParameter(OrderParameter? param)
    {
        OrderParameters ??= new List<OrderParameter?>();
        OrderParameters.Add(param);
    }
    private void AddQueryParameter(QueryParameter param)
    {
        QueryParameters ??= new List<QueryParameter>();
        QueryParameters.Add(param);
    }

    private SqlCommand GetQueryCommand()
    {
        var cmd = Dao!.GetSqlCommand();
        var queryText = GetShortQuery();
        if (QueryParameters?.Count > 0)
        {
            var condition = GetConditionString(cmd);
            queryText += "\nwhere " + condition;
        }

        queryText += GetOrderByClause();
        cmd.CommandText = queryText;
        return cmd;
    }

    /// <inheritdoc />
    /// <summary>
    /// Returns query result as IList of queries Class
    /// </summary>
    /// <returns></returns>
    public virtual IList<T> ToList()
    {
        using var cmd = GetQueryCommand();
        return GetFindResult(cmd);
    }

    /// <inheritdoc />
    /// <summary>
    /// Returns query result as IList of inherited Class
    /// </summary>
    /// <returns></returns>
    public virtual IList<T1> ToListOf<T1>() where T1 : class
    {
        using var cmd = GetQueryCommand();
        var result = GetFindResult(cmd);
        return result.Select(x => x as T1).ToList()!;
    }

    /// <summary>
    /// Returns query result as IList of queries Class
    /// including ManyToMany and OneToMany relations
    /// </summary>
    /// <returns></returns>
    public virtual IList<T> ToExtendedList()
    {
        var result = ToList();
        foreach (var r in result)
        {
            ResolveOneWayRelations(r);
            ResolveTwoWayRelations(r);
        }

        return result;
    }

    /// <summary>
    /// Get count of objects that will be returned from query (without mapping).
    /// </summary>
    /// <returns></returns>
    public virtual int Count()
    {
        using var cmd = Dao!.GetSqlCommand();
        var queryText = GetCountQuery();
        if (QueryParameters?.Count > 0)
        {
            var condition = GetConditionString(cmd);
            queryText += "\nwhere " + condition;
        }

        queryText += GetOrderByClause();
        cmd.CommandText = queryText;
        using var reader = CommandRunner.Run(cmd, x => x.ExecuteReader());
        reader.Read();
        var result = reader.GetInt32(0);
        reader.Close();
        return result;
    }

    /// <summary>
    /// Get first object (or null).
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public virtual T? FirstOrDefault(Func<QueryParameter, QueryParameter> condition)
    {
        return Where(condition).FirstOrDefault();
    }

    /// <summary>
    /// Get first object (or null).
    /// </summary>
    /// <returns></returns>
    public virtual T? FirstOrDefault()
    {
        using var cmd = Dao!.GetSqlCommand();
        _top = 1;
        var queryText = GetShortQuery();
        if (QueryParameters?.Count > 0)
        {
            var condition = GetConditionString(cmd);
            queryText += "\nwhere " + condition;
        }

        if (OrderParameters?.Count > 0)
        {
            queryText += GetOrderByClause();
        }

        cmd.CommandText = queryText;
        var queryResult = GetFindResult(cmd);
        return queryResult.Count > 0 ? queryResult.First() : null;
    }

    /// <summary>
    /// Execute SQL and map result into a list of entities.
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected internal IList<T> GetFindResult(SqlCommand cmd)
    {
        var mapper = PropertyMapper;
#if DEBUG
            var start = DateTime.Now;
#endif
        using var reader = CommandRunner.Run(cmd, x => x.ExecuteReader());
#if DEBUG
        var end = DateTime.Now;
        _log.Debug($"Execute SQL {end.Subtract(start).TotalMilliseconds:#,##0}mS");
        start = DateTime.Now;
#endif
        var result = mapper.MapAll(reader);

#if DEBUG
        end = DateTime.Now;
        _log.Debug($"Mapped {result.Count:#,##0} Objects in {end.Subtract(start).TotalMilliseconds:#,##0}mS");
#endif
        reader.Close();
        return result;
    }

    protected internal virtual string GetOrderByClause()
    {
        var orderParameters = OrderParameters;
        if (OrderParameters == null || OrderParameters.Count == 0)
        {
            if (TableMap.DefaultOrderParameters == null || TableMap.DefaultOrderParameters.Count == 0)
            {
                return "";
            }

            orderParameters = TableMap.DefaultOrderParameters;
        }

        var orderings = new List<string>();
        foreach (var op in orderParameters!)
        {
            if (op == null) continue;
            var orderBy = "[" + op.FieldName + "]";
            if (op.SortDescending) orderBy += " desc";
            orderings.Add(orderBy);
        }

        return orderings.Count > 0 ? "\norder by " + string.Join(", ", orderings) : "";
    }

    protected internal virtual string GetShortQuery()
    {
        var query = _baseQuery ?? $"select * from [{TableMap.SchemaName}].[{TableMap.ShortQueryName}]";
        return _top > 0 && !query.Contains("select top")
            ? query.Replace("select ", $"select top {_top} ")
            : query;
    }

    protected internal virtual string GetFullQuery()
    {
        return _top > 0
            ? $"select top {_top} * from [{TableMap.SchemaName}].[{TableMap.FullQueryName}]"
            : $"select * from [{TableMap.SchemaName}].[{TableMap.FullQueryName}]";
    }

    protected internal virtual string GetCountQuery()
    {
        return $"select count(*) from [{TableMap.SchemaName}].[{TableMap.ShortQueryName}]";
    }

    private string GetConditionString(SqlCommand cmd)
    {
        var condition = new StringBuilder();
        foreach (var param in QueryParameters!)
        {
            if (condition.Length > 0)
            {
                condition.Append(param.IsOredCondition ? " or " : " and ");
            }

            condition.Append(GetConditionString(cmd, param));
        }

        return condition.ToString();
    }

    private string GetConditionString(SqlCommand cmd, QueryParameter param)
    {
        var condition = new StringBuilder();
        var paramCondition = GetSingleCondition(cmd, param);

        if (paramCondition == null) throw new InvalidFilterCriteriaException
        (
            $"QueryExpression [{param.FieldName}] {param.Operator} '{param.CompareTo}' can not be evaluated."
        );

        condition.Append(paramCondition);
        if (param.AdditionalParameters.Count > 0)
        {
            foreach (var additionalParam in param.AdditionalParameters)
            {
                condition.Append(additionalParam.IsOredCondition ? " or " : " and ");
                condition.Append(GetConditionString(cmd, additionalParam));
            }

            return $"({condition})";
        }

        return condition.ToString();
    }

    protected internal virtual string? GetSingleCondition(SqlCommand cmd, QueryParameter param)
    {
        string? result = null;
        var field = $"[{param.FieldName}]";
        switch (param.Operator)
        {
            case ComparisonOperator.Equal:
            case ComparisonOperator.Greater:
            case ComparisonOperator.GreaterOrEqual:
            case ComparisonOperator.Less:
            case ComparisonOperator.LessOrEqual:
            case ComparisonOperator.Like:
            {
                result = $"{field} {param.GetOperator()} @p{cmd.Parameters.Count}";
                cmd.Parameters.AddWithValue($"p{cmd.Parameters.Count}", param.CompareTo);
                break;
            }
            case ComparisonOperator.Between:
            {
                result = $"{field} between @p{cmd.Parameters.Count} and @p{cmd.Parameters.Count + 1}";
                var compareValues = (object[])param.CompareTo;
                cmd.Parameters.AddWithValue($"p{cmd.Parameters.Count}", compareValues[0]);
                cmd.Parameters.AddWithValue($"p{cmd.Parameters.Count}", compareValues[1]);
                break;
            }
            case ComparisonOperator.In:
            {
                var values = (IEnumerable)param.CompareTo;
                var plist = new List<string>();
                foreach (var v in values)
                {
                    var item = v as IntegerBasedEntity;
                    var compareTo = item != null
                        ? item.Id
                        : v;

                    plist.Add($"@p{cmd.Parameters.Count}");
                    cmd.Parameters.AddWithValue($"p{cmd.Parameters.Count}", compareTo);
                }
                var pchain = string.Join(", ", plist.ToArray());
                result = $"{field} {param.GetOperator()} ({pchain})";
                break;
            }
            case ComparisonOperator.IsNull:
            {
                result = $"{field} {param.GetOperator()}";
                break;
            }
            case ComparisonOperator.True:
            {
                result = $"isnull({field},0)=1";
                break;
            }
            case ComparisonOperator.False:
            {
                result = $"isnull({field},0)=0";
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Resolve child references as defined in map.
    /// </summary>
    /// <param name="item"></param>
    public void ResolveRelations(object item)
    {
        ResolveOneWayRelations(item);
        ResolveTwoWayRelations(item);
    }

    /// <summary>
    /// Resolve 1:n relations.
    /// </summary>
    /// <param name="item"></param>
    public void ResolveOneWayRelations(object item)
    {
        foreach (var r in TableMap.OneWayRelations)
        {
            MapChildCollection(r, item);
        }
    }

    /// <summary>
    /// Resolve n:m relations.
    /// </summary>
    /// <param name="item"></param>
    public void ResolveTwoWayRelations(object item)
    {
        foreach (var r in TableMap.TwoWayRelations)
        {
            MapChildCollection(r, item);
        }
    }

    private void MapChildCollection(ChildCollection r, object? item)
    {
        if (item == null || string.IsNullOrEmpty(r.GetCommandName))
        {
            return;
        }

        var start = DateTime.Now;

        SqlCommand cmd;

        if (r.GetCommandType == CommandType.StoredProcedure)
        {
            cmd = Dao!.GetSqlCommand(r.GetCommandName);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue(typeof(T).Name + "Id", TableMap.GetKeyValues(item).First());
        }
        else
        { 
            //assume table or view
            var viewName = r.GetCommandName;
            var masterField = r.MasterFieldName ?? $"{typeof(T).Name}Id";
            var condition = $"[{masterField}]=@p1";
            var query = $"select * from {viewName} where {condition}";
            cmd = Dao!.GetSqlCommand(query);
            cmd.Parameters.AddWithValue("p1", TableMap.GetKeyValues(item).First());
        }

        using (var reader = CommandRunner.Run(cmd, x => x.ExecuteReader()))
        {
            //get generic mapper for child property
            var childType = new[] { r.ChildType };
            var getMapper = _catalog.GetType().GetMethod("GetPropertyMapper");
            var getGenericMapper = getMapper!.MakeGenericMethod(childType);
            var mapper = getGenericMapper.Invoke(_catalog, null);

            var mapMethod = mapper.GetType().GetMethod("MapAll", new[] { typeof(IDataReader) });
            var args = new object[] { reader };
            var list = mapMethod.Invoke(mapper, args);

            var prop = typeof(T).GetProperty(r.PropertyName);
            prop.SetValue(item, list, null);

            reader.Close();
        }

        var end = DateTime.Now;
#if DEBUG
            _log.Debug($"Execute query {end.Subtract(start).TotalMilliseconds:#,##0}mS");
#endif
    }
}