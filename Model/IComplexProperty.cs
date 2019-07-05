namespace Net.Arqsoft.QsMapper.Model {
    /// <summary>
    /// interface for complex property definitions.
    /// </summary>
    public interface IComplexProperty {
        /// <summary>
        /// Return complex property itself.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        object GetProperty(object item);

        /// <summary>
        /// Return value from complex property.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        object GetValue(object  item);
    }
}
