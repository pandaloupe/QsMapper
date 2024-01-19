using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using log4net;

using Net.Arqsoft.QsMapper.CommandBuilder;
using Net.Arqsoft.QsMapper.Exceptions;
using Net.Arqsoft.QsMapper.Model;
using Net.Arqsoft.QsMapper.QueryBuilder;

using CommandType = System.Data.CommandType;

namespace Net.Arqsoft.QsMapper;

/// <inheritdoc cref="IGenericDao" />
/// <summary>
/// Common implementation of IGenericDao.
/// </summary>
public class GenericDao : IGenericDao
{
    public delegate SqlConnection GetConnectionDelegate();

    private readonly GetConnectionDelegate _getConnection;

    private readonly ILog _log = LogManager.GetLogger(typeof(GenericDao));

    public ICatalog Catalog { get; set; }

    private SqlConnection _sqlConnection;

    private SqlTransaction _transaction;

    public GenericDao(GetConnectionDelegate getConnectionDelegate, ICatalog catalog = null)
    {
        _getConnection = getConnectionDelegate;
        Catalog = catalog ?? new Catalog();
    }

    public GenericDao(string connectionString, ICatalog catalog = null)
    {
        _getConnection = () =>
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        };

        Catalog = catalog ?? new Catalog();
    }

    public SqlConnection OpenConnection()
    {
#if DEBUG
            var time = DateTime.Now;
#endif
        if (_sqlConnection == null)
        {
            _sqlConnection = _getConnection();
        }
        else if (_sqlConnection.State != ConnectionState.Open)
        {
            _sqlConnection.Dispose();
            _sqlConnection = _getConnection();
        }

        try
        {
            // test for connection timeout, should not be used inside transaction
            if (_transaction == null)
            {
                using var cmd = new SqlCommand("select 1", _sqlConnection);
                cmd.ExecuteScalar();
            }
        }
        catch (SqlException ex)
        {
            _log.Error("OpenConnection failed", ex);
            _sqlConnection = _getConnection();
        }
        catch (Exception ex)
        {
            _log.Error("OpenConnection failed", ex);
            throw;
        }
#if DEBUG
            var span = DateTime.Now.Subtract(time).TotalMilliseconds;
            _log.Debug($"Open Connection {span:#,##0} mS");
#endif
        return _sqlConnection;
    }

    public void CloseConnection()
    {
        if (_sqlConnection == null || _transaction != null)
        {
            return;
        }

        _sqlConnection.Close();
        _sqlConnection.Dispose();
        _sqlConnection = null;
    }

    private SqlConnection SqlConnection => OpenConnection();

    public DateTime GetCurrentDate()
    {
        var cmd = GetSqlCommand("select getdate()");
        using var reader = cmd.ExecuteReader();
        reader.Read();
        var date = reader.GetDateTime(0);
        reader.Close();
        return date;
    }

    public T? Get<T>(object id) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();

        var keyName = map.KeyFields[0];
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        queryPlugin.QueryParameters = null;
        var queryText = queryPlugin.GetFullQuery() + "\n"
                                                   + "where [" + keyName + "]=@" + keyName;
        var query = GetSqlCommand(queryText);
        query.Parameters.AddWithValue(keyName, id);

        using var reader = CommandRunner.Run(query, x => x.ExecuteReader());
        if (!reader.Read())
        {
            return null;
        }

        var result = mapper.Map(reader);
        reader.Close();
        map.GetQueryPlugin(this, mapper).ResolveRelations(result);
        return result;
    }

    public SqlConnection GetSqlConnection()
    {
        return SqlConnection;
    }

    public SqlCommand GetSqlCommand(string? queryText)
    {
        var cmd = new SqlCommand(queryText, SqlConnection);
        if (_transaction != null)
        {
            cmd.Transaction = _transaction;
        }

        return cmd;
    }

    public SqlCommand GetSqlCommand()
    {
        var cmd = new SqlCommand { Connection = SqlConnection };
        if (_transaction != null)
        {
            cmd.Transaction = _transaction;
        }

        return cmd;
    }

    public void Restore<T>(object item) where T : class, new()
    {
        Restore<T>(item, true);
    }

    public void Restore<T>(object item, bool withRelations) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();
        var keyName = map.KeyFields[0];
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        queryPlugin.QueryParameters = null;
        var queryText = queryPlugin.GetShortQuery() + "\n"
                                                    + $"where [{keyName}]=@{keyName}";
        for (var i = 1; i < map.KeyFields.Length; i++)
        {
            queryText += $"\r\nand [{map.KeyFields[i]}]=@{map.KeyFields[i]}";
        }

        var query = GetSqlCommand(queryText);
        query.Parameters.AddWithValue(keyName, map.GetKeyValues(item).First());

        for (var i = 1; i < map.KeyFields.Length; i++)
        {
            query.Parameters.AddWithValue(map.KeyFields[i], map.GetKeyValues(item)[i]);
        }

        using var reader = CommandRunner.Run(query, x => x.ExecuteReader());
        if (reader.Read())
        {
            mapper.Map(item, reader);
        }

        reader.Close();
        if (withRelations)
        {
            map.GetQueryPlugin(this, mapper).ResolveRelations(item);
        }
    }

    public T? Get<T>(params object[] compositeId) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        queryPlugin.QueryParameters = null;
        var queryText = queryPlugin.GetShortQuery()
                        + "\n"
                        + "\nwhere " + map.GetKeyCondition();

        var query = GetSqlCommand(queryText);
        map.AddKeyParams(query, compositeId);

        using var reader = CommandRunner.Run(query, x => x.ExecuteReader());
        if (!reader.Read())
        {
            return null;
        }

        var result = mapper.Map(reader);
        reader.Close();
        map.GetQueryPlugin(this, mapper).ResolveRelations(result);
        return result;
    }

    public void Save<T>(T item) where T : class, new()
    {

        Save<T>((object)item);
    }

    public virtual void Save<T>(object item) where T : class, new()
    {
        ExecuteTransaction(() =>
        {
            var itemBase = item as IntegerBasedEntity;

            if (itemBase != null && itemBase.Id == 0)
            {
                ExecuteInsert<T>(item);
            }
            else
            {
                if (CommandRunner.DebuggingOn &&  _log.IsDebugEnabled)
                {
                    _log.Debug("--begin update--");
                }

                var map = Catalog.GetTableMap<T>();
                var queryText = "select * from " + map.FullTableName + "\nwhere " + map.GetKeyCondition();
                var query = GetSqlCommand(queryText);
                map.AddKeyParams(query, item);
                using var reader = query.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    ExecuteInsert<T>(item);
                }
                else
                {
                    var fields = new List<string>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        fields.Add(reader.GetName(i));
                    }

                    reader.Close();

                    var update = "update " + map.FullTableName;
                    var setters = new List<string>();

                    using var cmd = GetSqlCommand();
                    foreach (var field in fields)
                    {
                        if (!AddParameter(cmd, field, item, map))
                        {
                            continue;
                        }

                        setters.Add(string.Format("[{0}]=@{0}", field));
                    }

                    update += "\nset " + string.Join(",\n  ", setters.ToArray())
                                       + "\nwhere " + map.GetKeyCondition();

                    cmd.CommandText = update;
                    map.AddKeyParams(cmd, item);

                    CommandRunner.Run(cmd, x => x.ExecuteNonQuery());
                }
            }

            //update relations only if full object was passed
            //if (typeof(T) == item.GetType()) {
            UpdateRelations<T>(item);
            //}
        });

        CloseConnection();
    }

    public virtual void Delete<T>(T item) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var queryText = "delete " + map.FullTableName + "\nwhere " + map.GetKeyCondition();
        var cmd = GetSqlCommand(queryText);
        map.AddKeyParams(cmd, item);

        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());
        CloseConnection();
    }

    public void Delete<T>(params object[] keys) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var queryText = "delete " + map.FullTableName + "\nwhere " + map.GetKeyCondition();
        var cmd = GetSqlCommand(queryText);
        map.AddKeyParams(cmd, keys);

        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());

        CloseConnection();
    }

    public void Delete<T>(IDictionary<string, object> parameters) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var queryText = "delete " + map.FullTableName;
        var conditions = parameters.Keys.Select(key => string.Format("[{0}]=@{0}", key)).ToList();
        queryText += "\nwhere " + string.Join(" and ", conditions);

        var cmd = GetSqlCommand(queryText);
        foreach (var key in parameters.Keys)
        {
            cmd.Parameters.AddWithValue(key, parameters[key]);
        }

        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());

        CloseConnection();
    }

    public IQuery<T> Query<T>() where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        return queryPlugin.NewQuery();
    }

    public IQuery<T> Query<T>(string tableOrView) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        return queryPlugin.NewQuery(tableOrView);
    }

    public IQuery<T> Query<T>(Func<QueryParameter, QueryParameter> condition) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var mapper = Catalog.GetPropertyMapper<T>();
        var queryPlugin = map.GetQueryPlugin(this, mapper);
        return queryPlugin
            .NewQuery()
            .Where(condition);
    }

    public void BeginTransaction()
    {
        if (_transaction != null)
        {
            return;
        }

        _transaction = SqlConnection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            _transaction.Commit();
        }
        catch (Exception ex)
        {
            _log.Error("SQL transaction COMMIT failed.", ex);
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void RollbackTransaction()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            _transaction.Rollback();
        }
        catch (Exception ex)
        {
            _log.Error("SQL transaction ROLLBACK failed.", ex);
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        // already inside transaction
        if (_transaction != null)
        {
            await action();
            return;
        }

        using var transaction = SqlConnection.BeginTransaction();
        _transaction = transaction;

        try
        {
            await action();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            _transaction = null;
        }
    }

    public void ExecuteTransaction(Action action)
    {
        ExecuteTransaction(x => 
        {
            action.Invoke();
        });
    }

    public void ExecuteTransaction(Action<IGenericDao> action)
    {
        // already inside transaction
        if (_transaction != null)
        {
            action?.Invoke(this);
            return;
        }

        using var transaction = SqlConnection.BeginTransaction();
        _transaction = transaction;

        try
        {
            action?.Invoke(this);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            _transaction = null;
        }
    }

    public ICommand Execute(string? procedureName)
    {
        return new DatabaseCommand(procedureName, Catalog, this);
    }

    public ICommand ExecuteSql(string? queryText)
    {
        return new DatabaseCommand(queryText, Catalog, this, CommandType.Text);
    }

    private void ExecuteInsert<T>(object item) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var insert = "insert into " + map.FullTableName;

        // execute dummy query to get table structure
        var queryText = "select * from " + map.FullTableName + "\nwhere 0=1";
        var query = GetSqlCommand(queryText);
        using (var reader = query.ExecuteReader())
        {
            reader.Read();
            var fields = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var field = reader.GetName(i);
                fields.Add(field);
            }

            reader.Close();

            using (var cmd = GetSqlCommand())
            {

                // use only fields that aren't readonly
                var columns = new List<string>();
                foreach (var field in fields)
                {
                    if (!AddParameter(cmd, field, item, map))
                    {
                        continue;
                    }

                    columns.Add(field);
                }

                insert += " ( " + string.Join(",\n  ", columns.Select(x => "[" + x + "]")) + ")"
                          + "\nvalues (" + string.Join(",\n  ", columns.Select(x => "@" + x)) + ");"
                          + "select cast(scope_identity() as int);";

                if (CommandRunner.DebuggingOn && _log.IsDebugEnabled)
                {
                    _log.Debug($"Executing Insert: {insert}");
                }

                cmd.CommandText = insert;

                var identity = CommandRunner.Run(cmd, x => x.ExecuteScalar());

                if (identity is int i)
                {
                    var baseItem = item as IntegerBasedEntity;
                    if (baseItem != null)
                    {
                        baseItem.Id = i;
                    }
                    else
                    {
                        var key = map.KeyFields[0];
                        var prop = item.GetType().GetProperty(key);
                        prop.SetValue(item, i, null);
                    }
                }
            }
        }

        //load item back from database to reflect
        //changes made by triggers or computed columns
        Restore<T>(item, false);

        CloseConnection();
    }

    private bool AddParameter<T>(SqlCommand cmd, string field, object item, TableMap<T> map)
        where T : class, new()
    {
        var type = item.GetType();

        if (!map.KeyFields.Contains(field) || map.HasAutoId)
        {
            if (map.ReadonlyFields != null && map.ReadonlyFields.Contains(field))
            {
                return false;
            }
        }

        var prop = type.GetProperty(field);
        if (map.IsReference(field))
        {
            try
            {
                var refValue = map.GetReferenceValue(item, field);
                cmd.Parameters.AddWithValue(field, refValue ?? DBNull.Value);
                return true;
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not map field '{field}' to object '{item.GetType()}'", ex);
            }
        }
        else if (prop != null)
        {
            var param = cmd.Parameters.AddWithValue(field, prop.GetValue(item, null) ?? DBNull.Value);
            if (param.Value == DBNull.Value || param.Value is string strVal && string.IsNullOrEmpty(strVal))
            {
                if (prop.PropertyType == typeof(byte[]))
                {
                    param.SqlDbType = SqlDbType.Binary;
                }
            }

            return true;
        }
        else if (field.EndsWith("Id"))
        {
            var refField = field.Substring(0, field.Length - 2);
            prop = type.GetProperty(refField);
            if (prop != null)
            {
                var refItem = prop.GetValue(item, null);
                if (refItem != null)
                {
                    var refType = refItem.GetType();
                    var refProp = refType.GetProperty("Id");
                    if (refProp == null)
                    {
                        throw new MapperException($"'Id' property not found on object of type {refType.Name}");
                    }

                    cmd.Parameters.AddWithValue(field, refProp.GetValue(refItem, null) ?? DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue(field, DBNull.Value);
                }
                return true;
            }
        }
        return false;
    }

    private void UpdateRelations<T>(object item) where T : class, new()
    {
        var map = Catalog.GetTableMap<T>();
        var oldState = Get<T>(map.GetKeyValues(item));

        foreach (var r in map.OneWayRelations)
        {
            UpdateOneToManyRelation(r, oldState, item);
        }

        foreach (var r in map.TwoWayRelations)
        {
            UpdateManyToManyRelation(r, oldState, item);
        }
    }

    private void UpdateOneToManyRelation(ChildCollection collection, object oldState, object newState)
    {
        // if no table name is set this is considered a read only collection
        if (collection.TableName == null)
        {
            return;
        }

        var newStateType = newState.GetType();
        var newStateProp = newStateType.GetProperty(collection.PropertyName);
        if (newStateProp == null)
        {
            return;
        }

        var newCollection = newStateProp.GetValue(newState, null) as IEnumerable<object>;
        // only update collections that are present on newState object
        // => prevent deletion if incomplete object is saved
        if (newCollection == null)
        {
            return;
        }

        // get generic method infos for saving child objects
        var delete = GetMethod("Delete", collection.ChildType);
        var save = GetMethod("Save", collection.ChildType);

        if (oldState == null)
        {
            return;
        }

        // compare with old state for updates, deletes and inserts
        var oldStateType = oldState.GetType();
        var oldStateProp = oldStateType.GetProperty(collection.PropertyName);
        if (oldStateProp.GetValue(oldState, null) is IEnumerable<object> oldCollection)
        {
            // process deletes
            foreach (var d in oldCollection.Except(newCollection))
            {
                // delete d from database    
                var oldBase = d as IntegerBasedEntity;
                if (oldBase != null && oldBase.Id == 0)
                {
                    continue;
                }

                delete.Invoke(this, new[] { d });
            }

            // process updates
            foreach (var u in newCollection.Intersect(oldCollection))
            {
                var oldItem = oldCollection.First(x => x.Equals(u));
                if (!HasChanges(u, oldItem))
                {
                    continue;
                }

                // update u
                save.Invoke(this, new[] { u });
            }

            // process inserts
            foreach (var i in newCollection.Except(oldCollection))
            {
                // insert i
                save.Invoke(this, new[] { i });
            }
        }
        else
        {
            // insert all items when old collection is null
            foreach (var i in newCollection)
            {
                // insert i
                save.Invoke(this, new[] { i });
            }
        }
    }

    private void UpdateManyToManyRelation(ChildCollection collection, object oldState, object newState)
    {
        //if no table name is set this is considered a read only collection
        if (collection.TableName == null)
        {
            return;
        }

        var newStateType = newState.GetType();
        var newStateProp = newStateType.GetProperty(collection.PropertyName);
        //only update collections that are present on newState object
        if (newStateProp == null)
        {
            return;
        }

        var newCollection = newStateProp.GetValue(newState, null) as IEnumerable<object>;
        //only update collections that are present on newState object
        if (newCollection == null)
        {
            return;
        }

        if (oldState != null)
        {
            //compare with old state for deletes and inserts
            var oldStateType = oldState.GetType();
            var oldStateProp = oldStateType.GetProperty(collection.PropertyName);
            var oldCollection = oldStateProp.GetValue(oldState, null) as IEnumerable<object>;
            if (oldCollection != null)
            {
                //process deletes
                foreach (var d in oldCollection.Except(newCollection))
                {
                    //delete d from database
                    DeleteManyToManyRelation(collection, newState, d, oldState.GetType().Name);
                }

                //process inserts
                foreach (var i in newCollection.Except(oldCollection))
                {
                    //insert i
                    InsertManyToManyRelation(collection, newState, i, oldState.GetType().Name);
                }
            }
            else
            {
                //inserts all items
                foreach (var i in newCollection)
                {
                    //insert i
                    InsertManyToManyRelation(collection, newState, i, oldState.GetType().Name);
                }
            }
        }
    }

    private void InsertManyToManyRelation(ChildCollection collection, object newState, object linkedObject,
        string typeName)
    {
        using var cmd = GetSqlCommand();
        var masterField = collection.MasterFieldName ?? $"{typeName}Id";
        var childField = collection.ChildFieldName ?? $"{linkedObject.GetType().Name}Id";
        cmd.CommandText =
            $"insert into {collection.TableName} ([{masterField}], [{childField}]) values (@p1, @p2)";
        var id1Property = newState.GetType().GetProperty(collection.MasterPropertyName ?? "Id");
        var id2Property = linkedObject.GetType().GetProperty(collection.ChildPropertyName ?? "Id");
        cmd.Parameters.AddWithValue("p1", id1Property.GetValue(newState, null));
        cmd.Parameters.AddWithValue("p2", id2Property.GetValue(linkedObject, null));
        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());
    }

    private void DeleteManyToManyRelation(ChildCollection collection, object newState, object linkedObject,
        string typeName)
    {
        using var cmd = GetSqlCommand();
        var masterField = collection.MasterFieldName ?? $"{typeName}Id";
        var childField = collection.ChildFieldName ?? $"{linkedObject.GetType().Name}Id";
        cmd.CommandText = $"delete {collection.TableName} where [{masterField}] = @p1 and [{childField}] = @p2";
        var id1Property = newState.GetType().GetProperty(collection.MasterPropertyName ?? "Id");
        var id2Property = linkedObject.GetType().GetProperty(collection.ChildPropertyName ?? "Id");
        cmd.Parameters.AddWithValue("p1", id1Property.GetValue(newState, null));
        cmd.Parameters.AddWithValue("p2", id2Property.GetValue(linkedObject, null));
        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());
    }

    private static bool HasChanges(object item, object compareTo)
    {
        var itemType = item.GetType();
        var compareType = compareTo.GetType();
        foreach (var prop in itemType.GetProperties())
        {
            var compareProp = compareType.GetProperty(prop.Name);
            //only compare props that are present
            if (compareProp == null)
                continue;
            //return true on first difference
            var value = prop.GetValue(item, null);
            if (value != null && !value.Equals(compareProp.GetValue(compareTo, null))
                || prop.GetValue(item, null) == null && compareProp.GetValue(compareTo, null) != null)
            {
                return true;
            }
        }
        return false;
    }

    private MethodInfo GetMethod(string methodName, Type? argumentType)
    {
        var methodInfo = GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(x => x.Name == methodName && x.IsGenericMethod);

        return methodInfo.MakeGenericMethod(new[] { argumentType });
    }

    public void Dispose()
    {
        if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
        {
            return;
        }

        _sqlConnection.Close();
        _sqlConnection.Dispose();
    }
}