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

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet : AggregateRoots
	{
		public Guid Id { get; private set; }
		public string PalletNumber { get; private set; }
		public DateTime DateReceived { get; private set; }
		public int LocationId { get; private set; }
		public virtual Location Location { get; private set; }
		public PalletStatus Status { get; private set; } = 0;
		public ICollection<ProductOnPallet> ProductsOnPallet { get; private set; } = new List<ProductOnPallet>();
		public ICollection<PalletMovement> PalletMovements { get; private set; } = new List<PalletMovement>();
		public Guid? ReceiptId { get; private set; }
		public Receipt? Receipt { get; private set; }
		public Guid? IssueId { get; private set; }
		public Issue? Issue { get; private set; }
		[Timestamp]
		public byte[] RowVersion { get; set; } //działa tylko w M-SQL wymaga DbUpdateConcurrencyException
											   //konstruktor techniczny dla seed
											   //private Pallet(Guid id, string palletNumber, ){}

		private Pallet() { }

		private Pallet(string palletNumber, DateTime dateReceived)
		{
			Id = Guid.NewGuid();
			PalletNumber = palletNumber;
			DateReceived = dateReceived;
		}

		public static Pallet Create(string palletNumber)
			=> new Pallet(palletNumber, DateTime.Now);


		private Pallet(Guid id, string palletNumber, DateTime dateReceived, int locationId, PalletStatus status, Guid? receiptId, Guid? issueId)
		{
			Id = id;
			PalletNumber = palletNumber;
			DateReceived = dateReceived;
			LocationId = locationId;
			Status = status;
			ReceiptId = receiptId;
			IssueId = issueId;
		}
		public static Pallet CreateForSeed(Guid id, string palletNumber, DateTime dateReceived, int locationId, PalletStatus status, Guid? receiptId, Guid? issueId)
		=> new Pallet(id, palletNumber, dateReceived, locationId, status, receiptId, issueId);
		private Pallet(string palletNumber, DateTime dateReceived, int locationId, PalletStatus status, Guid? receiptId, Guid? issueId)
		{
			Id = Guid.NewGuid();
			PalletNumber = palletNumber;
			DateReceived = dateReceived;
			LocationId = locationId;
			Status = status;
			ReceiptId = receiptId;
			IssueId = issueId;
		}
		public static Pallet CreateForTests(string palletNumber, DateTime dateReceived, int locationId, PalletStatus status, Guid? receiptId, Guid? issueId)
		=> new Pallet(palletNumber, dateReceived, locationId, status, receiptId, issueId);

		public void AssignToWarehouse(Location location, string userId)
		{
			var listProducts = this.CreateStockItem();
			Status = PalletStatus.InStock;
			Location = location;

			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				location.Id, location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));
			this.AddDomainEvent(new ChangeStockNotification(listProducts));
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
					ProductsOnPallet.Add(pop);
				}
				else
				{
					existing.SetQuantity(pop.Quantity);
					//existing.Quantity = pop.Quantity;
					existing.SetBestBefore(pop.BestBefore);					
					//existing.BestBefore = pop.BestBefore;
				}
			}
			this.AddDomainEvent(new ChangeStockNotification(changeQuangtityInventory));
		}
		public void AddProduct(Guid productId, int quantity, DateOnly? bestBefore)
		{
			if (ProductsOnPallet is null)
				throw new DomainProductOnPalletException(Id, PalletNumber);
			//("Paleta musi zawierać produkty.");
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, DateTime.UtcNow, bestBefore));

		}
		public void AddProductForTests(Guid productId, int quantity, DateTime dateAdd, DateOnly? bestBefore)
		{
			if (ProductsOnPallet is null)
				throw new DomainProductOnPalletException(Id, PalletNumber);
			//("Paleta musi zawierać produkty.");
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, DateTime.UtcNow, bestBefore));
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
		//
		public void ReserveToIssue(Issue issue, string userId)
		{
			if (issue == null) throw new DomainIssueException("Issue not exists.");
			if(Status != PalletStatus.Picking) 
			{
				Status = PalletStatus.InTransit;
			}
			//żeby można było dalej kompletować na tą samą paletę
			IssueId = issue.Id;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.ToLoad, userId, this.Status, BuildMovementDetails()));
		}
		public void AssignToIssue(Issue issue, string userId)
		{
			if (issue == null) throw new DomainIssueException(issue.IssueNumber);

			Status = PalletStatus.ToIssue;
			IssueId = issue.Id;
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

		public void AssignToReceipt(Guid receiptId, Location location, string userId)
		{
			//if (ReceiptId == null) throw new DomainReceiptException(this.Id, this.PalletNumber);
			if (ReceiptId != null) throw new DomainReceiptException(this.Id, this.PalletNumber);
			ReceiptId = receiptId;
			Location = location;
			Status = PalletStatus.Receiving;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));
		}		
		public void ToArchive(string userId)
		{
			Status = PalletStatus.Archived;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), ReasonMovement.Received, userId, this.Status, BuildMovementDetails()));

		}
		public void MoveToLocation(Location location, string userId)
		{
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), location.Id, Location.ToSnopShot(), ReasonMovement.Moved, userId, this.Status, BuildMovementDetails()));
			this.LocationId = location.Id;
		}
		public void AddLocation(Location location)
		{
			Location = location;
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

		public void ChangeStatus(PalletStatus status)
		{
			//invarianty!!
			if (status == PalletStatus.Archived) throw new InvalidOperationException("Pallet in archive.");
			this.Status = status;
		}
		public void AddHistory(PalletStatus status, ReasonMovement reason, string userId)
		{
			if (Status == PalletStatus.Archived) throw new InvalidOperationException("Pallet in archive.");
			//if (status == PalletStatus.Archived) throw new InvalidOperationException("Pallet in archive.");

			this.Status = status;
			this.AddDomainEvent(new ChangeStatusOfPalletNotification(this.Id, PalletNumber,
				LocationId, Location.ToSnopShot(), LocationId, Location.ToSnopShot(), reason, userId, this.Status, BuildMovementDetails()));
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