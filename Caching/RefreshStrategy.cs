namespace Net.Arqsoft.QsMapper.Caching {
    ///<summary>
    /// Defines how cache should be updated.
    ///</summary>
    public enum RefreshStrategy {
        ///<summary>
        /// Cache is updated on each call of Items property.
        ///</summary>
        Auto,
        /// <summary>
        /// Cache is updated only by call of RefreshCache().
        /// </summary>
        OnDemand
    }
}
