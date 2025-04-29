namespace EveryDaily.Core.Dtos;

public class IdNameResponse<T>
{
    public T Id { get; set; }
    public string Name { get; set; }
}