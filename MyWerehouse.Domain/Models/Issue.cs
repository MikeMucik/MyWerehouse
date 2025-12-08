using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Domain.Models
{
	public class Issue
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; } 
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryIssue> HistoryIssues { get; set; } = new List<HistoryIssue>();
		public virtual ICollection<HistoryPicking> HistoryPickings { get; set; } = new List<HistoryPicking>();
		public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
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

		public List<Pallet> RemoveNotLoadedPallets()
		{
			var toReturn = Pallets.Where(p => p.Status != PalletStatus.Loaded).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.Status = PalletStatus.Available;
				pallet.IssueId = null;
				Pallets.Remove(pallet);
			}
			return toReturn;
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