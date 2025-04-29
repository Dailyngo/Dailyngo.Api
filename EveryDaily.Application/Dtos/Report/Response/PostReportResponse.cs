using EveryDaily.Core.Dtos;

namespace EveryDaily.Application.Dtos.Report.Response;

public class PostReportResponse
{
    public string Id { get; set; }
    public string PostId { get; set; }
    public IdNameResponse<Guid> ReportedBy { get; set; }
    public string? Reason { get; set; }
    public bool IsProcess { get; set; }
}