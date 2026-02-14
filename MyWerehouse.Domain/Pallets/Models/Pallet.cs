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
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.PalletDto;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet : AggregateRoots
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }
		public PalletStatus Status { get; set; } = 0;
		public virtual ICollection<ProductOnPallet> ProductsOnPallet { get; set; } = new List<ProductOnPallet>();
		public virtual ICollection<PalletMovement> PalletMovements { get; set; } = new List<PalletMovement>();
		public int? ReceiptId { get; set; }
		public virtual Receipt? Receipt { get; set; }
		public int? IssueId { get; set; }
		public virtual Issue? Issue { get; set; }
		[Timestamp]

		public byte[] RowVersion { get; set; }
		public Pallet() { }
		public Pallet(string id, DateTime dateReceived, ICollection<ProductOnPallet> productsOnPallet)
		{
			Id = id;
			DateReceived = dateReceived;
			ProductsOnPallet = productsOnPallet;
		}
		public Pallet(string id, DateTime dateReceived, int locationId, Location location)
		//, PalletStatus status, int? receiptId, Receipt? receipt)
		{
			Id = id;
			DateReceived = dateReceived;
			LocationId = locationId;
			Location = location;
			//Status = status;
			//ReceiptId = receiptId;
			//Receipt = receipt;
		}

		public void AssignToWarehouse(Location location, string userId)
		{
			Status = PalletStatus.InStock;
			Location = location;

			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				location.Id, location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));
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
		public void UpdateProductChanges(List<ProductOnPalletDto> updatedProducts)
		{
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
		}
		public void AddProduct(int productId, int quantity, DateOnly? bestBefore)
		{
			if (ProductsOnPallet is null)
				throw new DomainPalletException("Paleta musi zawierać produkty.");
			this.ProductsOnPallet.Add(new ProductOnPallet
			{
				ProductId = productId,
				Quantity = quantity,
				DateAdded = DateTime.UtcNow,
				BestBefore = bestBefore
			});
		}
		public void ChangeStatus(PalletStatus status, ReasonMovement reason, string userId)
		{
			this.Status = status;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), reason, userId, this.Status, BuildMovementDetails()));
		}
		public void RemoveProducts(List<int> ids)
		{
			foreach (var id in ids)
			{
				var pop = ProductsOnPallet.First(x => x.ProductId == id);
				ProductsOnPallet.Remove(pop);
			}
		}
		public List<StockProductChange> CalculateQuantityDelta(IEnumerable<ProductOnPallet> updatedProducts)//It must be done before update
		{
			var result = new List<StockProductChange>();
			var updatedById = updatedProducts.ToDictionary(x => x.ProductId, x => x.Quantity);
			var allIds = ProductsOnPallet.Select(x => x.ProductId).Union(updatedProducts.Select(p => p.ProductId));
			foreach (var id in allIds)
			{
				var oldQty = ProductsOnPallet.FirstOrDefault(p => p.ProductId == id)?.Quantity ?? 0;
				updatedById.TryGetValue(id, out var newQty);
				var delta = newQty - oldQty;
				if (delta != 0)
				{
					result.Add(new StockProductChange(id, delta));
				}
			}
			return result;
		}
		public void AssignToIssue(Issue issue, Location location, string userId)
		{
			if (issue == null) throw new DomainIssueException(this.Id);
			//IssueId = issue.Id;
			Status = PalletStatus.InTransit;

			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				location.Id, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));
		}
		public void DetachToIssue(int issueId, string userId)
		{
			if (IssueId != issueId)
				throw new DomainIssueException(Id);
			IssueId = null;
			Status = PalletStatus.Available;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				Location.Id, Location.ToSnopShot(), Location.Id, Location.ToSnopShot(), ReasonMovement.CancelIssue, userId, this.Status, BuildMovementDetails()));
		}

		public void AssignToPicking(Location location, string userId)
		{
			Status = PalletStatus.ToPicking;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				location.Id, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Picking, userId, this.Status, BuildMovementDetails()));
		}

		public void AssignToReceipt(int receiptId, string userId)
		{
			if (ReceiptId != null) throw new DomainReceiptException(this.Id);
			ReceiptId = receiptId;
			Status = PalletStatus.Receiving;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
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
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				LocationId, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Moved, userId, this.Status, BuildMovementDetails()));
			this.LocationId = location.Id;
		}
		public void CloseAndAddPickingPallet(int issueId, string userId, Location location)
		{
			if (Status != PalletStatus.Picking)
				throw new DomainPalletException(this.Id);
			Status = PalletStatus.ToIssue;
			IssueId = issueId;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id,
				location.Id, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));
		}
		//metody pomocnicze
		private IReadOnlyCollection<PalletMovementDetailDto> BuildMovementDetails()
		{
			return ProductsOnPallet
				.Select(p => new PalletMovementDetailDto
				(
					p.ProductId,
					p.Quantity
				))
				.ToList();
		}
	}
}

//ChangeStatus(PalletStatus status)

//AssignToIssue(int issueId)

//DetachFromIssue()(opcjonalnie)

//AssignToReceipt(int receiptId)

//MoveToLocation(int locationId)

//SetDateReceived(DateTime date)
//AddOrUpdateProduct(int productId, int quantity)

//RemoveProduct(int productId)
//CanModifyProducts()

//CanChangeStatusTo(PalletStatus status)

//IsAssignedToIssue()
//public void UpdateListPallets(List<Pallet> updatedPallets)
//{
//	//do poprawy!!!
//	//var updatedByPallets = updatedPallets.ToDictionary(x => x.Id);
//	//var toRemove = ProductPallet

//	foreach (var itemToUpadate in updatedPallets)
//	{
//		this.DateReceived = DateTime.UtcNow;
//		this.IssueId = itemToUpadate.IssueId;
//		this.Issue = itemToUpadate.Issue;
//		this.LocationId = itemToUpadate.LocationId;
//		this.Location = itemToUpadate.Location;
//		this.Receipt = itemToUpadate.Receipt;
//		this.ReceiptId = itemToUpadate.ReceiptId;
//		this.Status = itemToUpadate.Status;

//		//this.ProductsOnPallet = 
//			UpdateProducts(itemToUpadate.ProductsOnPallet.ToList());				
//	}
//}