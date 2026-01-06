using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Products.Models
{
	public class ProductDetail
	{		
		public int Length { get; set; }
		public int Height {  get; set; }
		public int Width { get; set; }
		public int Weight { get; set; }
		public string Description {  get; set; }
		public int ProductId { get; set; }
		public Product Product { get; set; }
	}
}
