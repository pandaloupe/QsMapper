using System.Collections.Generic;
using System.Linq;

namespace Net.Arqsoft.QsMapper.Model
{
    public class CompositeKey
    {
        private readonly IDictionary<string, object> _fields;

        public CompositeKey(IDictionary<string, object> fields)
        {
            _fields = fields ?? new Dictionary<string, object>();
        }

        public object this[string index]
        {
            get => _fields?[index];
            set
            {
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

        public override bool Equals(object obj)
        {
            if (obj?.GetType() != GetType())
            {
                return false;
            }

            var otherKey = (CompositeKey) obj;
            return _fields.All(x => Equals(otherKey._fields[x.Key], x.Value));
        }

        public override int GetHashCode()
        {
            return _fields.Any() 
                ? _fields.First().GetHashCode() 
                : 0;
        }
    }
}
