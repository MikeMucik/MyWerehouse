using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Products.Models
{
	public class Product
	{
		//public Guid Id { get; private set; } = Guid.NewGuid();
		public Guid Id { get; set; } = Guid.NewGuid();
		//public int ProductId { get; set; }
		public string Name { get; set; }
		public string SKU { get; set; }		
		public DateTime AddedItemAd {  get; set; } = DateTime.Now;
		public int CategoryId { get; set; }
		public virtual Category Category { get; set; }
		public bool IsDeleted { get; set; } = false;
		public int CartonsPerPallet { get; set; }
		public virtual ICollection<Receipt> ReceiptList { get; set; } = new List<Receipt>();
		public virtual ICollection<Issue> IssueList { get; set; } = new List<Issue>();
		public ProductDetail Details { get; set; }
		public virtual Inventory InventoryItem { get; set; }
		public Product()
		{
			
		}
		public static Product Create(Guid id, string name, string SKU, int  categoryId, bool isDeleted, int cartoonPerPallets)
		{
			return new Product
			{
				Id = id,
				Name = name,
				SKU = SKU,
				CategoryId = categoryId,
				IsDeleted = isDeleted,
				CartonsPerPallet = cartoonPerPallets
			};
		}
	}
}
