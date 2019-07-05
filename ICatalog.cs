namespace Net.Arqsoft.QsMapper
{
    /// <summary>
    /// Mapping catalog.
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// Return the configured map for a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        TableMap<T> GetTableMap<T>() where T : class, new();

        /// <summary>
        /// Creates a new TableMap for a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        TableMap<T> RegisterMap<T>() where T : class, new();

        /// <summary>
        /// Returns a mapper for a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        PropertyMapper<T> GetPropertyMapper<T>() where T : class, new();
    }
}