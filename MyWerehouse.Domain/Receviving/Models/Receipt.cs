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
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Events;
using MyWerehouse.Domain.Receviving.ReceivingExceptions;

namespace MyWerehouse.Domain.Receviving.Models
{
	public class Receipt : AggregateRoots
	{
		public Guid Id { get; private set; }
		public int ReceiptNumber { get; private set; }
		public int ClientId { get; private set; }
		public virtual Client Client { get; private set; }
		public DateTime ReceiptDateTime { get; private set; }
		public virtual ICollection<Pallet> Pallets { get; private set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; private set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; private set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; private set; }
		public int RampNumber { get; private set; }

		private Receipt() { }


		private Receipt(int receiptNumber, int clientId, string performedBy, int rampNumber)
		{
			Id = Guid.NewGuid();
			ReceiptNumber = receiptNumber;
			if (clientId <= 0) throw new ArgumentException("ClientId must be more than zero.");
			//if (clientId <= 0) throw new InvalidClientNumberException(clientId);
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			//if (rampNumber <= 0 || rampNumber > 100) throw new NotFoundRampException(rampNumber);//dostępne rampy
			ClientId = clientId;
			PerformedBy = performedBy ?? throw new InvalidUserIdException(performedBy);
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
			if (clientId <= 0) throw new ArgumentException("ClientId must be more than zero.");
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

		public void Create(string userId)
		{
			ReceiptStatus = ReceiptStatus.Planned;
			AddHistory(userId);
		}

		public void Delete(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.Planned)
			{
				throw new InvalidReceiptStateException(Id, ReceiptStatus);
			}
			ReceiptStatus = ReceiptStatus.Deleted;
			AddHistory(userId);
		}

		public void Cancel(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new ReceiptAlreadyVerifyException(Id);
			}
			if (!(ReceiptStatus == ReceiptStatus.InProgress
			|| ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
			{
				throw new InvalidReceiptStateException(Id, ReceiptStatus);
			}
			ReceiptStatus = ReceiptStatus.Cancelled;
			AddHistory(userId);
		}

		public void StartReceiving(DateTime now, string userId)
		{
			if (ReceiptStatus == ReceiptStatus.InProgress) return;
			if (ReceiptStatus != ReceiptStatus.Planned && ReceiptStatus != ReceiptStatus.InProgress)
				throw new InvalidReceiptStateException(Id, ReceiptStatus);
			ReceiptStatus = ReceiptStatus.InProgress;
			ReceiptDateTime = now;
			AddHistory(userId);
		}

		public void UpdateReceipt(string userId, int clientId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
				throw new ReceiptAlreadyVerifyException(Id);
			PerformedBy = userId;
			ReceiptStatus = ReceiptStatus.Correction;
			ClientId = clientId;
			AddHistory(userId);
		}

		public void CompletePhysicalReceipt(string userId)
		{
			if (ReceiptStatus != ReceiptStatus.InProgress)
				throw new InvalidReceiptStateException(Id, ReceiptStatus);
			ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
			AddHistory(userId);
		}

		public void VerifiedReceipt(string userId)
		{
			if (ReceiptStatus == ReceiptStatus.Verified)
			{
				throw new ReceiptAlreadyVerifyException(Id);
			}
			if (ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			{
				throw new InvalidReceiptStateException(Id, ReceiptStatus);
			}
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.ChangeStatus(PalletStatus.InStock);
				pallet.AddHistory(ReasonMovement.Received, userId, pallet.Location.ToSnopShot());
			}
			ReceiptStatus = ReceiptStatus.Verified;
			AddHistory(userId);
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(toReturn)));
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
			this.AddDomainEvent(new AddHistoryReceiptNotification(Id, ReceiptNumber, ClientId, ReceiptStatus, userId, BuildListPalletsForReceipt()));
		}

		//metody pomocnicze
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
