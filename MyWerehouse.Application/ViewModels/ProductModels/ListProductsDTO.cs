using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class ListProductsDTO
	{
		public List<ProductToListDTO> products {  get; set; }
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }		
		public int Count { get; set; }
	}
}
