using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Clients.ClientsExceptions;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receiving.Events;

namespace MyWerehouse.Domain.Issuing.Models
{
	public class Issue : AggregateRoots
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public int IssueNumber { get; private set; }
		public int ClientId { get; private set; }
		public Client Client { get; private set; } = null!;
		public DateTime IssueDateTimeCreate { get; private set; }
		public DateOnly IssueDateTimeSend { get; private set; }
		public ICollection<Pallet> Pallets { get; private set; } = new List<Pallet>();
		public ICollection<HistoryIssue> HistoryIssues { get; private set; } = new List<HistoryIssue>();
		public ICollection<HistoryPicking> HistoryPickings { get; private set; } = new List<HistoryPicking>();
		public ICollection<PickingTask> PickingTasks { get; private set; } = new List<PickingTask>();
		public string PerformedBy { get; private set; } = string.Empty;
		public IssueStatus IssueStatus { get; private set; }
		public ICollection<IssueItem> IssueItems { get; private set; } = new List<IssueItem>();
		private Issue() { }
		private Issue(int issueNumber, int clientId, DateOnly dateToSend, DateTime createdAt, string performedBy)
		{
			Id = Guid.NewGuid();
			IssueNumber = issueNumber;
			if (clientId <= 0) throw new ClientDomainException();
			ClientId = clientId;
			if (dateToSend < DateOnly.FromDateTime(createdAt)) throw new WrongDateDomainException();
			IssueDateTimeSend = dateToSend;
			IssueDateTimeCreate = createdAt;
			PerformedBy = performedBy ?? throw new InvalidUserIdDomainException();
			IssueStatus = IssueStatus.New;
		}

		public static Issue Create(int issueNumber, int clientId, DateOnly dateToSend, DateTime createdAt, string performedBy)
			=> new Issue(issueNumber, clientId, dateToSend, createdAt, performedBy);

		private Issue(Guid id, int issueNumber, int clientId, DateTime issueDateTimeCreate,
			DateOnly issueDateTimeSend, string performedBy, IssueStatus issueStatus, List<IssueItem>? issueItems)
		{
			Id = id;
			IssueNumber = issueNumber;
			ClientId = clientId;
			IssueDateTimeCreate = issueDateTimeCreate;
			IssueDateTimeSend = issueDateTimeSend;
			PerformedBy = performedBy;
			IssueStatus = issueStatus;
			IssueItems = issueItems ?? new List<IssueItem>();
		}
		public static Issue CreateForSeed(Guid id, int issueNumber, int clientId, DateTime issueDateTimeCreate,
			DateOnly issueDateTimeSend, string performedBy, IssueStatus issueStatus, List<IssueItem>? issueItems) =>
			new Issue(id, issueNumber, clientId, issueDateTimeCreate, issueDateTimeSend,
				performedBy, issueStatus, issueItems);

		public void ChangeStatus(IssueStatus issueStatus)
		{
			if (IssueStatus == IssueStatus.Cancelled || issueStatus == IssueStatus.Archived)
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			IssueStatus = issueStatus;
		}

		public void CancelIssue(string userId, DateTime createdAt)
		{
			if (IssueStatus == IssueStatus.Cancelled || IssueStatus == IssueStatus.Archived)
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			IssueStatus = IssueStatus.Cancelled;
			AddHistory(userId);
			foreach (var pallet in Pallets)
			{
				pallet.DetachFromIssue(userId, pallet.Location.ToSnapshot(), ReasonForPallet.CancelIssue);
			}
			foreach (var task in PickingTasks)
			{
				task.Cancel(userId, createdAt);
			}
		}

		public int GetQuantityForProduct(Guid productId)
		{
			var item = IssueItems
				.FirstOrDefault(x => x.ProductId == productId);
			return item?.Quantity ?? 0;
		}

