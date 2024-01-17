using System.Data;
using System.Data.SqlClient;
using Net.Arqsoft.QsMapper.Util;

namespace Net.Arqsoft.QsMapper;

public class TableInfo
{
    public IList<TableColumn> Columns { get; }

    public TableInfo(SqlDataReader reader)
    {
        var schema = reader.GetSchemaTable();
        if (schema == null)
        {
            throw new NullReferenceException("Table schema could not be retrieved.");
        }

        var readonlyColumn = -1;
        var nameColumn = -1;
        foreach (DataColumn column in schema.Columns)
        {
            if (column.ColumnName == "IsReadOnly")
            {
                readonlyColumn = column.Ordinal;
            }
            else if (column.ColumnName == "ColumnName")
            {
                nameColumn = column.Ordinal;
            }

            if (readonlyColumn > -1 && nameColumn > -1)
            {
                break;
            }
        }

        if (readonlyColumn == -1)
        {
            throw new NullReferenceException("IsReadOnly column could not be determined for table schema.");
        }

        if (nameColumn == -1)
        {
            throw new NullReferenceException("Name column could not be determined for table schema.");
        }

        Columns = new List<TableColumn>();

        foreach (DataRow row in schema.Rows)
        {
            Columns.Add( new TableColumn
            {
                ColumnName = (string)row[nameColumn],
                IsReadOnly = (bool)row[readonlyColumn]
            });
        }
    }

    public IList<string> WriteableFields => Columns.Where(x => !x.IsReadOnly).Select(x => x.ColumnName).ToList();
}