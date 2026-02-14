using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Domain.Receviving.Models
{
	public class Receipt : AggregateRoots
	{
		public int Id { get; set; }
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
		}
		public List<Pallet> VerifiedReceipt()
		{			
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.Status = PalletStatus.InStock;
			}
			ReceiptStatus = ReceiptStatus.Verified;
			return toReturn;
		}
		public void StartReceiving(DateTime now, string userId)
		{
			if (ReceiptStatus == ReceiptStatus.InProgress) return;
			if (ReceiptStatus != ReceiptStatus.Planned && ReceiptStatus != ReceiptStatus.InProgress)
				throw new DomainReceiptException("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			ReceiptStatus = ReceiptStatus.InProgress;
			ReceiptDateTime = now;
			this.AddDomainEvent(new ChangeStatusReceiptNotification(Id, ReceiptStatus, userId));
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
			this.AddDomainEvent(new ChangeStatusReceiptNotification(Id, ReceiptStatus, userId));
		}
		private IReadOnlyCollection<HistoryReceiptDetailDto> BuildListPalletsForReceipt()
		{
			return Pallets
				.Select(p=> new HistoryReceiptDetailDto(
					p.Id,
					p.LocationId,
					p.Location))
				.ToList();
		}
	}
}