		public void AddIssueItem(Guid productId, int quantity, DateOnly? bestBefore, DateTime createdAt)
		{
			var existing = IssueItems.FirstOrDefault(x => x.ProductId == productId);
			if (existing != null)
			{
				throw new ProductAlreadyExistDomainException(productId);
			}
			if (quantity <= 0) throw new InvalidQuantityIssueItemDomainException(quantity, Id, IssueNumber);
			var item = new IssueItem(Id, productId, quantity, bestBefore, createdAt);
			this.IssueItems.Add(item);
		}

		public void BeginAllocation()
		{
			if (IssueStatus == IssueStatus.New)
			{
				IssueStatus = IssueStatus.Pending;
			}
			if (IssueStatus != IssueStatus.Pending &&
				IssueStatus != IssueStatus.RequiresCorrection &&
				IssueStatus != IssueStatus.New)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			;
		}

		public void StartEmergencyPicking()
		{
			if (IssueStatus == IssueStatus.New)
			{
				IssueStatus = IssueStatus.Pending;
			}
			if (IssueStatus != IssueStatus.Pending &&
				IssueStatus != IssueStatus.InProgress)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			;
		}

		public void MarkAllocationCompleted(string userId)
		{
			if (IssueStatus != IssueStatus.New && IssueStatus != IssueStatus.Pending
				&& IssueStatus != IssueStatus.RequiresCorrection)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			this.ChangeStatus(IssueStatus.Pending);
			PerformedBy = userId;
			this.AddHistory(userId);
		}

		public void MarkAllocationNotCompleted(string userId)
		{
			if (IssueStatus != IssueStatus.New && IssueStatus != IssueStatus.Pending
				&& IssueStatus != IssueStatus.RequiresCorrection)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			this.ChangeStatus(IssueStatus.RequiresCorrection);
			this.AddHistory(userId);
		}

