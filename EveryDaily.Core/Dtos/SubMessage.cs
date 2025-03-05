namespace EveryDaily.Core.Dtos;

public class SubMessage
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public TimeSpan TimeSpan { get; set; } = TimeSpan.FromDays(1);
}