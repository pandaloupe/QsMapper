namespace Net.Arqsoft.QsMapper.CommandBuilder;

public class TableValueParameter
{
    public string TypeName { get; set; } = "System.Array";
    public IDictionary<string, Type> Columns { get; set; } = new Dictionary<string, Type> { { "Id", typeof(int) } };
    public List<List<object>> Rows { get; set; } = new();
}