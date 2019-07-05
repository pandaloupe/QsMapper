using System;

namespace Net.Arqsoft.QsMapper.Model {
    /// <summary>
    /// Base class for integer based tables.
    /// </summary>
    [Serializable]
    public class StringBasedEntity : Entity<string>
    {
        /// <inheritdoc />
        public override bool IsNew => string.IsNullOrEmpty(Id);
    }
}