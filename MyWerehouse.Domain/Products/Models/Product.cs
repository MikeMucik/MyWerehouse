using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Domain.Products.ProductsExceptions;

namespace MyWerehouse.Domain.Products.Models
{
	public class Product
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public string SKU { get; private set; } = string.Empty;
		public DateTime AddedAd { get; private set; }
		public int CategoryId { get; private set; }
		public Category Category { get; private set; } = null!;
		public bool IsDeleted { get; private set; } = false;
		public int CartonsPerPallet { get; private set; }
		public ProductDetail Details { get; private set; } = null!;
		public Inventory InventoryItem { get; private set; } = null!;
		private Product() { } //EF
		private Product(string name, string sku, DateTime createdAt, int categoryId, bool isDeleted, int cartonsPerPallets,
			int length, int height, int width, int weight, string description)
		{
			if (cartonsPerPallets <= 0) throw new PalletCartonQuantityMustBePositiveDomainException();
			Id = Guid.NewGuid();
			Name = name;
			SKU = sku;
			AddedAd = createdAt;
			CategoryId = categoryId;
			IsDeleted = isDeleted;
			CartonsPerPallet = cartonsPerPallets;
			Details = ProductDetail.CreateDetails(Id, length, height, width, weight, description);
		}
		public static Product Create(string name, string sku, DateTime createdAt, int categoryId, int cartonsPerPallets,
			int length, int height, int width, int weight, string description)
		=> new Product(name, sku, createdAt, categoryId, false, cartonsPerPallets, length, height, width, weight, description);

		private Product(Guid id, string name, string sku, DateTime addedAd, int categoryId, bool isDeleted, int cartonsPerPallet)
		{
			if (cartonsPerPallet <= 0) throw new PalletCartonQuantityMustBePositiveDomainException();
			Id = id;
			Name = name;
			SKU = sku;
			AddedAd = addedAd;
			CategoryId = categoryId;
			IsDeleted = isDeleted;
			CartonsPerPallet = cartonsPerPallet;
			Details = ProductDetail.CreateDetails(Id, 30, 30, 30, 30, "Test");
		}
		public static Product CreateForTests(Guid id, string name, string SKU,
			DateTime addedItemAd, int categoryId, bool isDeleted, int cartonsPerPallet)
		=> new Product(id, name, SKU, addedItemAd, categoryId, isDeleted, cartonsPerPallet);
		private Product(Guid id, string name, string sku, DateTime createdAt, int categoryId, bool isDeleted, int cartonsPerPallets,
			int length, int height, int width, int weight, string description)
		{
			if (cartonsPerPallets <= 0) throw new PalletCartonQuantityMustBePositiveDomainException();
			Id = id;
			Name = name;
			SKU = sku;
			AddedAd = createdAt;
			CategoryId = categoryId;
			IsDeleted = isDeleted;
			CartonsPerPallet = cartonsPerPallets;
			Details = ProductDetail.CreateDetails(Id, length, height, width, weight, description);
		}
		public static Product CreateForSeed(Guid id, string name, string SKU,
		DateTime addedItemAd, int categoryId, bool isDeleted, int cartonsPerPallet,
			int length, int height, int width, int weight, string description)
	=> new Product(id, name, SKU, addedItemAd, categoryId, isDeleted, cartonsPerPallet, length, height, width, weight, description);

		public void Hide()
		{
			this.IsDeleted = true;
		}

		public void SetCategory(Category category)
		{
			Category = category;
			CategoryId = category.Id;
		}

		public void ApplyChangesForProduct(
			string name, string sku, int categoryId,
			int cartonsPerPallet, int length, int height,
			int width, int weight, string description)
		{
			if (cartonsPerPallet <= 0) throw new PalletCartonQuantityMustBePositiveDomainException();
			Name = name;
			SKU = sku;
			CategoryId = categoryId;
			CartonsPerPallet = cartonsPerPallet;
			Details = ProductDetail.CreateDetails(Id, length, height, width, weight, description);
		}
	}
}
