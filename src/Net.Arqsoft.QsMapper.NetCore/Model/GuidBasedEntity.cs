namespace Net.Arqsoft.QsMapper.Model;

/// <summary>
/// Base class for integer based tables.
/// </summary>
[Serializable]
public class GuidBasedEntity() : Entity<Guid?>(null)
{
    /// <inheritdoc />
    public override bool IsNew => Equals(Id, Guid.Empty);
}