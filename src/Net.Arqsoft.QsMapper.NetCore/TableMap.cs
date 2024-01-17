using System.Data.SqlClient;
using System.Linq.Expressions;
using Net.Arqsoft.QsMapper.Model;
using Net.Arqsoft.QsMapper.QueryBuilder;
using Net.Arqsoft.QsMapper.Util;

namespace Net.Arqsoft.QsMapper;

/// <summary>
/// A TableMap defines how object properties are mapped to a SQL Server table.
/// Fields in the data table are mapped automatically by name so object.[Name] will be mapped to table.[Name].
/// If nothing else is declared it is assumed the tables' primary key is named Id.
/// n:1 references are mapped by default as object.ReferenceObject.Id => table.ReferenceObjectId.
/// 1:n and n:m references may be mapped explicitly using the WithMany and WithManyToMany methods.
/// All catalog methods support fluent syntax.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TableMap<T> where T : class, new()
{
    public TableMap()
    {
        HasAutoId = true;
    }

    #region Private

    /**
     * Support for default naming and keys
     */
    private string _schemaName;
    private string _tableName;
    private string[] _keyFields;
    private string[] _readonlyFields;
    private IDictionary<string, IComplexProperty> _references;

    private OrderParameter _currentOrderParameter;

    public ICatalog Catalog { get; set; }

    private void AddOrderParameter(OrderParameter param)
    {
        if (DefaultOrderParameters == null) DefaultOrderParameters = new List<OrderParameter>();
        DefaultOrderParameters.Add(param);
    }

    #endregion

    #region Public

    public bool UseViewForQuery { get; set; }
    public string ShortViewName { get; set; }
    public string FullViewName { get; set; }
    //public object Mapper { get; set; }

    public object QueryPlugin;

    public Type PluginType { get; set; }

    public bool IsProvisioned { get; set; }
    public bool HasAutoId { get; set; }

    public string TableName
    {
        get
        {
            if (string.IsNullOrEmpty(_tableName))
            {
                NameResolver.ResolveTableName(this);
            }
            return _tableName;
        }
        set { _tableName = value; }
    }

    public string SchemaName
    {
        get
        {
            if (string.IsNullOrEmpty(_tableName))
            {
                NameResolver.ResolveTableName(this);
            }
            return _schemaName;
        }
        set { _schemaName = value; }
    }

    public string[] KeyFields
    {
        get
        {
            if (_keyFields == null)
            {
                _keyFields = new[] { "Id" };
                if (_readonlyFields == null)
                {
                    _readonlyFields = new[] { "Id" };
                }
                else
                {
                    var list = _readonlyFields.ToList();
                    if (!list.Contains("Id"))
                    {
                        list.Add("Id");
                        _readonlyFields = list.ToArray();
                    }
                }
            }
            return _keyFields;
        }
        set => _keyFields = value;
    }

    public string[] ReadonlyFields
    {
        get
        {
            if (_keyFields == null)
            {
                _keyFields = new[] { "Id" };
                if (_readonlyFields == null)
                {
                    _readonlyFields = new[] { "Id" };
                }
                else
                {
                    var list = _readonlyFields.ToList();
                    if (!list.Contains("Id"))
                    {
                        list.Add("Id");
                        _readonlyFields = list.ToArray();
                    }
                }
            }
            return _readonlyFields;
        }
        set => _readonlyFields = value;
    }

    //1:n relations
    private IList<ChildCollection> _oneWayRelations;
    public IList<ChildCollection> OneWayRelations
    {
        get { return _oneWayRelations ?? (_oneWayRelations = new List<ChildCollection>()); }
        set { _oneWayRelations = value; }
    }

    //m:n relations
    private IList<ChildCollection> _twoWayRelations;
    public IList<ChildCollection> TwoWayRelations
    {
        get { return _twoWayRelations ?? (_twoWayRelations = new List<ChildCollection>()); }
        set { _twoWayRelations = value; }
    }

    private IList<OrderParameter> _defaultOrderParameters;
    public IList<OrderParameter> DefaultOrderParameters
    {
        get { return _defaultOrderParameters ?? (_defaultOrderParameters = new List<OrderParameter>()); }
        set { _defaultOrderParameters = value; }
    }

    #endregion

    #region Fluent Catalog Methods

    /// <summary>
    /// Makes the ID field writeable in insert statements.
    /// </summary>
    /// <returns></returns>
    public TableMap<T> IncludeIdInInsert()
    {
        HasAutoId = false;
        return this;
    }

    /// <summary>
    /// Set Keys Field(s) if complex key or not equal "Id"
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public TableMap<T> WithKeys(params string[] keys)
    {
        KeyFields = keys;
        return this;
    }

    /// <summary>
    /// Set Schema and Table Name for Get and Save explicitly
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public TableMap<T> Table(string schemaName, string tableName)
    {
        SchemaName = schemaName;
        TableName = tableName;
        return this;
    }

    /// <summary>
    /// Set explicit Table Name, resolves Schema from the Classes Namespace
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public TableMap<T> Table(string tableName)
    {
        NameResolver.ResolveTableName(this);
        TableName = tableName;
        return this;
    }

    /// <summary>
    /// Append 'Query' to TableName for query execution
    /// </summary>
    /// <returns>self</returns>
    public TableMap<T> QueryWithView()
    {
        UseViewForQuery = true;
        return this;
    }

    /// <summary>
    /// Use specific view for query execution and single get statements.
    /// Schema name will be added automatically from namespace.
    /// </summary>
    /// <param name="viewName">name of SQL Server View (same schema as table)</param>
    /// <returns>self</returns>
    public TableMap<T> QueryWithView(string viewName)
    {
        UseViewForQuery = true;
        ShortViewName = viewName;
        FullViewName = viewName;
        return this;
    }

    /// <summary>
    /// Use different views for query execution and single get statements.
    /// Schema name will be added automatically from namespace.
    /// </summary>
    /// <param name="shortViewName">name of SQL Server View (same schema as table) used for queries</param>
    /// <param name="fullViewName">name of SQL Server View (same schema as table) used for get</param>
    /// <returns>self</returns>
    public TableMap<T> QueryWithView(string shortViewName, string fullViewName)
    {
        UseViewForQuery = true;
        ShortViewName = shortViewName;
        FullViewName = fullViewName;
        return this;
    }

    public TableMap<T> WithPlugin(Type queryPluginType)
    {
        PluginType = queryPluginType;
        return this;
    }

    public TableMap<T> Id(string idField)
    {
        KeyFields = new[] { idField };
        return this;
    }

    public TableMap<T> CompositeId(params string[] idFields)
    {
        KeyFields = idFields;
        return this;
    }

    public TableMap<T> ReadOnly(params string[] readonlyFields)
    {
        ReadonlyFields = readonlyFields;
        return this;
    }

    /// <summary>
    /// activate provisioning for this type, so all references will point to the same instance
    /// </summary>
    /// <returns></returns>
    public TableMap<T> WithProvisioning()
    {
        IsProvisioned = true;
        return this;
    }

    public TableMap<T> ByDefaultOrderBy(string fieldname)
    {
        _currentOrderParameter = new OrderParameter(fieldname);
        AddOrderParameter(_currentOrderParameter);
        return this;
    }

    public TableMap<T> ThenBy(string fieldname)
    {
        return ByDefaultOrderBy(fieldname);
    }

    public TableMap<T> Descending
    {
        get
        {
            _currentOrderParameter.SortDescending = true;
            //prevent settings order direction anew
            _currentOrderParameter = null;
            return this;
        }
    }
    public TableMap<T> Ascending
    {
        get
        {
            _currentOrderParameter.SortDescending = false;
            //prevent settings order direction anew
            _currentOrderParameter = null;
            return this;
        }
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        return WithManyToMany(propertyExpression, tableName, tableName);
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        return WithManyToMany(propertyExpression, viewName, tableName);
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName, string masterFieldName, string childFieldName)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        return WithManyToMany(propertyExpression, viewName, tableName, masterFieldName, childFieldName);
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName, string tableName)
    {
        TwoWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName

        });
        return this;
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName, string tableName, string masterFieldName, string childFieldName)
    {
        TwoWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            MasterFieldName = masterFieldName,
            ChildFieldName = childFieldName,
            GetCommandName = viewName
        });
        return this;
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName, string tableName,
        Expression<Func<T, object>> masterPropertyExpression, Expression<Func<T1, object>> childPropertyExpression,
        string masterFieldName, string childFieldName)
    {
        TwoWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName,
            MasterFieldName = masterFieldName,
            ChildFieldName = childFieldName,
            MasterPropertyName = ExpressionHelper.GetPropertyName(masterPropertyExpression),
            ChildPropertyName = ExpressionHelper.GetPropertyName(childPropertyExpression)
        });
        return this;
    }

    public TableMap<T> WithManyToMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName, string tableName,
        Expression<Func<T, object>> masterPropertyExpression, Expression<Func<T1, object>> childPropertyExpression)
    {
        var masterPropertyName = ExpressionHelper.GetPropertyName(masterPropertyExpression);
        var childPropertyName = ExpressionHelper.GetPropertyName(childPropertyExpression);
        TwoWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName,
            MasterFieldName = masterPropertyName,
            ChildFieldName = childPropertyName,
            MasterPropertyName = masterPropertyName,
            ChildPropertyName = childPropertyName
        });
        return this;
    }

    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string commandName, CommandType commandType)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        OneWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = commandName,
            GetCommandType = commandType
        });
        return this;
    }

    /// <summary>
    /// Defines a one to many relation for retrieval and update with specific view and table name.
    /// </summary>
    /// <typeparam name="T1">Type of detail model.</typeparam>
    /// <param name="propertyExpression">Lambda expression pointing to detail property.</param>
    /// <param name="viewName">Name of SQL view used for retrieval.</param>
    /// <param name="tableName">Name of SQL table used for updates.</param>
    /// <returns>configured TableMap instance.</returns>
    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string tableName, string viewName)
    {
        OneWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName
        });
        return this;
    }

    /// <summary>
    /// Defines a one to many relation for retrieval and update with a specific view name.
    /// Table name will be determined automatically from the property name used in propertyExpression.
    /// </summary>
    /// <typeparam name="T1">Type of detail model.</typeparam>
    /// <param name="propertyExpression">Lambda expression pointing to detail property.</param>
    /// <param name="viewName">Name of SQL view used for retrieval.</param>
    /// <returns>configured TableMap instance.</returns>
    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression, string viewName)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        return WithMany(propertyExpression, tableName, viewName);
    }

    /// <summary>
    /// Defines a one to many relation for retrieval and update.
    /// Table name will be determined automatically as [ParentClass.Namespace].[ParentClass.Name + PropertyName].
    /// </summary>
    /// <typeparam name="T1">Type of detail model.</typeparam>
    /// <param name="propertyExpression">Lambda expression pointing to detail property.</param>
    /// <returns>configured TableMap instance.</returns>
    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression)
    {
        var propertyName = ExpressionHelper.GetPropertyName(propertyExpression);
        var tableName = $"[{SchemaName}].[{typeof(T).Name}{propertyName}]";
        return WithMany(propertyExpression, tableName, tableName);
    }

    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression,
        string viewName, string tableName,
        Expression<Func<T1, object>> referenceToMaster, Expression<Func<T, object>> masterKey,
        string masterFieldName)
    {
        OneWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName,
            MasterFieldName = masterFieldName,
            MasterPropertyName = ExpressionHelper.GetPropertyName(masterKey),
            ChildPropertyName = ExpressionHelper.GetPropertyName(referenceToMaster)
        });
        return this;
    }

    public TableMap<T> WithMany<T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression,
        string viewName, string tableName,
        Expression<Func<T1, object>> referenceToMaster, Expression<Func<T, object>> masterKey)
    {
        OneWayRelations.Add(new ChildCollection
        {
            ChildType = typeof(T1),
            PropertyName = ExpressionHelper.GetPropertyName(propertyExpression),
            TableName = tableName,
            GetCommandName = viewName,
            MasterFieldName = ExpressionHelper.GetPropertyName(masterKey),
            MasterPropertyName = ExpressionHelper.GetPropertyName(masterKey),
            ChildPropertyName = ExpressionHelper.GetPropertyName(referenceToMaster)
        });
        return this;
    }

    public TableMap<T> HasReference<T1>(string field, Expression<Func<T, T1>> propertyExpression, Expression<Func<T1, object>> fieldExpression)
    {
        if (_references == null) _references = new Dictionary<string, IComplexProperty>();
        _references.Add(field, new ComplexProperty<T, T1>(field, propertyExpression, fieldExpression));
        return this;
    }

    public bool IsReference(string field)
    {
        if (_references == null) return false;
        return _references.ContainsKey(field);
    }

    public object GetReferenceValue(object item, string field)
    {
        return !IsReference(field)
            ? null
            : _references[field].GetValue(item);
    }

    #endregion

    #region BaseQuery Implementation

    public BaseQuery<T> GetQueryPlugin(GenericDao dao, PropertyMapper<T> mapper)
    {
        if (QueryPlugin == null)
        {
            QueryPlugin = PluginType == null
                ? new BaseQuery<T>(Catalog)
                : Activator.CreateInstance(PluginType, Catalog);
        }

        var plugin = (BaseQuery<T>)QueryPlugin;
        if (plugin.TableMap == null) plugin.TableMap = this;
        if (plugin.Dao == null) plugin.Dao = dao;
        return plugin;
    }

    //public PropertyMapper<T> GetPropertyMapper() {
    //    if (Mapper == null) Mapper = new PropertyMapper<T>();
    //    return Mapper as PropertyMapper<T>;
    //}

    public string FullTableName
    {
        get { return $"[{SchemaName}].[{TableName}]"; }
    }

    public string ShortQueryName =>
        UseViewForQuery
            ? string.IsNullOrEmpty(ShortViewName) ? TableName + "Query" : ShortViewName
            : TableName;

    public string FullQueryName =>
        UseViewForQuery
            ? string.IsNullOrEmpty(FullViewName) ? TableName + "Query" : FullViewName
            : TableName;

    public object[] GetKeyValues(object item)
    {
        if (item == null) return null;
        var type = item.GetType();
        var result = new List<object>();
        foreach (var f in KeyFields)
        {
            var prop = type.GetProperty(f);
            if (prop == null && f.EndsWith("Id"))
            {
                prop = type.GetProperty(f.Substring(0, f.Length - 2));
                var child = prop.GetValue(item, null);
                var childType = prop.PropertyType;
                var childProp = childType.GetProperty("Id");
                result.Add(childProp.GetValue(child, null));
                continue;
            }
            result.Add(prop.GetValue(item, null));
        }
        return result.ToArray();
    }

    public string GetKeyCondition()
    {
        var conditions = KeyFields.Select(x => string.Format("[{0}] = @{0}", x)).ToArray();
        return string.Join(" and ", conditions);
    }

    public void AddKeyParams(SqlCommand cmd, T item)
    {
        AddKeyParams(cmd, (object)item);
    }

    public void AddKeyParams(SqlCommand cmd, object item)
    {
        var type = item.GetType();
        foreach (var f in KeyFields)
        {
            if (cmd.Parameters.Contains(f))
            {
                continue;
            }

            var prop = type.GetProperty(f);
            if (prop == null && f.EndsWith("Id"))
            {
                prop = type.GetProperty(f.Substring(0, f.Length - 2));
                var child = prop.GetValue(item, null);
                var childType = prop.PropertyType;
                var childProp = childType.GetProperty("Id");
                cmd.Parameters.AddWithValue(f, childProp.GetValue(child, null));
                continue;
            }

            cmd.Parameters.AddWithValue(f, prop.GetValue(item, null));
        }
    }

    public void AddKeyParams(SqlCommand cmd, object[] values)
    {
        if (values.Length != KeyFields.Length)
        {
            throw new Exception($"Parameter count does not match key count in map {FullTableName}.");
        }
        for (var i = 0; i < KeyFields.Length; i++)
        {
            cmd.Parameters.AddWithValue(KeyFields[i], values[i]);
        }
    }

    #endregion

}