namespace MyWerehouse.Domain.Models
{
	public class Product
	{
		public int Id { get; set; }
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
	}
}
