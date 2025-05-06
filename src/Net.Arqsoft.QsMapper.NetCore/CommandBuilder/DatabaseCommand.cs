using System.Data;
using Microsoft.Data.SqlClient;

using Net.Arqsoft.QsMapper.QueryBuilder;
using CommandType = System.Data.CommandType;

namespace Net.Arqsoft.QsMapper.CommandBuilder;

/// <inheritdoc />
/// <summary>
/// Base class for building SQl Commands for stored procedures.
/// </summary>
public class DatabaseCommand : ICommand
{
    private readonly IGenericDao _dao;
    protected readonly ICatalog Catalog;
    private readonly string? _commandText;
    private readonly IDictionary<string, object> _parameters;
    private readonly CommandType _commandType;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="commandText">Name of SQL procedure or Text Command</param>
    /// <param name="catalog">ICatalog</param>
    /// <param name="dao">IGenericDao providing connection</param>
    /// <param name="commandType"></param>
    public DatabaseCommand(string? commandText, ICatalog catalog, IGenericDao dao, CommandType commandType = CommandType.StoredProcedure)
    {
        _commandText = commandText;
        _dao = dao;
        Catalog = catalog;
        _commandType = commandType;
        _parameters = new Dictionary<string, object>();
    }

    public ICommand WithParameter(string name, object value)
    {
        _parameters.Add(name, value);
        return this;
    }

    public virtual IList<T> AsListOf<T>() where T : class, new()
    {
        using var cmd = GetCommand();
        var mapper = Catalog.GetPropertyMapper<T>();
        using var reader = CommandRunner.Run(cmd, x => x.ExecuteReader());
        var result = mapper.MapAll(reader);
        reader.Close();
        return result;
    }

    public virtual IList<T> AsListOf<T>(out int returnValue) where T : class, new()
    {
        using var cmd = GetCommand();
        cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
        var mapper = Catalog.GetPropertyMapper<T>();

        using var reader = CommandRunner.Run(cmd, x => x.ExecuteReader());
        var result = mapper.MapAll(reader);
        reader.Close();
        returnValue = (int)cmd.Parameters["@RETURN_VALUE"].Value;
        return result;
    }
        
    public void AsVoid()
    {
        using var cmd = GetCommand();
        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());
    }

    public int AsFunction()
    {
        using var cmd = GetCommand();
        cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());

        return (int)cmd.Parameters["@RETURN_VALUE"].Value;
    }

    public T AsFunction<T>()
    {
        using var cmd = GetCommand();
        SqlDbType returnType;
        if (typeof(T) == typeof(int))
        {
            returnType = SqlDbType.Int;
        }
        else if (typeof(T) == typeof(string))
        {
            returnType = SqlDbType.NVarChar;
        }
        else
        {
            throw new Exception($"Invalid Type {typeof(T)}");
        }

        cmd.Parameters.Add("@RETURN_VALUE", returnType).Direction = ParameterDirection.ReturnValue;

        CommandRunner.Run(cmd, x => x.ExecuteNonQuery());

        return (T)cmd.Parameters["@RETURN_VALUE"].Value;
    }

    public IList<IDictionary<string, object?>> AsList()
    {
        using var cmd = GetCommand();
        var result = new List<IDictionary<string, object?>>();
        
        using var reader = CommandRunner.Run(cmd, x => x.ExecuteReader());
        while (reader.Read())
        {
            var record = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                var fieldName = reader.GetName(i);
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = "NN";
                }

                AddValue(record, fieldName, value);
            }

            result.Add(record);
        }

        reader.Close();
        return result;
    }

    private void AddValue(IDictionary<string, object?> record, string fieldName, object value)
    {
        var iPos = fieldName.IndexOf('.');
        if (iPos > 0)
        {
            var parentName = fieldName.Substring(0, iPos);
            if (!record.ContainsKey(parentName))
            {
                record.Add(parentName, new Dictionary<string, object?>());
            }

            AddValue((IDictionary<string, object?>)record[parentName]!, fieldName.Substring(iPos + 1), value);
            return;
        }

        if (record.ContainsKey(fieldName))
        {
            var j = 0;
            while (record.ContainsKey($"{fieldName}_{j}"))
            {
                j++;
            }

            fieldName = $"{fieldName}_{j}";
        }

        record.Add(fieldName, Equals(value, DBNull.Value) ? null : value);
    }

    /// <summary>
    /// Prepare command and set parameters
    /// </summary>
    /// <returns></returns>
    protected SqlCommand GetCommand()
    {
        var cmd = _dao.GetSqlCommand(_commandText);
        cmd.CommandType = _commandType;
        foreach (var key in _parameters.Keys)
        {
            // special handling for structured types
            if (_parameters[key] is TableValueParameter tvp)
            {
                var table = new DataTable();
                foreach (var columnName in tvp.Columns.Keys)
                {
                    table.Columns.Add(columnName, tvp.Columns[columnName]);
                }

                foreach (var row in tvp.Rows)
                {
                    table.Rows.Add(row.ToArray());
                }

                var param = cmd.Parameters.AddWithValue(key, table);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = tvp.TypeName;

                continue;
            }

            // default handling for integral types
            cmd.Parameters.AddWithValue(key, _parameters[key] ?? DBNull.Value);
        }

        return cmd;
    }
}