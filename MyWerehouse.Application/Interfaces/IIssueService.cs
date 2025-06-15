using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IIssueService
	{
		void AddIssue(int clientId, string perfomedBy, List<IssueItemDTO> values);
		Task AddIssueAsync(int clientId, string perfomedBy, List<IssueItemDTO> values);
		void UpdateIssue(int clientId, string perfomedBy, List<IssueItemDTO> values);
		void DeleteIssue(int issueId);

		List<Pallet> SelectPalletsForIssue(IQueryable<Pallet> pallet, int quantity);
		Task<List<Pallet>> SelectPalletsForIssueAsync(IQueryable<Pallet> pallet, int quantity);

		void LoadingIssue(int clientId, int issueId, string sendedBy);
		void CompletedIssue(int clientId, int issueId, string confirmedBy);
	}
}
