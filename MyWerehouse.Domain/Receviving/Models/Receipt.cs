using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Events;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Events;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Receviving.Models
{
	public class Receipt : AggregateRoots
	{
		public Guid Id { get; private set; }// = Guid.NewGuid();  //gdyby nie testy to private set
		public int ReceiptNumber { get; private set; }
		public int ClientId { get; private set; }
		public virtual Client Client { get; private set; }
		public DateTime ReceiptDateTime { get; private set; }
		public virtual ICollection<Pallet> Pallets { get; private set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; private set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; private set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; private set; }
		public int RampNumber { get; private set; }
		//public Receipt() { }
		private Receipt() { }
		public Receipt(int clientId, string performedBy, int rampNumber)
		{
			//if (clientId <= 0) throw new InvalidClientNumberException(clientId);
			if (clientId <= 0) throw new ArgumentException("ClientId musi być dodatni");

			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			//if (rampNumber <= 0 || rampNumber > 100) throw new NotFoundRampException(rampNumber);//dostępne rampy
			ClientId = clientId;
			PerformedBy = performedBy;
			ReceiptDateTime = DateTime.UtcNow;
			ReceiptStatus = ReceiptStatus.Planned;
			RampNumber = rampNumber;
			Pallets = new List<Pallet>();
		}

		private Receipt(int receiptNumber, int clientId, string performedBy, int rampNumber)
		{
			Id = Guid.NewGuid();
			ReceiptNumber = receiptNumber;
			//if (clientId <= 0) throw new InvalidClientNumberException(clientId);
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			//if (rampNumber <= 0 || rampNumber > 100) throw new NotFoundRampException(rampNumber);//dostępne rampy
			ClientId = clientId;
			PerformedBy = performedBy;
			ReceiptDateTime = DateTime.UtcNow;
			ReceiptStatus = ReceiptStatus.Planned;
			RampNumber = rampNumber;
			Pallets = new List<Pallet>();
		}

		public static Receipt Create(int receiptNumber, int clientId, string performedBy, int rampNumber)
			=> new Receipt(receiptNumber, clientId, performedBy, rampNumber);

		//Tests
		private Receipt(Guid id, int receiptNumber, int clientId,
			string performedBy, DateTime dateTime, ReceiptStatus receiptStatus, int rampNumber)
		{
			Id = id;
			ReceiptNumber = receiptNumber;
			//if (clientId <= 0) throw new InvalidClientNumberException(clientId);
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			//if (rampNumber <= 0 || rampNumber > 100) throw new NotFoundRampException(rampNumber);//dostępne rampy
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

		public void VerifiedReceipt(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				throw new DomainReceiptException("Nie można zweryfikować przyjęcia");
			}
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.ChangeStatus(PalletStatus.InStock);
				//pallet.Status = PalletStatus.InStock;
				pallet.AddHistory(PalletStatus.InStock, ReasonMovement.Received, userId);
			}
			ReceiptStatus = ReceiptStatus.Verified;

			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(toReturn)));
			//return toReturn;
		}
		public void AddHistory(string userId)
		{
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}
		public void ChangeStatus(ReceiptStatus receiptStatus, string userId)
		{
			ReceiptStatus = receiptStatus;
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}
		public bool Delete(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Planned)
			{
				ReceiptStatus = ReceiptStatus.Deleted;
				this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
				return true;
			}
			return false;
		}
		public void Cancel(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new DomainReceiptException("Nie można usunąć zweryfikowanego przyjęcia", ReceiptNumber);
			}
			if (!(ReceiptStatus == ReceiptStatus.InProgress
			|| ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
			{
				throw new DomainReceiptException("Nieprawidłowy status przyjęcia");
			}
			ReceiptStatus = ReceiptStatus.Cancelled;
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}
		public void StartReceiving(DateTime now, string userId)
		{
			if (ReceiptStatus == ReceiptStatus.InProgress) return;
			if (ReceiptStatus != ReceiptStatus.Planned && ReceiptStatus != ReceiptStatus.InProgress)
				throw new DomainReceiptException("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			ReceiptStatus = ReceiptStatus.InProgress;
			ReceiptDateTime = now;
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}
		public void AttachPallet(Pallet pallet, Location location, string userId)
		{
			if (pallet.Status == PalletStatus.Receiving)
				throw new DomainException("Already assigned");
			Pallets.Add(pallet);
			pallet.AssignToReceipt(Id, location, userId);
		}
		public void DetachPallet(Pallet pallet)
		{
			Pallets.Remove(pallet);
		}
		public void UpdateReceipt(string userId, int clientId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
				throw new DomainReceiptException("Zatwierdzone przyjęcie nie może być modyfikowane.");
			PerformedBy = userId;
			ReceiptStatus = ReceiptStatus.Correction;
			ClientId = clientId;
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}
		public void CompletePhysicalReceipt(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.InProgress)
				throw new DomainReceiptException("Nie można zakończyć przyjęcia - błędny status zlecenia");
			ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}

		private IReadOnlyCollection<HistoryReceiptIssueDetailDto> BuildListPalletsForReceipt()
		{
			return Pallets
				.Select(p => new HistoryReceiptIssueDetailDto(
					p.Id,
					p.PalletNumber,
					p.LocationId,
					p.Location.ToSnopShot()))
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
	}
}
