namespace EveryDaily.Core.Pagination;

public class PaginationFilter
{
    private readonly int DefaultPageSize = 10;
    private readonly int MaximumPageSize = 100;

    public PaginationFilter()
    {
        PageNumber = 1;
        PageSize = DefaultPageSize;
    }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize > MaximumPageSize ? MaximumPageSize : pageSize <= 0 ? DefaultPageSize : pageSize;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}