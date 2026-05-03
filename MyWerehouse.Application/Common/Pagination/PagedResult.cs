using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Receipts.DTOs;

namespace MyWerehouse.Application.Common.Pagination
{
	public class PagedResult<T>
	{
		public IReadOnlyList<T> Dtos { get; init; } = new List<T>();
		public int CurrentPage { get; init; }
		public int PageSize { get; init; }
		public int TotalCount { get; init; }
		public int TotalPages  => (int)Math.Ceiling((double)TotalCount/PageSize);
		public bool HasNext => CurrentPage < TotalPages;
		public bool HasPrevious => CurrentPage > 1;
	}
}
