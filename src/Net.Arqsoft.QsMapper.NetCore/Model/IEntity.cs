namespace Net.Arqsoft.QsMapper.Model;

/// <summary>
/// Interface for all Entities.
/// </summary>
public interface IEntity<T> {
    /// <summary>
    /// Id property.
    /// </summary>
    T Id { get; set; }

    /// <summary>
    /// Name property.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Defines if this is a new unpersisted Entity.
    /// </summary>
    bool IsNew { get; }

    /// <summary>
    /// Logical deletion marker
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Entity does not exist anymore and appears only because of remaining LogEntry
    /// </summary>
    bool IsGhost { get; }
}