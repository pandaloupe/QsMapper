using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Arqsoft.QsMapper.CommandBuilder
{
    public class TableValueParameter
    {
        public string TypeName { get; set; }
        public IDictionary<string, Type> Columns { get; set; }
        public List<List<object>> Rows { get; set; }
    }
}
