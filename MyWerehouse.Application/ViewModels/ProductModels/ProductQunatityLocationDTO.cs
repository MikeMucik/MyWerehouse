using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class ProductQunatityLocationsDTO
	{
		
		public int ProductId { get; set; }			
		public List<QuantityLocation> ListLocation { get; set; }
	}
}
