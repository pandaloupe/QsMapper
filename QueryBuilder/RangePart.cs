namespace Net.Arqsoft.QsMapper.QueryBuilder {
    public class RangePart {
        private readonly object _start;
        private readonly QueryParameter _host;

        public RangePart(QueryParameter host, object start) {
            _host = host;
            _start = start;
        }

        public QueryParameter And(object end) {
            _host.CompareTo = new[] {_start, end};
            return _host;
        }
    }
}
