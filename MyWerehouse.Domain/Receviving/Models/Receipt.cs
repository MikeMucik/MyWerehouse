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

namespace MyWerehouse.Domain.Receviving.Models
{
	public class Receipt : AggregateRoots
	{
		public Guid Id { get; set; } = Guid.NewGuid();  //gdyby nie testy to private set
		public int ReceiptNumber { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }
		public Receipt() { }
		public Receipt(int clientId, string performedBy, int rampNumber)
		{
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
		public void VerifiedReceipt(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				throw new DomainReceiptException("Nie można zweryfikować przyjęcia");
			}
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.Status = PalletStatus.InStock;
				pallet.AddHistory(PalletStatus.InStock, ReasonMovement.Received, userId);
			}
			ReceiptStatus = ReceiptStatus.Verified;

			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(toReturn)));
			//return toReturn;
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
		public void AttachPallet(Pallet pallet, string userId)
		{
			Pallets.Add(pallet);
			pallet.AssignToReceipt(Id, userId);
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
