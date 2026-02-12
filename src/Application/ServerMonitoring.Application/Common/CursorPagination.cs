using Microsoft.EntityFrameworkCore;

namespace ServerMonitoring.Application.Common;

/// <summary>
/// Cursor-based pagination for efficient large dataset traversal
/// Better performance than offset pagination for real-time data
/// </summary>
public class CursorPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    
    public CursorPagedResult()
    {
    }

    public CursorPagedResult(List<T> items, string? nextCursor, string? previousCursor, int pageSize, bool hasNext, bool hasPrevious)
    {
        Items = items;
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
        PageSize = pageSize;
        HasNextPage = hasNext;
        HasPreviousPage = hasPrevious;
    }
}

/// <summary>
/// Cursor pagination parameters
/// </summary>
public class CursorPaginationParams
{
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
    public string Direction { get; set; } = "next"; // next or previous

    public bool IsForward => Direction?.ToLower() == "next";
}

/// <summary>
/// Extension methods for cursor-based pagination
/// </summary>
public static class CursorPaginationExtensions
{
    /// <summary>
    /// Apply cursor pagination to IQueryable
    /// Cursor is base64 encoded ID
    /// </summary>
    public static async Task<CursorPagedResult<T>> ToCursorPagedListAsync<T>(
        this IQueryable<T> query,
        Func<T, int> keySelector,
        CursorPaginationParams paginationParams,
        CancellationToken cancellationToken = default) where T : class
    {
        var pageSize = Math.Min(paginationParams.PageSize, 100); // Max 100 items
        var isForward = paginationParams.IsForward;

        IQueryable<T> pagedQuery = query;

        // Decode cursor
        int? cursorId = null;
        if (!string.IsNullOrEmpty(paginationParams.Cursor))
        {
            try
            {
                var decodedCursor = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(paginationParams.Cursor));
                cursorId = int.Parse(decodedCursor);
            }
            catch
            {
                // Invalid cursor, ignore
            }
        }

        // Apply cursor filter
        if (cursorId.HasValue)
        {
            if (isForward)
            {
                pagedQuery = pagedQuery.Where(x => keySelector(x) > cursorId.Value);
            }
            else
            {
                pagedQuery = pagedQuery.Where(x => keySelector(x) < cursorId.Value);
            }
        }

        // Order by ID
        if (isForward)
        {
            pagedQuery = pagedQuery.OrderBy(x => keySelector(x));
        }
        else
        {
            pagedQuery = pagedQuery.OrderByDescending(x => keySelector(x));
        }

        // Take one extra to check if there's a next page
        var items = await pagedQuery.Take(pageSize + 1).ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items = items.Take(pageSize).ToList();
        }

        // If going backward, reverse the list
        if (!isForward)
        {
            items.Reverse();
        }

        // Generate cursors
        string? nextCursor = null;
        string? previousCursor = null;

        if (items.Any())
        {
            var lastId = keySelector(items.Last());
            var firstId = keySelector(items.First());

            if (hasMore && isForward)
            {
                nextCursor = EncodeCursor(lastId);
            }

            if (cursorId.HasValue || (!isForward && hasMore))
            {
                previousCursor = EncodeCursor(firstId);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        return new CursorPagedResult<T>(
            items,
            nextCursor,
            previousCursor,
            pageSize,
            hasMore && isForward,
            cursorId.HasValue || (!isForward && hasMore));
    }

    private static string EncodeCursor(int id)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(id.ToString());
        return Convert.ToBase64String(bytes);
    }
}
