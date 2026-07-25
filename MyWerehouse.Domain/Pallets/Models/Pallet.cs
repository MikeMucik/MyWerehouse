using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet : AggregateRoots
	{
		public Guid Id { get; private set; }
		public string PalletNumber { get; private set; } = string.Empty;
		public DateTime DateReceived { get; private set; }
		// Snapshot przechowywany jako string – uproszczenie pod potrzeby projektu/portfolio.
		// W systemie produkcyjnym byłby to Value Object (np. LocationSnapshot).		
		public int LocationId { get; private set; }
		public Location Location { get; private set; } = null!;
		public PalletStatus Status { get; private set; } = 0;
		public ICollection<ProductOnPallet> ProductsOnPallet { get; private set; } = new List<ProductOnPallet>();
		public ICollection<HistoryPallet> PalletHistory { get; private set; } = new List<HistoryPallet>();
		public Guid? ReceiptId { get; private set; }
		public Receipt? Receipt { get; private set; }
		public Guid? IssueId { get; private set; }
		public Issue? Issue { get; private set; }
		[Timestamp]
		public byte[] RowVersion { get; set; } = []; //działa tylko w M-SQL wymaga DbUpdateConcurrencyException											   

		private Pallet() { }

		private Pallet(string palletNumber, int locationId, DateTime dateReceived)
		{
			Id = Guid.NewGuid();
			PalletNumber = palletNumber;
			LocationId = locationId;
			DateReceived = dateReceived;
		}

		public static Pallet Create(string palletNumber, int locationId, DateTime receivedAt)
			=> new Pallet(palletNumber, locationId, receivedAt);

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

		public void CreateNewPalletFromReservePicking(string snapShot, string userId)
		{
			Status = PalletStatus.InStock;
			AddHistory(ReasonForPallet.ReversePicking, userId, snapShot);
		}

		public void AssignToWarehouse(int locationId, string snapShot, string userId)
		{
			var listProducts = this.CreateStockItem();
			Status = PalletStatus.InStock;
			AddHistory(ReasonForPallet.New, userId, snapShot);
			this.AddDomainEvent(new ChangeStockNotification(listProducts));
		}

		public void Update(string userId, List<ProductOnPallet> products, PalletStatus palletStatus, string snapShot)
		{
			var changeQuangtityInventory = this.CalculateQuantityDelta(products);
			Status = palletStatus;
			this.ReplaceProducts(products);
			this.AddDomainEvent(new ChangeStockNotification(changeQuangtityInventory));
			AddHistory(ReasonForPallet.Correction, userId, snapShot);
		}

		public void ReplaceProducts(List<ProductOnPallet> updatedProducts)
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
					ProductsOnPallet.Add(pop);
				}
				else
				{
					existing.SetQuantity(pop.Quantity);
					existing.SetBestBefore(pop.BestBefore);
				}
			}
		}

		public void AddProduct(Guid productId, int quantity, DateTime createdAt, DateOnly? bestBefore)
		{
			if (quantity <= 0)
				throw new InvalidQuantityDomainException(Id);
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, createdAt, bestBefore));
		}

		public void AddOrIncreaseProductQuantity(Guid productId, int quantity, DateTime createdAt, DateOnly? bestBefore)
		{
			var existingProduct = ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
			if (existingProduct != null)
			{
				if (existingProduct.BestBefore != bestBefore)
					throw new TwoDateOneProductOnPalletDomainException(Id);
				existingProduct.IncreaseQuantity(quantity);
				return;
			}
			AddProduct(productId, quantity, createdAt, bestBefore);
		}

		public void AddProductForTests(Guid productId, int quantity, DateTime dateAdd, DateOnly? bestBefore)
		{
			if (quantity <= 0)
				throw new InvalidQuantityDomainException(Id);
			this.ProductsOnPallet.Add(ProductOnPallet.Create(productId, Id, quantity, dateAdd, bestBefore));
		}

		//zmiana sposób zapisywania historii dla rezerwacji bo nowa paleta
		public void ReserveToIssue(Guid issueId, string userId, string snapShot)
		{
			if (Status == PalletStatus.ToIssue)
				throw new AlreadyAssignedDomainException(Id);

			if (Status == PalletStatus.Available || Status == PalletStatus.InStock)
			{
				Status = PalletStatus.LockedForIssue;
			}
			//żeby można było dalej kompletować na tą samą paletę, status lockedForIssue dla modify
			else if (Status == PalletStatus.Picking || Status == PalletStatus.LockedForIssue)
			{
				// OK – zostaje
			}
			else
			{
				throw new InvalidPalletStatusDomainException(Id, PalletNumber);
			}
			IssueId = issueId;
			AddHistory(ReasonForPallet.ToLoad, userId, snapShot);
		}

		public void AssignToIssue(Guid issueId, string userId, string snapShot)
		{
			if (Status != PalletStatus.ToIssue && Status != PalletStatus.LockedForIssue)
			{
				throw new InvalidPalletStatusDomainException(Id, PalletNumber);
			}
			if (Status == PalletStatus.LockedForIssue)
			{
				Status = PalletStatus.ToIssue;
			}
			IssueId = issueId;
			AddHistory(ReasonForPallet.ToLoad, userId, snapShot);
		}

		public void DetachFromReceipt(string userId, string snapShot)
		{
			Status = PalletStatus.Cancelled;
			ReceiptId = null;
			AddHistory(ReasonForPallet.ToLoad, userId, snapShot);
		}

		public void DetachFromIssue(string userId, string snapShot, ReasonForPallet reason)
		{
			IssueId = null;
			Status = PalletStatus.Available;
			AddHistory(reason, userId, snapShot);
		}

		public void AssignToPicking(string userId, string snapShot)
		{
			Status = PalletStatus.ToPicking;
			AddHistory(ReasonForPallet.Picking, userId, snapShot);
		}

		public void AssignToReceipt(Guid receiptId, string snapshot, string userId)
		{
			if (Status == PalletStatus.Receiving) throw new AlreadyAssignedDomainException(Id);
			ReceiptId = receiptId;
			Status = PalletStatus.Receiving;
			AddHistory(ReasonForPallet.Received, userId, snapshot);
		}

		public void ToArchive(string userId, ReasonForPallet reason, string snapShot)
		{
			if (Status == PalletStatus.Archived) throw new InvalidPalletStatusDomainException(Id, PalletNumber);
			Status = PalletStatus.Archived;
			AddHistory(reason, userId, snapShot);
		}

		public void MoveToLocation(int newLocationId, string newLocationSnapShot, int oldLocationId, string oldLocationSnapShot, string userId)
		{
			if (this.Status == PalletStatus.InStock)
			{
				Status = PalletStatus.Available;
			}

			this.AddDomainEvent(new PalletHistoryNotification(this.Id, PalletNumber,
				oldLocationId, oldLocationSnapShot, newLocationId, newLocationSnapShot, ReasonForPallet.Moved, userId, this.Status, BuildMovementDetails()));
			this.LocationId = newLocationId;
		}

		public void CloseAndAddPickingPallet(Guid issueId, string userId, string snapShot)
		{
			if (Status == PalletStatus.ToIssue)
			{
				throw new AlreadyAssignedDomainException(Id);
			}
			if (Status != PalletStatus.Picking)
				throw new InvalidPalletStatusDomainException(Id, PalletNumber);
			Status = PalletStatus.ToIssue;
			IssueId = issueId;
			AddHistory(ReasonForPallet.ToLoad, userId, snapShot);
		}

		public void AddHistory(ReasonForPallet reason, string userId, string snapShot)
		{
			this.AddDomainEvent(new PalletHistoryNotification(this.Id, PalletNumber,
				LocationId, snapShot, LocationId, snapShot, reason, userId, this.Status, BuildMovementDetails()));
		}
		public bool ContainsProduct(Guid productId)
		{
			return ProductsOnPallet.Any(p => p.ProductId == productId);
			//założenie że na palecie tylko jedna data danego produktu 
		}

		public int GetProductQuantity(Guid productId)
		{
			return ProductsOnPallet
				.Where(p => p.ProductId == productId)
				.Sum(p => p.Quantity);
		}

		public bool CanBeCancelled()
		{
			if (PalletHistory.Count > 1)
				return false;
			return true;
		}
		public void CkeckIfToArchive(string userId, ReasonForPallet reason, string snapShot)
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
		public ProductOnPallet GetProductOnPallet(Guid productId)
		{
			var product = this.ProductsOnPallet.Where(p => p.ProductId == productId);
			if (product.Count() > 1)
			{
				throw new MultipleProductsOnPalletDomainException(Id, PalletNumber, productId);
			}
			if (!product.Any()) throw new ProductNotFoundOnPalletDomainException(Id, PalletNumber, productId);

			return product.First();
		}

		public void MarkAsLoaded(string userId, string snapShot)
		{
			if (Status == PalletStatus.Loaded)
			{
				throw new PalletAlreadyLoadedDomainException(Id, PalletNumber);
			}
			if (Status != PalletStatus.ToIssue
				&& Status != PalletStatus.LockedForIssue)
			{
				throw new InvalidPalletStatusDomainException(Id, PalletNumber);
			}
			this.Status = PalletStatus.Loaded;
			this.AddHistory(ReasonForPallet.Loaded, userId, snapShot);
		}

		public void ChangeStatus(PalletStatus status)
		{
			if (Status == PalletStatus.Archived) throw new InvalidPalletStatusDomainException(Id, PalletNumber);
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

		private IReadOnlyCollection<HistoryPalletDetail> BuildMovementDetails()
		{
			return ProductsOnPallet
				.Select(p => new HistoryPalletDetail
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


		//Nowe metody - logika  application -> domain
		public static Pallet CreatePickingPallet(
			string palletNumber,
			int locationId,
			DateTime createdAt,
			Guid firstProduct,
			int fisrtQuantity,
			DateOnly? bestBefore)
		{
			var pallet = Pallet.Create(palletNumber, locationId, createdAt);
			pallet.ChangeStatus(PalletStatus.Picking);
			pallet.AddProduct(firstProduct, fisrtQuantity, createdAt, bestBefore);
			return pallet;
		}
		public void PickProduct(ProductOnPallet product, int quantity, string userId, string snapshot)
		{
			product.DecreaseQuantity(quantity);
			if (product.Quantity == 0)
			{
				this.ChangeStatus(PalletStatus.Archived);
			}
			this.AddHistory(ReasonForPallet.Picking, userId, snapshot);
		}
	}
}
