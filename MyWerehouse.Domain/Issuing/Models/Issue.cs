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
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Domain.Issuing.Models
{
	public class Issue : AggregateRoots
	{
		//public Guid Id { get; set; } = Guid.NewGuid();
		public Guid Id { get; private set; } = Guid.NewGuid();
		public int IssueNumber { get; private set; }
		public int ClientId { get; private set; }
		public Client Client { get; private set; }
		public DateTime IssueDateTimeCreate { get; private set; }
		public DateTime IssueDateTimeSend { get; private set; }
		public ICollection<Pallet> Pallets { get; private set; } = new List<Pallet>();
		public ICollection<HistoryIssue> HistoryIssues { get; private set; } = new List<HistoryIssue>();
		public ICollection<HistoryPicking> HistoryPickings { get; private set; } = new List<HistoryPicking>();
		public ICollection<PickingTask> PickingTasks { get; private set; } = new List<PickingTask>();
		public string PerformedBy { get; private set; }
		public IssueStatus IssueStatus { get; private set; }
		public ICollection<IssueItem> IssueItems { get; private set; } = new List<IssueItem>();//nowe powiązanie		
																							   //public Issue() { }
		private Issue() { }
		//public Issue(int clientId, string performedBy, DateTime dateToSend)
		//{
		//	ClientId = clientId;
		//	if (clientId <= 0) throw new ArgumentException("ClientId musi być dodatni");
		//	PerformedBy = performedBy ?? throw new ArgumentNullException(nameof(performedBy));
		//	IssueDateTimeSend = dateToSend;
		//	if (dateToSend < DateTime.UtcNow) throw new DomainException("Data wysyłki nie może być w przeszłości");
		//	IssueStatus = IssueStatus.New;
		//	IssueDateTimeCreate = DateTime.UtcNow;
		//}

		private Issue(int issueNumber, int clientId, DateTime dateToSend, string performedBy)
		{
			Id = Guid.NewGuid();
			IssueNumber = issueNumber;
			if (clientId <= 0) throw new ArgumentException("ClientId musi być dodatni");
			ClientId = clientId;
			if (dateToSend < DateTime.UtcNow) throw new DomainException("Data wysyłki nie może być w przeszłości");
			IssueDateTimeSend = dateToSend;
			IssueDateTimeCreate = DateTime.UtcNow;
			PerformedBy = performedBy ?? throw new ArgumentNullException(nameof(performedBy));
			IssueStatus = IssueStatus.New;
		}

		public static Issue Create(int issueNumber, int clientId, DateTime dateToSend, string performedBy)
			=> new Issue(issueNumber, clientId, dateToSend, performedBy);

		private Issue(Guid id, int issueNumber, int clientId, DateTime issueDateTimeCreate,
			DateTime issueDateTimeSend, string performedBy, IssueStatus issueStatus, List<IssueItem>? issueItems)
		{
			Id = id;
			IssueNumber = issueNumber;
			ClientId = clientId;
			IssueDateTimeCreate = issueDateTimeCreate;
			IssueDateTimeSend = issueDateTimeSend;
			PerformedBy = performedBy;
			IssueStatus = issueStatus;
			IssueItems = issueItems;
		}
		public static Issue CreateForSeed(Guid id, int issueNumber, int clientId, DateTime issueDateTimeCreate,
			DateTime issueDateTimeSend, string performedBy, IssueStatus issueStatus, List<IssueItem>? issueItems) =>
			new Issue(id, issueNumber, clientId, issueDateTimeCreate, issueDateTimeSend,
				performedBy, issueStatus, issueItems);

		public void ChangeUser(string userId)
		{
			if (userId == null) throw new ArgumentNullException("userId");
			PerformedBy = userId;
		}
		public void ChangeStatus(IssueStatus issueStatus)
		{
			if (IssueStatus == IssueStatus.Cancelled || issueStatus == IssueStatus.Archived) throw new InvalidOperationException("Operation forbidden.");
			IssueStatus = issueStatus;
		}
		public void CancelIssue(string userId)
		{
			IssueStatus = IssueStatus.Cancelled;
			AddHistory(userId);
			foreach (var pallet in Pallets)
			{
				pallet.DetachToIssue(Id, userId);
			}
			foreach (var task in PickingTasks)
			{
				task.Cancel(userId, IssueNumber);
				//task.PickingStatus = PickingStatus.Cancelled;
				//task.RequestedQuantity = 0;
				//task.AddHistory(userId, PickingStatus.Allocated, PickingStatus.Cancelled, 0);
			}
		}
		public int GetQuantityForProduct(Guid productId)
		{
			var item = IssueItems
				.FirstOrDefault(x => x.ProductId == productId);
			return item?.Quantity ?? 0;
		}
		public void AddIssueItem(Guid productId, int quantity, DateOnly bestBefore)
		{
			var existing = IssueItems.FirstOrDefault(x => x.ProductId == productId);
			if (existing != null)
			{
				//existing.IncreaseQuantity(quantity);
				//return;
				throw new InvalidOperationException("IssueItem exist.");
			}
			if (quantity <= 0) throw new InvalidDataException("Quantity must be grater than zero.");
			var item = new IssueItem(Id, productId, quantity, bestBefore);
			this.IssueItems.Add(item);
		}
		public List<Pallet> RemoveNotLoadedPallets(string userId)
		{
			var toReturn = Pallets.Where(p => p.Status != PalletStatus.Loaded).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.DetachToIssue(Id, userId);
				pallet.AddHistory(PalletStatus.Available, ReasonMovement.Correction, userId);
				Pallets.Remove(pallet);
			}
			return toReturn;
		}
		public void ReservePallet(Pallet pallet, string userId)
		{
			if (pallet.Status == PalletStatus.ToIssue)
				throw new DomainException("Already reserved");
			this.Pallets.Add(pallet);
			pallet.ReserveToIssue(this, userId);
		}


		public void DetachPallet(Pallet pallet, string userId)
		{
			this.Pallets.Remove(pallet);
			pallet.DetachToIssue(this.Id, userId);
		}

		public void AssignPallet(Pallet pallet, string userId)
		{
			this.Pallets.Add(pallet);
			pallet.AssignToIssue(this, userId);
		}
		public void AttachPickingTask(PickingTask task)
		{
			this.PickingTasks.Add(task);
		}



		public void ConfirmToLoad(string userId)
		{
			IssueStatus = IssueStatus.ConfirmedToLoad;
			foreach (var pallet in Pallets)
			{
				pallet.AssignToIssue(this, userId);
				//pallet.IssueId = Id;
				pallet.AddHistory(PalletStatus.ToIssue, ReasonMovement.ToLoad, userId);
			}
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));

		}
		public void VeryfiedAfterLoading(string userId)
		{
			if (Pallets.Any(p => p.Status != PalletStatus.Loaded))
			{
				throw new DomainIssueException("Nie wszystkie palety mają status Loaded.");
			}
			PerformedBy = userId;
			if (IssueStatus != IssueStatus.IsShipped)
			{
				throw new DomainIssueException("Nie zakończono załadunku.");
			}
			foreach (var pallet in Pallets)
			{
				pallet.AddHistory(PalletStatus.Archived, ReasonMovement.Loaded, userId);
			}
			IssueStatus = IssueStatus.Archived;
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(Pallets.ToList())));
		}
		public void FinishIssueNotCompleted(string userId)
		{
			PerformedBy = userId;
			foreach (var pallet in Pallets)
			{
				pallet.AddHistory(pallet.Status, ReasonMovement.Loaded, userId);
			}
			IssueStatus = IssueStatus.IsShipped;
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));
		}
		public void Cancel(string userId)
		{
			IssueStatus = IssueStatus.Cancelled;
			PerformedBy = userId;
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));
		}
		public void ChangePalletInIssue(string userId)
		{
			IssueStatus = IssueStatus.ChangingPallet;
			PerformedBy = userId;
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));

		}
		public void CompletedLoad(string userId)
		{
			foreach (var pallet in Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new DomainIssueException("Nie załadowano wszystkich palet.");
				}
			}
			IssueStatus = IssueStatus.IsShipped;
			PerformedBy = userId;
			this.AddDomainEvent(new AddHistoryForIssueNotification(
				Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));
		}
		//*
		public void AddHistory(string userId)
		{
			this.AddDomainEvent(new AddHistoryForIssueNotification(
			Id, IssueNumber, ClientId, IssueStatus, userId, BuildListPalletsForIssue(), BuildListItems()));
		}

		private IReadOnlyCollection<HistoryReceiptIssueDetailDto> BuildListPalletsForIssue()
		{
			return Pallets
				.Select(p => new HistoryReceiptIssueDetailDto(
					p.Id,
					p.PalletNumber,
					p.LocationId,
					p.Location.ToSnopShot()))
				.ToList();
		}
		private IReadOnlyCollection<AddListItemsOfIssueDetailsDto> BuildListItems()
		{
			if (IssueItems is null) throw new ArgumentNullException("No data for items.");
			return IssueItems
				.Select(i => new AddListItemsOfIssueDetailsDto(
					i.Id,
					i.ProductId,
					i.Quantity,
					i.BestBefore))
				.ToList();
		}
		private IEnumerable<StockItemChange> CreateStockItem(List<Pallet> pallets)
		{
			return pallets
				.SelectMany(p => p.ProductsOnPallet)
				.GroupBy(p => p.ProductId)
				.Select(g => new StockItemChange(
					g.Key,
					-g.Sum(q => q.Quantity)));
		}

	}
}
//// Fabryka dla Update (jeśli zmieniasz stan)
//public void UpdateStatus(IssueStatus newStatus)
//{
//	if (newStatus == IssueStatus.ConfirmedToLoad && IssueDateTimeSend == default)
//		throw new DomainException("ConfirmedToLoad wymaga daty wysyłki");
//	IssueStatus = newStatus;
//}