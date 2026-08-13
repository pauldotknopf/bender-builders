namespace BenderBuilders.Interfaces.Dtos;

/// <summary>
/// A single page of results for a paged query.
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; set; }
}
