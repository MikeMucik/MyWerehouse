using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace MyWerehouse.Application.Common.Pagination
{
	public static class PagingExtension
	{
		public static async Task<PagedResult<TDestination>> ToPagedResultAsync<TSource, TDestination>(
			this IQueryable<TSource> query,
			IConfigurationProvider mappConfig,
			int currentPage,
			int pageSize,
			CancellationToken ct)
		{
			var totalCount = await query.CountAsync(ct);

			var items = await query
				.Skip(pageSize * (currentPage - 1))
				.Take(pageSize)
				.ProjectTo<TDestination>(mappConfig)
				.ToListAsync(ct);

			return new PagedResult<TDestination>
			{
				Dtos = items,
				TotalCount = totalCount,
				CurrentPage = currentPage,
				PageSize = pageSize,
			};
		}
	}
}
