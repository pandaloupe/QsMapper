namespace Net.Arqsoft.QsMapper.QueryBuilder
{
    /// <summary>
    /// Command type enumeration.
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// Command is based on table or view.
        /// </summary>
        TableOrView,

        /// <summary>
        /// Command is based on stored procedure.
        /// </summary>
        StoredProcedure,

        /// <summary>
        /// Command is based on function.
        /// </summary>
        Function
    }
}
