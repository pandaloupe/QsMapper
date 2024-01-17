namespace Net.Arqsoft.QsMapper.CommandBuilder;

public class TableValueParameter
{
    public string TypeName { get; set; }
    public IDictionary<string, Type> Columns { get; set; }
    public List<List<object>> Rows { get; set; }
}