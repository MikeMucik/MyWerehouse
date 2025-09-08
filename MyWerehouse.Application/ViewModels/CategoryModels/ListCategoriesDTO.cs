using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.CategoryModels
{
	public class ListCategoriesDTO
	{
		public required List<CategoryDTO> Categories { get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int Count { get; set; }
	}
}
