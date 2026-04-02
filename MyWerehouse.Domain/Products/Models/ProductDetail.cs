using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Products.Models
{
	public class ProductDetail
	{
		public Guid ProductId { get; private set; }
		public Product Product { get;private set; }
		public int Length { get;private set; }
		public int Height { get;private set; }
		public int Width { get;private set; }
		public int Weight { get;private set; }
		public string Description { get;private set; }

		private ProductDetail() { }

		private ProductDetail(Guid productId,int length, int height, int width, int weight, string description)
		{			
			ProductId = productId;
			if (length < 0 || length > 120) throw new ArgumentException("Not corect size of length");//cm
			Length = length;
			if (height < 0 || height > 220) throw new ArgumentException("Not corect size of height");//cm
			Height = height;
			if (width < 0 || width > 120) throw new ArgumentException("Not corect size of width");//cm
			Width = width;
			if (weight < 0 || weight > 50000) throw new ArgumentException("Not corect weight"); //grams
			Weight = weight;
			Description = description;
		}
		public static ProductDetail CreateDetails(Guid productId, int length, int height, int width, int weight, string description)
			=>new ProductDetail(productId, length, height, width, weight, description);
	}
}
