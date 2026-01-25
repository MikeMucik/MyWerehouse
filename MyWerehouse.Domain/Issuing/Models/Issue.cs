using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Issuing.Models
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