namespace EveryDaily.Core.Entity;

public interface IEntityBase<TKey> : IEntityBase 
    where TKey : struct
{
    TKey Id { get; set; }
}

public interface IEntityBase
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}