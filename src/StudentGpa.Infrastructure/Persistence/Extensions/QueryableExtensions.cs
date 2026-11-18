using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Common;

namespace StudentGpa.Infrastructure.Persistence.Extensions;

internal static class QueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedList<T>(items, total, page, pageSize);
    }
}
