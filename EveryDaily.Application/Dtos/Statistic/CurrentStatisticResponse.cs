namespace EveryDaily.Application.Dtos.Statistic;

public class CurrentStatisticResponse
{
    public long TotalPostCount { get; set; }
    public int TotalUserCount { get; set; }
    public int OnlineUserCount { get; set; }
}