		public List<Pallet> RemoveNotLoadedPallets(string userId)
		{
			var toReturn = Pallets.Where(p => p.Status != PalletStatus.Loaded).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.DetachFromIssue(userId, pallet.Location.ToSnapshot(), ReasonForPallet.Correction);
				Pallets.Remove(pallet);
			}
			return toReturn;
		}

		public void VerifyToLoad(string userId)
		{
			if (!(IssueStatus == IssueStatus.InProgress || IssueStatus == IssueStatus.ChangingPallet
				|| IssueStatus == IssueStatus.Pending || IssueStatus == IssueStatus.PickingShortage))
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			IssueStatus = IssueStatus.ConfirmedToLoad;
			foreach (var pallet in Pallets)
			{
				pallet.AssignToIssue(Id, userId, pallet.Location.ToSnapshot());
			}
			AddHistory(userId);
		}

		public void ConfirmAfterLoading(string userId)
		{
			if (Pallets.Any(p => p.Status != PalletStatus.Loaded))
			{
				throw new NotEndedLoadingDomainException(Id, IssueNumber);
			}
			PerformedBy = userId;
			if (IssueStatus != IssueStatus.IsShipped)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			foreach (var pallet in Pallets)
			{
				pallet.ToArchive(userId, ReasonForPallet.Loaded, pallet.Location.ToSnapshot());
			}
			IssueStatus = IssueStatus.Archived;
			AddHistory(userId);
			this.AddDomainEvent(new ChangeStockNotification(CreateStockItem(Pallets.ToList())));
		}

		public void FinishIssueNotCompleted(string userId)
		{
			if (IssueStatus != IssueStatus.ConfirmedToLoad)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			PerformedBy = userId;
			foreach (var pallet in Pallets)
			{
				pallet.AddHistory(ReasonForPallet.Loaded, userId, pallet.Location.ToSnapshot());
			}
			IssueStatus = IssueStatus.IsShipped;
			AddHistory(userId);
		}

		public void Cancel(string userId)
		{
			if (IssueStatus == IssueStatus.Archived || IssueStatus == IssueStatus.IsShipped)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			IssueStatus = IssueStatus.Cancelled;
			PerformedBy = userId;
			AddHistory(userId);
		}

		public void ReplacePalletInIssue(Pallet oldPallet, Pallet newPallet, string userId, DateOnly? bestBefore)
		{
			if (IssueStatus != IssueStatus.ConfirmedToLoad &&
				IssueStatus != IssueStatus.Pending)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
			if (oldPallet.Status == PalletStatus.Loaded)
			{
				throw new PalletAlreadyLoadedDomainException(oldPallet.Id, oldPallet.PalletNumber);
			}
			if (oldPallet.Id == newPallet.Id)
			{
				throw new CannotReplaceTheSamePalletDomainException(oldPallet.Id);
			}
			if (!Pallets.Any(p => p.Id == oldPallet.Id) || oldPallet.IssueId != Id)
			{
				throw new NotBelongsToIssueDomainException(oldPallet.Id, oldPallet.PalletNumber, Id, IssueNumber);
			}
			if (!(newPallet.Status == PalletStatus.Available || newPallet.Status == PalletStatus.InStock))
			{
				throw new InvalidPalletStatusDomainException(newPallet.Id, newPallet.PalletNumber);
			}
			if (newPallet.IssueId != null)
			{
				throw new AlreadyAssignedDomainException(newPallet.Id);
			}
			if (newPallet.ProductsOnPallet.Count != 1)
			{
				throw new NotOneProductsOnPalletDomainException(newPallet.Id, newPallet.PalletNumber);
			}
			if (oldPallet.ProductsOnPallet.Count != 1)
			{
				throw new NotOneProductsOnPalletDomainException(oldPallet.Id, oldPallet.PalletNumber);
			}
			if (oldPallet.ProductsOnPallet.Single().ProductId != newPallet.ProductsOnPallet.Single().ProductId)
			{
				throw new ProductOnPalletsAreNotTheSameDomainException(oldPallet.ProductsOnPallet.Single().ProductId, newPallet.ProductsOnPallet.Single().ProductId);
			}
			if (oldPallet.ProductsOnPallet.Single().Quantity != newPallet.ProductsOnPallet.Single().Quantity)
			{
				throw new ProductOnPalletsAreNotTheSameAmountDomainException(oldPallet.ProductsOnPallet.Single().ProductId, oldPallet.ProductsOnPallet.Single().Quantity, newPallet.ProductsOnPallet.Single().Quantity);
			}
			if (bestBefore != null)
			{
				if (bestBefore > newPallet.ProductsOnPallet.Single().BestBefore)
				{
					throw new ProductOnPalletsAreNotTheSameBBDomainException(oldPallet.ProductsOnPallet.Single().ProductId, oldPallet.ProductsOnPallet.Single().BestBefore, newPallet.ProductsOnPallet.Single().BestBefore);
				}
			}
			newPallet.ReserveToIssue(Id, userId, newPallet.Location.ToSnapshot());
			this.AttachPallet(newPallet);
			oldPallet.DetachFromIssue(userId, oldPallet.Location.ToSnapshot(), ReasonForPallet.Correction);
			this.DetachPallet(oldPallet);
			var status = IssueStatus;
			IssueStatus = IssueStatus.ChangingPallet;
			PerformedBy = userId;
			AddHistory(userId);
			IssueStatus = status;
		}

		public void CompletedLoad(string userId)
		{
			foreach (var pallet in Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new NotEndedLoadingDomainException(Id, IssueNumber);
				}
			}
			IssueStatus = IssueStatus.IsShipped;
			PerformedBy = userId;
			AddHistory(userId);
		}

		public void ChangeClient(int clientId)
		{
			//
			ClientId = clientId;
		}

		public void RemovePickingTask(PickingTask pickingTask)
		{
			PickingTasks.Remove(pickingTask);
		}
		//Detach i Attach tylko dla update, changePallet - dla historii
		public void DetachPallet(Pallet pallet)
		{
			this.Pallets.Remove(pallet);
		}

		public void AttachPallet(Pallet pallet)
		{
			if (!Pallets.Contains(pallet))
			{
				this.Pallets.Add(pallet);
				pallet.ChangeStatus(PalletStatus.ToIssue);
			}
		}


		public void AttachPickingTask(PickingTask task)
		{
			this.PickingTasks.Add(task);
		}

		public void ReservePallet(Pallet pallet)
		{
			if (pallet.Status == PalletStatus.ToIssue)
				throw new AlreadyAssignedDomainException(pallet.Id);
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

		//Nowe metody
		public void EnsureCanBeCancelled()
		{
			if (IssueStatus == IssueStatus.Archived || IssueStatus == IssueStatus.Cancelled)
			{
				throw new NotAllowedOperationDomainException(Id, IssueNumber);
			}
		}

		public IssueModificationMode DetremineModificationMode()
		{
			return IssueStatus switch
			{
				IssueStatus.New or
				IssueStatus.Pending or
				IssueStatus.RequiresCorrection
				=> IssueModificationMode.Reallocation,

				IssueStatus.ConfirmedToLoad
				=> IssueModificationMode.SupplementaryIssue,

				_ => throw new NotAllowedOperationDomainException(Id, IssueNumber)
			};
		}
		public ReallocationPreparation PrepareForReallocation(int clientId, string userId, DateTime now)
		{
			this.ChangeClient(clientId);
			// Odłączamy poprzednie palety i anulujemy dotychczasowe zadania pickingu przed ponowną alokacją.
			var reusablePallets = new List<Pallet>();
			var listOldPallets = Pallets.ToList();
			foreach (var pallet in listOldPallets)
			{
				DetachPallet(pallet);
				pallet.DetachFromIssue(userId, pallet.Location.ToSnapshot(), ReasonForPallet.Correction);
				pallet.ChangeStatus(PalletStatus.LockedForIssue);// Palety pozostają zablokowane, żeby nie zostały użyte równolegle w innym zleceniu.
				reusablePallets.Add(pallet);
			}
			var listOldPickingTask = PickingTasks.ToList();
			// Anulujemy poprzednie zadania pickingu przed ponowną alokacją.
			foreach (var pickingTask in listOldPickingTask)
			{
				RemovePickingTask(pickingTask);
				pickingTask.Cancel(userId, now);
			}
			var touchedVirtualPalletIds = listOldPickingTask
				.Where(a => a.VirtualPalletId.HasValue)
				.Select(a => a.VirtualPalletId!.Value)
				.Distinct()
				.ToList();
			return new ReallocationPreparation(listOldPallets, touchedVirtualPalletIds);
		}

		public bool CompleteReallocation(List<Pallet> pallets, List<Pallet> oldPallets)
		{
			var assignedIds = pallets.Select(p => p.Id).ToHashSet();
			var returnPallets = oldPallets
				.Where(p => !assignedIds.Contains(p.Id))
				.ToList();

			foreach (var returnPallet in returnPallets)
			{
				returnPallet.ChangeStatus(PalletStatus.Available);
			}
			return true;
		}

		public void CompletePickingPlanned(bool missingQuantityAllocated, Pallet sourcePallet, string userId)
		{
			if (IssueStatus == IssueStatus.Pending)
			{
				this.ChangeStatus(IssueStatus.InProgress);
			}
			if (!missingQuantityAllocated)
			{
				this.ChangeStatus(IssueStatus.PickingShortage);
			}
			sourcePallet.ChangeStatus(PalletStatus.OnHold);
			sourcePallet.AddHistory(ReasonForPallet.Correction, userId, sourcePallet.Location.ToSnapshot());
		}
		public void CompletePicking()
		{
			if (IssueStatus == IssueStatus.Pending)
			{
				this.ChangeStatus(IssueStatus.InProgress);
			}
		}
		public void AssignPallets(IEnumerable<Pallet> pallets, string userId)
		{
			foreach (var pallet in pallets)
			{
				var snapShot = pallet.Location.ToSnapshot();
				pallet.ReserveToIssue(Id, userId, snapShot);
			}
		}
	}
}
