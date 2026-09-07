using System.ComponentModel.DataAnnotations;

namespace EventBooking.Core.DTOs.Common;

/// <summary>Bound from the query string. Defaults keep pages small; the range caps abuse.</summary>
public class PageQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
