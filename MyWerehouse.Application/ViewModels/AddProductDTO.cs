using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels
{
	public class AddProductDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string SKU { get; set; }
		public string Category { get; set; }
		public int Length { get; set; } //cm
		public int Height { get; set; } //cm
		public int Width { get; set; } //cm
		public int Weight { get; set; } //kg
		public string Description { get; set; }
		public DateTime AddedItemAd { get; set; } = DateTime.Now;
	}
}
