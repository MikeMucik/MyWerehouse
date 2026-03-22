using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Events;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet : AggregateRoots
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string PalletNumber { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }
		public PalletStatus Status { get; set; } = 0;
		public virtual ICollection<ProductOnPallet> ProductsOnPallet { get; set; } = new List<ProductOnPallet>();
		public virtual ICollection<PalletMovement> PalletMovements { get; set; } = new List<PalletMovement>();
		public Guid? ReceiptId { get; set; }
		public virtual Receipt? Receipt { get; set; }
		public Guid? IssueId { get; set; }
		public virtual Issue? Issue { get; set; }
		[Timestamp]
		public byte[] RowVersion { get; set; } //działa tylko w M-SQL wymaga DbUpdateConcurrencyException
		public Pallet() { }
		public Pallet(
			//Guid id ,
			string palletId, DateTime dateReceived, ICollection<ProductOnPallet> productsOnPallet)
		{
			//Id = id;
			PalletNumber = palletId;
			DateReceived = dateReceived;
			ProductsOnPallet = productsOnPallet;
		}
		public Pallet(
			//Guid id,
			string palletId, DateTime dateReceived, int locationId, Location location)
		{
			//Id = id;
			PalletNumber = palletId;
			DateReceived = dateReceived;
			LocationId = locationId;
			Location = location;
		}

		public void AssignToWarehouse(Location location, string userId, List<ProductOnPallet> products)
		{
			var listProducts = this.CreateStockItem();
			Status = PalletStatus.InStock;
			Location = location;

			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				location.Id, location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));
			this.AddDomainEvent(new ChangeStockNotification(listProducts));
		}

		public void ApplyProductChanges(List<ProductOnPallet> updatedProducts)
		{
			foreach (var pop in updatedProducts)
			{
				var existing = ProductsOnPallet
					.SingleOrDefault(x => x.ProductId == pop.ProductId);

				if (existing == null)
					ProductsOnPallet.Add(pop);
				else
				{
					existing.Quantity = pop.Quantity;
					existing.BestBefore = pop.BestBefore;
				}
			}
		}
		public void Update(string userId, List<ProductOnPallet> products, PalletStatus palletStatus)
		{
			Status = palletStatus;
			this.UpdateProductChanges(products);
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.Correction, userId, this.Status, BuildMovementDetails()));

		}
		public void UpdateProductChanges(List<ProductOnPallet> updatedProducts)
		{
			var changeQuangtityInventory = this.CalculateQuantityDelta(updatedProducts);
			var toRemove = ProductsOnPallet
				.Where(existing => updatedProducts.All(d => d.ProductId != existing.ProductId))
				.ToList();
			foreach (var item in toRemove)
			{
				ProductsOnPallet.Remove(item);
			}
			foreach (var pop in updatedProducts)
			{
				var existing = ProductsOnPallet
					.SingleOrDefault(x => x.ProductId == pop.ProductId);

				if (existing == null)
				{
					var newProduct = new ProductOnPallet
					{
						ProductId = pop.ProductId,
						Quantity = pop.Quantity,
						BestBefore = pop.BestBefore,
						DateAdded = DateTime.UtcNow,
					};
					ProductsOnPallet.Add(newProduct);
				}
				else
				{
					existing.Quantity = pop.Quantity;
					existing.BestBefore = pop.BestBefore;
				}
			}
			this.AddDomainEvent(new ChangeStockNotification(changeQuangtityInventory));
		}
		public void AddProduct(Guid productId, int quantity, DateOnly? bestBefore)
		{
			if (ProductsOnPallet is null)
				throw new DomainProductOnPalletException(Id, PalletNumber);
					//("Paleta musi zawierać produkty.");
			this.ProductsOnPallet.Add(new ProductOnPallet
			{
				ProductId = productId,
				Quantity = quantity,
				DateAdded = DateTime.UtcNow,
				BestBefore = bestBefore
			});
		}
		public void AddHistory(PalletStatus status, ReasonMovement reason, string userId)
		{
			this.Status = status;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), reason, userId, this.Status, BuildMovementDetails()));
		}

		//
		public void RemoveProducts(List<Guid> ids)
		{
			foreach (var id in ids)
			{
				var pop = ProductsOnPallet.First(x => x.ProductId == id);
				ProductsOnPallet.Remove(pop);
			}
		}
		public List<StockItemChange> CalculateQuantityDelta(IEnumerable<ProductOnPallet> updatedProducts)//It must be done before update
		{
			var result = new List<StockItemChange>();
			var updatedById = updatedProducts.ToDictionary(x => x.ProductId, x => x.Quantity);
			var allIds = ProductsOnPallet.Select(x => x.ProductId).Union(updatedProducts.Select(p => p.ProductId));
			foreach (var id in allIds)
			{
				var oldQty = ProductsOnPallet.FirstOrDefault(p => p.ProductId == id)?.Quantity ?? 0;
				updatedById.TryGetValue(id, out var newQty);
				var delta = newQty - oldQty;
				if (delta != 0)
				{
					result.Add(new StockItemChange(id, delta));
				}
			}
			return result;
		}
		//
		public void AssignToIssue(Issue issue, string userId)
		{
			if (issue == null) throw new DomainIssueException(issue.IssueNumber);
			//IssueId = issue.Id;
			Status = PalletStatus.InTransit;

			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));
		}
		public void CancelFromReceipt(string userId)
		{
			Status = PalletStatus.Cancelled;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));

		}
		public void DetachToIssue(Guid issueId, string userId)
		{
			if (IssueId != issueId)
				throw new DomainIssueException("Niepoprawne wydanie dla palety");
			IssueId = null;
			Status = PalletStatus.Available;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				Location.Id, Location.ToSnopShot(), Location.Id, Location.ToSnopShot(), ReasonMovement.CancelIssue, userId, this.Status, BuildMovementDetails()));
		}

		public void AssignToPicking(string userId)
		{
			Status = PalletStatus.ToPicking;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.Picking, userId, this.Status, BuildMovementDetails()));
		}

		public void AssignToReceipt(Guid receiptId, string userId)
		{
			if (ReceiptId != null) throw new DomainReceiptException(this.Id,this.PalletNumber);
			ReceiptId = receiptId;
			Status = PalletStatus.Receiving;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));
		}
		//public void DetachToReceipt(int receiptId, Location location, string userId)
		//{
		//	if (ReceiptId != receiptId)	throw new DomainIssueException(Id);
		//	IssueId = null;
		//	Status = PalletStatus.Available;
		//	//this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
		//	//	location.Id, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.CancelIssue, userId, this.Status, BuildMovementDetails()));
		//}
		public void MoveToLocation(Location location, string userId)
		{
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Moved, userId, this.Status, BuildMovementDetails()));
			this.LocationId = location.Id;
		}
		public void CloseAndAddPickingPallet(Guid issueId, string userId, Location location)
		{
			if (Status != PalletStatus.Picking)
				throw new DomainPalletException(this.Id, PalletNumber);
			Status = PalletStatus.ToIssue;
			IssueId = issueId;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				location.Id, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));
		}
		public bool CanBeCancelled()
		{
			if (PalletMovements.Count > 1)
				return false;
			return true;
		}





		//metody pomocnicze
		private IReadOnlyCollection<PalletMovementDetail> BuildMovementDetails()
		{
			return ProductsOnPallet
				.Select(p => new PalletMovementDetail
				(
					p.ProductId,
					p.Quantity
				))
				.ToList();
		}
		private IEnumerable<StockItemChange> CreateStockItem()
		{
			return ProductsOnPallet
				.GroupBy(p => p.ProductId)
				.Select(g => new StockItemChange(
					g.Key,
					g.Sum(q => q.Quantity)));
		}
	}
}