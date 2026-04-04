using System.Xml.Linq;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Products.Models
{
	public class Product
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public string SKU { get; private set; }
		public DateTime AddedAd { get; private set; }
		public int CategoryId { get; private set; }
		public Category Category { get; private set; }
		public bool IsDeleted { get; private set; } = false;
		public int CartonsPerPallet { get; private set; }
		public ICollection<Receipt> ReceiptList { get; private set; } = new List<Receipt>();
		public ICollection<Issue> IssueList { get; private set; } = new List<Issue>();
		public ProductDetail Details { get; private set; }
		public Inventory InventoryItem { get; private set; }
		private Product() { } //EF
		private Product(string name, string sku, DateTime addedAd, int categoryId, bool isDeleted, int cartonsPerPallets, ProductDetail? details = null)
		{
			if (cartonsPerPallets <= 0) throw new ArgumentException("Cartoons on pallet must be more than zero.");
			Id = Guid.NewGuid();
			Name = name;
			SKU = sku;
			AddedAd = addedAd;
			CategoryId = categoryId;
			IsDeleted = isDeleted;
			CartonsPerPallet = cartonsPerPallets;
			Details = details;
		}
		public static Product Create(string name, string sku, int categoryId, int cartonsPerPallets, ProductDetail? details = null)
		=> new Product(name, sku, DateTime.UtcNow, categoryId, false, cartonsPerPallets, details);

		private Product(Guid id, string name, string sku, DateTime addedAd, int categoryId, bool isDeleted, int cartonsPerPallet)
		{
			if (cartonsPerPallet <= 0) throw new ArgumentException("Cartoons on pallet must be more than zero.");
			Id = id;
			Name = name;
			SKU = sku;
			AddedAd = addedAd;
			CategoryId = categoryId;
			IsDeleted = isDeleted;
			CartonsPerPallet = cartonsPerPallet;
		}
		public static Product CreateForSeed(Guid id, string name, string SKU,
			DateTime addedItemAd, int categoryId, bool isDeleted, int cartonsPerPallet)

		=> new Product(id, name, SKU, addedItemAd, categoryId, isDeleted, cartonsPerPallet);

		public void Hide()
		{
			this.IsDeleted = true;
		}
		public void SetDetails(ProductDetail details)
		{
			this.Details = details ?? throw new ArgumentNullException(nameof(details));
		}
		public void SetCategory(Category category)
		{
			Category = category;
			CategoryId = category.Id;
		}
	}
}
