using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Clients.ClientsExceptions;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Receiving.Models
{
	public class Receipt : AggregateRoots
	{
		public Guid Id { get; private set; }
		public int ReceiptNumber { get; private set; }
		public int ClientId { get; private set; }
		public Client Client { get; private set; } = null!;
		public DateTime ReceiptDateTime { get; private set; }
		public virtual ICollection<Pallet> Pallets { get; private set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; private set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; private set; } = string.Empty;
		public ReceiptStatus ReceiptStatus { get; private set; }
		public int RampNumber { get; private set; }

		private Receipt() { }


		private Receipt(int receiptNumber, int clientId, string performedBy, int rampNumber, DateTime createdAt)
		{
			Id = Guid.NewGuid();
			ReceiptNumber = receiptNumber;
			if (clientId <= 0) throw new ClientDomainException();
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdDomainException();
			ClientId = clientId;
			PerformedBy = performedBy ?? throw new InvalidUserIdDomainException();
			ReceiptDateTime = createdAt;
			ReceiptStatus = ReceiptStatus.Planned;
			RampNumber = rampNumber;
			Pallets = new List<Pallet>();
		}

		public static Receipt Create(int receiptNumber, int clientId, string performedBy, int rampNumber, DateTime createdAt)
			=> new Receipt(receiptNumber, clientId, performedBy, rampNumber, createdAt);

		//Tests
		private Receipt(Guid id, int receiptNumber, int clientId,
			string performedBy, DateTime dateTime, ReceiptStatus receiptStatus, int rampNumber)
		{
			Id = id;
			ReceiptNumber = receiptNumber;
			if (clientId <= 0) throw new ClientDomainException();
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdDomainException();
			ClientId = clientId;
			PerformedBy = performedBy;
			ReceiptDateTime = dateTime;
			ReceiptStatus = receiptStatus;
			RampNumber = rampNumber;
			Pallets = new List<Pallet>();
		}

		public static Receipt CreateForSeed(Guid id, int receiptNumber, int clientId,
			string performedBy, DateTime dateTime, ReceiptStatus receiptStatus, int rampNumber)
			=> new Receipt(id, receiptNumber, clientId, performedBy, dateTime, receiptStatus, rampNumber);

		public void Create(string userId)
		{
			ReceiptStatus = ReceiptStatus.Planned;
			AddHistory(userId);
		}

		public void Delete(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.Planned)
			{
				throw new InvalidReceiptStateDomainException(Id, ReceiptNumber, ReceiptStatus);
			}
			ReceiptStatus = ReceiptStatus.Deleted;
			AddHistory(userId);
		}

		public void Cancel(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new ReceiptAlreadyVerifyDomainException(Id, ReceiptNumber);
			}
			if (!(ReceiptStatus == ReceiptStatus.InProgress
			|| ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
			{
				throw new InvalidReceiptStateDomainException(Id, ReceiptNumber, ReceiptStatus);
			}
			foreach (var pallet in Pallets)
			{
				if (!pallet.CanBeCancelled())
				{
					throw new CannotCancelReceiptDomainException(Id, ReceiptNumber);
				}
			}
			foreach (var pallet in Pallets)
			{
				pallet.DetachFromReceipt(userId, pallet.Location.ToSnapshot());
			}
			ReceiptStatus = ReceiptStatus.Cancelled;
			AddHistory(userId);
		}

		public void StartReceiving(DateTime now, string userId)
		{
			if (ReceiptStatus == ReceiptStatus.InProgress) return;
			if (ReceiptStatus != ReceiptStatus.Planned && ReceiptStatus != ReceiptStatus.InProgress)
				throw new InvalidReceiptStateDomainException(Id, ReceiptNumber, ReceiptStatus);
			ReceiptStatus = ReceiptStatus.InProgress;
			ReceiptDateTime = now;
			AddHistory(userId);
		}

		public void UpdateReceipt(string userId, int clientId, DateTime updatedAt)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
				throw new ReceiptAlreadyVerifyDomainException(Id, ReceiptNumber);


			PerformedBy = userId;
			ReceiptStatus = ReceiptStatus.Correction;
			ClientId = clientId;
			ReceiptDateTime = updatedAt;
			AddHistory(userId);
		}

		public void CompletePhysicalReceipt(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.InProgress)
				throw new InvalidReceiptStateDomainException(Id, ReceiptNumber, ReceiptStatus);
			ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
			AddHistory(userId);
		}

		public void VerifiedReceipt(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new ReceiptAlreadyVerifyDomainException(Id, ReceiptNumber);
			}
			if (ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				throw new InvalidReceiptStateDomainException(Id, ReceiptNumber, ReceiptStatus);
			}
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.ChangeStatus(PalletStatus.InStock);
				pallet.AddHistory(ReasonForPallet.Received, userId, pallet.Location.ToSnapshot());
			}
			ReceiptStatus = ReceiptStatus.Verified;
			AddHistory(userId);
			AddDomainEvent(new ChangeStockNotification(CreateStockItem(toReturn)));
		}

		//Detach i Attach tylko dla update - dla historii
		public void AttachPallet(Pallet pallet)
		{
			if (!Pallets.Contains(pallet))
				Pallets.Add(pallet);
		}

		public void DetachPallet(Pallet pallet)
		{
			Pallets.Remove(pallet);
		}

		public void AddHistory(string userId)
		{
			AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}

		//metody pomocnicze
		private IReadOnlyCollection<HistoryReceiptIssueDetailDto> BuildListPalletsForReceipt()
		{
			return Pallets
				.Select(p => new HistoryReceiptIssueDetailDto(
					p.Id,
					p.PalletNumber,
					p.LocationId,
					p.Location.ToSnapshot()))
				.ToList();
		}
		private IEnumerable<StockItemChange> CreateStockItem(List<Pallet> pallets)
		{
			return pallets
				.SelectMany(p => p.ProductsOnPallet)
				.GroupBy(p => p.ProductId)
				.Select(g => new StockItemChange(
					g.Key,
					g.Sum(q => q.Quantity)));
		}
		public List<ReceiptPalletUpdate> StartUpdateReceipt(List<ReceiptPalletUpdate> pallets, string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
				throw new ReceiptAlreadyVerifyDomainException(Id, ReceiptNumber);

			foreach (var item in pallets)
			{
				if (item.PalletId != Guid.Empty && Pallets.All(p=>p.Id != item.PalletId))
				{
					throw new PalletDoesNotBelongToReceiptDomainException(Id, ReceiptNumber, item.PalletId, item.PalletNumber!);//istniejąca paleta nie może nie mieć palletNumber
				}
			}
			//List palet do usunięcia z bazy danych 
			var incomingPalletsIds = pallets
				.Select(p => p.PalletId)
				.Where(id => id != Guid.Empty)
				.ToHashSet();
			var palletToDelete = Pallets
				.Where(p => !incomingPalletsIds.Contains(p.Id))
				.ToList();
			//Usuwanie z bazy danych niepotrzebnych pallet
			foreach (var pallet in palletToDelete)
			{
				DetachPallet(pallet);//musi być żeby stworzyć dobrą historię					
				pallet.DetachFromReceipt(userId, pallet.Location.ToSnapshot());
			}
			var existingPallets = Pallets.ToDictionary(p => p.Id);
			//Aktualizacja palet
			foreach (var palletOld in pallets.Where(p => p.PalletId != Guid.Empty))
			{
				if (!existingPallets.TryGetValue(palletOld.PalletId!, out var pallet))
					continue;

				var productsForPallet = new List<ProductOnPallet>();

				var productForPallet = ProductOnPallet.Create(palletOld.ProductId,
					palletOld.PalletId, palletOld.Quantity, palletOld.DateAdded, palletOld.BestBefore);

				productsForPallet.Add(productForPallet);

				pallet.ReplaceProducts(productsForPallet);
				pallet.ChangeStatus(PalletStatus.Receiving);
				pallet.AddHistory(ReasonForPallet.Correction, userId, pallet.Location.ToSnapshot());
			}
			//Dodanie nowych palet - Adding new palets
			var palletsAdded = pallets
				.Where(p => p.PalletId == Guid.Empty)
				.ToList();

			return palletsAdded;
		}
	}
}
