using System.Data;
using System.Data.SqlClient;

namespace Net.Arqsoft.QsMapper.CommandBuilder;

public class MultiTableCommand : DatabaseCommand, IDisposable
{
    private DataSet _dataSet;
    private int _currentTable;

    public MultiTableCommand(string commandText, ICatalog catalog, IGenericDao dao, CommandType commandType = CommandType.StoredProcedure)
        : base(commandText, catalog, dao, commandType) { }

    public MultiTableCommand(string commandText, IGenericDao dao, CommandType commandType = CommandType.StoredProcedure)
        : base(commandText, dao.Catalog, dao, commandType) { }

    public new MultiTableCommand WithParameter(string name, object value)
    {
        base.WithParameter(name, value);
        return this;
    }

    public MultiTableCommand Run()
    {
        using (var cmd = GetCommand())
        {
            using (var adapter = new SqlDataAdapter(cmd))
            {
                _dataSet = new DataSet();
                adapter.Fill(_dataSet);
            }
        }

        _currentTable = -1;
        return this;
    }

    public IList<T> MapTable<T>() where T : class, new()
    {
        _currentTable++;
        return MapTable<T>(_currentTable);
    }

    public IList<T> MapTable<T>(int tableIndex) where T : class, new()
    {
        var mapper = Catalog.GetPropertyMapper<T>();
        var result = mapper.MapAll(_dataSet.Tables[tableIndex]);
        return result;
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        _dataSet?.Dispose();
    }
}