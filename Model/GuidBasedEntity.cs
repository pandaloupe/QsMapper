using System;

namespace Net.Arqsoft.QsMapper.Model {
    /// <summary>
    /// Base class for integer based tables.
    /// </summary>
    [Serializable]
    public class GuidBasedEntity : Entity<Guid?>
    {
        /// <inheritdoc />
        public override bool IsNew => Equals(Id, Guid.Empty);
    }
}