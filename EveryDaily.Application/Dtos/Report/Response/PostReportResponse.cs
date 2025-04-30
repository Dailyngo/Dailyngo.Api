using EveryDaily.Core.Dtos;

namespace EveryDaily.Application.Dtos.Report.Response;

public class PostReportResponse
{
    public string Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? LastReportDate => ReportDetails.FirstOrDefault()?.CreatedAt;
    public List<ReportDetailResponse> ReportDetails { get; set; }
}

public class ReportDetailResponse
{
    public string Id { get; set; }
    public IdNameResponse<Guid> ReportedBy { get; set; }
    public string? Reason { get; set; }
    public bool IsProcess { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}