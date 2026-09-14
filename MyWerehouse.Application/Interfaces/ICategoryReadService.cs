using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.ViewModels.CategoryModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface ICategoryReadService
	{

		Task<PagedResult<CategoryViewDTO>> GetCategoriesAsync
			(int pageNumber,
			int pageSize,
			CancellationToken ct);
	}
}
