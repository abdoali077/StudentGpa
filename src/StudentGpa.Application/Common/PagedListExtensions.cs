using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Common;

internal static class PagedListExtensions
{
    public static PagedList<TOut> Map<TIn, TOut>(
        this PagedList<TIn> source,
        Func<TIn, TOut> map) =>
        new(source.Items.Select(map).ToList(), source.TotalCount, source.Page, source.PageSize);
}
