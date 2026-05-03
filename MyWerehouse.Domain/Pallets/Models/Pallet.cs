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
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet : AggregateRoots
	{
		public Guid Id { get; private set; }
		public string PalletNumber { get; private set; }
		public DateTime DateReceived { get; private set; }
		// Snapshot przechowywany jako string – uproszczenie pod potrzeby projektu/portfolio.
		// W systemie produkcyjnym byłby to Value Object (np. LocationSnapshot).
		//private readonly string _locationSnapshot;
		//private readonly string snapShoot;
		public int LocationId { get; private set; }
		public Location Location { get; private set; }//dodać kiedyś factory
		public PalletStatus Status { get; private set; } = 0;
		public ICollection<ProductOnPallet> ProductsOnPallet { get; private set; } = new List<ProductOnPallet>();
		public ICollection<PalletMovement> PalletMovements { get; private set; } = new List<PalletMovement>();
		public Guid? ReceiptId { get; private set; }
		public Receipt? Receipt { get; private set; }
		public Guid? IssueId { get; private set; }
		public Issue? Issue { get; private set; }
		[Timestamp]
		public byte[] RowVersion { get; set; } //działa tylko w M-SQL wymaga DbUpdateConcurrencyException											   

		private Pallet() { }

		private Pallet(string palletNumber, int locationId, DateTime dateReceived)
		{
			Id = Guid.NewGuid();
			PalletNumber = palletNumber;
			LocationId = locationId;
			DateReceived = dateReceived;
		}

		public static Pallet Create(string palletNumber, int locationId)
			=> new Pallet(palletNumber, locationId, DateTime.Now);

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

		public void CreateNewPalletFromReservePicking(int locationId, string snapShot, string userId)
		{
			Status = PalletStatus.InStock;
			AddHistory(ReasonMovement.ReversePicking, userId, snapShot);
		}

		public void AssignToWarehouse(int locationId, string snapShot, string userId)
		{
			var listProducts = this.CreateStockItem();
			Status = PalletStatus.InStock;
			AddHistory(ReasonMovement.New, userId, snapShot);
			this.AddDomainEvent(new ChangeStockNotification(listProducts));
		}

		public void Update(string userId, List<ProductOnPallet> products, PalletStatus palletStatus, string snapShot)
		{
			Status = palletStatus;
			this.UpdateProductChanges(products);
			AddHistory(ReasonMovement.Correction, userId, snapShot);
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
					existing.SetBestBefore(pop.BestBefore);
				}
			}
			this.AddDomainEvent(new ChangeStockNotification(changeQuangtityInventory));
		}

		public void AddProduct(Guid productId, int quantity, DateOnly? bestBefore)
		{
			if (quantity <= 0)
				throw new InvalidQunatityException(Id);
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, DateTime.UtcNow, bestBefore));
		}

		public void AddOrIncreaseProductQuantity(Guid productId, int quantity, DateOnly? bestBefore)
		{
			var existingProduct = ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
			if (existingProduct != null)
			{
				if (existingProduct.BestBefore != bestBefore)
					throw new TwoDateOneProductOnPalletException(Id);
				existingProduct.IncreaseQuantity(quantity);
				return;
			}
			AddProduct(productId, quantity, bestBefore);
		}

		public void AddProductForTests(Guid productId, int quantity, DateTime dateAdd, DateOnly? bestBefore)
		{
			if (quantity <= 0)
				throw new InvalidQunatityException(Id);
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, dateAdd, bestBefore));
		}

		//zmieniam sposób zapisywania historii dla rezerwacji bo nowa paleta
		public void ReserveToIssue(Guid issueId, string userId, string snapShot)
		{
			if (Status == PalletStatus.ToIssue)
				throw new AlreadyAssignedException(Id);

			if (Status == PalletStatus.Available || Status == PalletStatus.InStock)
			{
				Status = PalletStatus.LockedForIssue;
			}
			//żeby można było dalej kompletować na tą samą paletę
			else if (Status == PalletStatus.Picking || Status == PalletStatus.LockedForIssue)
			{
				// OK – zostaje
			}
			else
			{
				throw new InvalidPalletStatusException(Id);
			}
			IssueId = issueId;
			AddHistory(ReasonMovement.ToLoad, userId, snapShot);
		}

		public void AssignToIssue(Guid issueId, string userId, string snapShot)
		{
			//może invarianty !!
			Status = PalletStatus.ToIssue;
			IssueId = issueId;
			AddHistory(ReasonMovement.ToLoad, userId, snapShot);
		}

		public void DetachFromReceipt(string userId, string snapShot)
		{
			Status = PalletStatus.Cancelled;
			ReceiptId = null;
			AddHistory(ReasonMovement.ToLoad, userId, snapShot);
		}

		public void DetachToIssue(string userId, string snapShot, ReasonMovement reason)
		{
			IssueId = null;
			Status = PalletStatus.Available;
			AddHistory(reason, userId, snapShot);
		}

		public void AssignToPicking(string userId, string snapShot)
		{
			Status = PalletStatus.ToPicking;
			AddHistory(ReasonMovement.Picking, userId, snapShot);
		}

		public void AssignToReceipt(Guid receiptId, string snapshot, string userId)
		{
			if (Status == PalletStatus.Receiving) throw new AlreadyAssignedException(Id);
			ReceiptId = receiptId;
			Status = PalletStatus.Receiving;
			AddHistory(ReasonMovement.Received, userId, snapshot);
		}

		public void ToArchive(string userId, ReasonMovement reason, string snapShot)
		{
			//invarianty ?? 
			if (Status == PalletStatus.Archived) throw new InvalidPalletStatusException(Id);
			Status = PalletStatus.Archived;
			AddHistory(reason, userId, snapShot);
		}

		public void MoveToLocation(int newLocationId, string newLocationSnapShot, int oldLocationId, string oldLocationSnapShot, string userId)
		{
			if(this.Status == PalletStatus.InStock)
			{
				Status = PalletStatus.Available;
			}
			//var oldLocationId = this.LocationId;
			//var oldLocationSnapshot = this.Location.ToSnopShot();
			this.AddDomainEvent(new PalletHistoryNotification(this.Id, PalletNumber,
				oldLocationId, oldLocationSnapShot, newLocationId, newLocationSnapShot, ReasonMovement.Moved, userId, this.Status, BuildMovementDetails()));
			this.LocationId = newLocationId;
		}

		public void CloseAndAddPickingPallet(Guid issueId, string userId, string snapShot)
		{
			if (Status != PalletStatus.Picking)
				throw new InvalidPalletStatusException(Id);
			Status = PalletStatus.ToIssue;
			IssueId = issueId;
			AddHistory(ReasonMovement.ToLoad, userId, snapShot);
		}

		public void AddHistory(ReasonMovement reason, string userId, string snapShot)
		{
			this.AddDomainEvent(new PalletHistoryNotification(this.Id, PalletNumber,
				LocationId, snapShot, LocationId, snapShot, reason, userId, this.Status, BuildMovementDetails()));
		}
		public bool ContainsProduct(Guid productId)
		{
			return ProductsOnPallet.Any(p => p.ProductId == productId);
			//założenie że na palecie tylko jedna data danego produktu bo inaczej + BB
		}

		public int GetProductQuantity(Guid productId)
		{
			return ProductsOnPallet
				.Where(p => p.ProductId == productId)
				.Sum(p => p.Quantity);
		}

		public bool CanBeCancelled()
		{
			if (PalletMovements.Count > 1)
				return false;
			return true;
		}
		public void CkeckIfToArchive(string userId, ReasonMovement reason, string snapShot)
		{
			if (ProductsOnPallet.All(p => p.Quantity == 0))
			{
				this.ToArchive(userId, reason, snapShot);
			}
			else
			{
				this.ChangeStatus(PalletStatus.ReversePicking);
			}
		}
		public ProductOnPallet GetProductAggregate(Guid productId)
		{
			var product = this.ProductsOnPallet.Where(p => p.ProductId == productId);
			if (product.Count() > 1)
			{
				throw new MultipleProductsOnPalletException(Id, PalletNumber, productId);
			}
			if (product == null) throw new ProductNotFoundOnPalletException(Id, PalletNumber, productId);

			return product.First();
		}
		//public void ChangeToAvailable(string userId, string snapShot)
		//{
		//	var pickingTasks = this.VirtualPallet.PickingTasks;
		//	if (!(pickingTasks.Any(t => t.PickingStatus == PickingStatus.Allocated)))
		//	{
		//		VirtualPallet.Pallet.ChangeStatus(PalletStatus.Available);
		//		VirtualPallet.Pallet.AddHistory(Histories.Models.ReasonMovement.ReversePicking, userId, snapShot);
		//	}
		//}
		public void ChangeStatus(PalletStatus status)
		{
			//invarianty!!
			if (Status == PalletStatus.Archived) throw new InvalidPalletStatusException(Id);
			this.Status = status;
		}
		//metody pomocnicze
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