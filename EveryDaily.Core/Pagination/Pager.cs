namespace EveryDaily.Core.Pagination;

public class Pager
{
    public decimal PageNumber { get; set; }
    public decimal PageSize { get; set; }

    public decimal TotalPages =>
        Convert.ToInt32(Math.Ceiling((double)TotalRecords / (PageSize == 0 ? 1 : (double)PageSize)));

    public decimal TotalRecords { get; set; }
}