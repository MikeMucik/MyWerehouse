using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public Guid Id { get; set; } = Guid.NewGuid();
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryIssue> HistoryIssues { get; set; } = new List<HistoryIssue>();
		public virtual ICollection<HistoryPicking> HistoryPickings { get; set; } = new List<HistoryPicking>();
		public virtual ICollection<PickingTask> PickingTasks { get; set; } = new List<PickingTask>();
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public virtual ICollection<IssueItem> IssueItems { get; set; } = new List<IssueItem>();//nowe powiązanie		
		public Issue() { }
		public Issue(int clientId, string performedBy, DateTime dateToSend)
		{
			ClientId = clientId;
			if (clientId <= 0) throw new ArgumentException("ClientId musi być dodatni");
			PerformedBy = performedBy ?? throw new ArgumentNullException(nameof(performedBy));
			IssueDateTimeSend = dateToSend;
			if (dateToSend < DateTime.UtcNow) throw new DomainException("Data wysyłki nie może być w przeszłości");
			IssueStatus = IssueStatus.New;
			IssueDateTimeCreate = DateTime.UtcNow;
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
		//public void AssignPallet(Pallet pallet, string userId)
		//{
		//	this.Pallets.Add(pallet);
		//	pallet.AssignToIssue(this, userId);
		//}
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
			foreach(var pallet in Pallets)
			{
				if(pallet.Status != PalletStatus.Loaded)
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

		public void CreateIssue(string userId, bool Success)
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
			return IssueItems
				.Select(i => new AddListItemsOfIssueDetailsDto(
					//i.Id,
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