using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.CategoryModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface ICategoryService
	{		
		Task<AppResult<Unit>> AddCategoryAsync(CategoryDTO categoryDTO);		
		Task<AppResult<Unit>> DeleteCategoryAsync(int id);
		Task<AppResult<Unit>> UpdateCategoryAsync(CategoryDTO categoryDTO);
		Task<AppResult<ListCategoriesDTO>> GetCategoriesAsync(int pageSize, int pageNumber);
	}
}
