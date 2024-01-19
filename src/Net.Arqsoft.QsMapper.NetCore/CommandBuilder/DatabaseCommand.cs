using System.Data;
using System.Data.SqlClient;

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