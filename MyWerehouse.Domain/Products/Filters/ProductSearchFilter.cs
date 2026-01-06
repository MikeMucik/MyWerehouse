using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Products.Filters;
public class ProductSearchFilter
{
	public string ProductName { get; set; }
	public string SKU { get; set; }
	public string Category { get; set; }
	public int CategoryId { get; set; }
	public int Length { get; set; }
	public int Height { get; set; }
	public int Width { get; set; }
	public int Weight { get; set; }
}

