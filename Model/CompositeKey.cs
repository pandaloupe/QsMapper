using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper.Model
{
    public class CompositeKey
    {
        private IDictionary<string, object> _fields;

        public CompositeKey(IDictionary<string, object> fields)
        {
            _fields = fields;
        }

        public object this[string index]
        {
            get => _fields?[index];
            set
            {
                if (_fields == null)
                {
                    _fields = new Dictionary<string, object>();
                }

                if (!_fields.ContainsKey(index))
                {
                    _fields.Add(index, value);
                }
                else
                {
                    _fields[index] = value;
                }
            }
        }
    }
}
