namespace ServiceBooking.Api.DTOs.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages
    {
        get
        {
            if(TotalCount == 0)
            {
                return 0;
            }
            else
            {
                return (int)Math.Ceiling(TotalCount / (double)PageSize);
            }
        }
    }
}