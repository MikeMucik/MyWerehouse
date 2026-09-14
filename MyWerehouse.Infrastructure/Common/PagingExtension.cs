using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;

namespace MyWerehouse.Infrastructure.Common
{
	public static class PagingExtension
	{
		public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
			this IQueryable<T> query,
			int pageNumber,
			int pageSize,
			CancellationToken ct)
		{
			pageNumber = pageNumber <= 0 ? 1 : pageNumber;
			pageSize = pageSize <= 0 ? 10 : pageSize;
			var totalCount = await query.CountAsync(ct);

			var items = await query
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync(ct);

			return new PagedResult<T>
			{
				Items = items,
				TotalCount = totalCount,
				CurrentPage = pageNumber,
				PageSize = pageSize,				
			};
		}
	}
}
