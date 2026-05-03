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
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receviving.Events;

namespace MyWerehouse.Domain.Issuing.Models
{
	public class Issue : AggregateRoots
	{
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
		public ICollection<IssueItem> IssueItems { get; private set; } = new List<IssueItem>();
		private Issue() { }
		private Issue(int issueNumber, int clientId, DateTime dateToSend, string performedBy)
		{
			Id = Guid.NewGuid();
			IssueNumber = issueNumber;
			if (clientId <= 0) throw new ArgumentException("ClientId musi być dodatni");
			ClientId = clientId;
			if (dateToSend < DateTime.UtcNow) throw new WrongDataException();
			IssueDateTimeSend = dateToSend;
			IssueDateTimeCreate = DateTime.UtcNow;
			PerformedBy = performedBy ?? throw new InvalidUserIdException(performedBy);
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
			if (userId == null || userId.Length == 0)
			{
				new InvalidUserIdException(userId);
			}			
			PerformedBy = userId;
		}

		public void ChangeStatus(IssueStatus issueStatus)
		{
			if (IssueStatus == IssueStatus.Cancelled || issueStatus == IssueStatus.Archived)
				throw new NotAllowedOperationException(Id);
			IssueStatus = issueStatus;
		}

		public void CancelIssue(string userId)
		{
			if (IssueStatus == IssueStatus.Cancelled || IssueStatus == IssueStatus.Archived)
				throw new NotAllowedOperationException(Id);
			IssueStatus = IssueStatus.Cancelled;
			AddHistory(userId);
			foreach (var pallet in Pallets)
			{
				pallet.DetachToIssue(userId, pallet.Location.ToSnapshot(), ReasonMovement.CancelIssue);
			}
			foreach (var task in PickingTasks)
			{
				task.Cancel(userId, IssueNumber);
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
				//TODO ?? jeśli zdecydujemy na zmianę Item a nie jak teraz nowa wartość
				//existing.IncreaseQuantity(quantity);
				//return;
				throw new ProductAlreadyExistException(productId);
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
				pallet.DetachToIssue(userId, pallet.Location.ToSnapshot(), ReasonMovement.Correction);
				Pallets.Remove(pallet);
			}
			return toReturn;
		}		
				
		public void ConfirmToLoad(string userId)
		{
			//invarianty status
			IssueStatus = IssueStatus.ConfirmedToLoad;
			foreach (var pallet in Pallets)
			{				
				pallet.AssignToIssue(Id, userId, pallet.Location.ToSnapshot());				
			}
			AddHistory(userId);
		}

		public void VeryfiedAfterLoading(string userId)
		{
			if (Pallets.Any(p => p.Status != PalletStatus.Loaded))
			{
				throw new NotEndedLoadingException(Id); //może dodać jakie nie są ale to nie w domenie				
			}
			PerformedBy = userId;
			if (IssueStatus != IssueStatus.IsShipped)
			{
				throw new NotAllowedOperationException(Id);
			}
			foreach (var pallet in Pallets)
			{
				pallet.ToArchive(userId, ReasonMovement.Loaded, pallet.Location.ToSnapshot());
			}
			IssueStatus = IssueStatus.Archived;
			AddHistory(userId);
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(Pallets.ToList())));
		}

		public void FinishIssueNotCompleted(string userId)
		{
			//invarianty dla status
			PerformedBy = userId;
			foreach (var pallet in Pallets)
			{
				pallet.AddHistory(ReasonMovement.Loaded, userId, pallet.Location.ToSnapshot());
			}
			IssueStatus = IssueStatus.IsShipped;
			AddHistory(userId);
		}

		public void Cancel(string userId)
		{
			//invarianty dla status
			IssueStatus = IssueStatus.Cancelled;
			PerformedBy = userId;
			AddHistory(userId);
		}

		public void ChangePalletInIssue(string userId)
		{
			//invarianty dla status
			IssueStatus = IssueStatus.ChangingPallet;
			PerformedBy = userId;
			AddHistory(userId);
		}

		public void CompletedLoad(string userId)
		{
			foreach (var pallet in Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new NotEndedLoadingException(Id);
				}
			}
			IssueStatus = IssueStatus.IsShipped;
			PerformedBy = userId;
			AddHistory(userId);			
		}

		public void RemovePickingTask(PickingTask pickingTask)
		{
			PickingTasks.Remove(pickingTask);
		}
		//Detach i Attach tylko dla update - dla historii
		public void DetachPallet(Pallet pallet)
		{
			this.Pallets.Remove(pallet);
		}

		public void AttachPallet(Pallet pallet)
		{
			if (!Pallets.Contains(pallet))
				this.Pallets.Add(pallet);
		}
		//*
		public void AttachPickingTask(PickingTask task) //do testów
		{
			this.PickingTasks.Add(task);
		}

		public void ReservePallet(Pallet pallet) //do testów
		{
			if (pallet.Status == PalletStatus.ToIssue)
				throw new AlreadyAssignedException(pallet.Id);
			this.Pallets.Add(pallet);
		}
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
					p.Location.ToSnapshot()))
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