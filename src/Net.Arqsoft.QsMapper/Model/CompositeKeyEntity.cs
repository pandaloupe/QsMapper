namespace Net.Arqsoft.QsMapper.Model
{
    /// <summary>
    /// A data entity which data table uses multiple columns as key
    /// Key columns must be set via CompositeId() method in catalog definition
    /// </summary>
    public class CompositeKeyEntity : Entity<CompositeKey>
    {
        /// <summary>
        /// An entity is regarded as new/unpersisted as long as the key is null
        /// </summary>
        public override bool IsNew => Id == null;
    }
}
