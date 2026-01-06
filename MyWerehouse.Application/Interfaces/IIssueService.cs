using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IIssueService
	{
		Task<List<IssueResult>> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend);
		//Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product);		//don't remove
		Task<UpdateIssueDTO> GetIssueByIdToUpdateAsync(int numberIssue);		
		Task<IssueDTO> GetIssueByIdAsync(int numberIssue);		
		Task<List<IssueResult>> UpdateIssueAsync(UpdateIssueDTO issueDTO, DateTime DateToSend);
		Task<IssueResult> DeleteIssueAsync(int issueId, string userId);//only notConfirmedToLoad	

		//TODO: całkowite wycofanie zamówienia:
		// - usunięcie z palet z issue 
		// - zmiana statusów palet
		// - wycofanie alokacji jeśli są nie wykonane - jeśli wykonane zadanie dekompletacyjne
		// - anulacja palet kompletacyjnych -> łączenie po dacie i towarze z innymi - sugestia			
		Task<IssueResult> CancelIssueAsync(int issueId, string userId);//for confirmed				
		Task<IssueResult> VerifyIssueToLoadAsync(int issueId, string userId);
		Task<ListPalletsToLoadDTO> LoadingIssueListAsync(int issueId, string userId);
		Task<IssueResult> MarkAsLoadedAsync(string palletId, string sendedBy);
		Task<IssueResult> FinishIssueNotCompleted(int issueId, string performedBy);
		//możesz zakończyć załadunek gdy wszystkie nie załadowane, bo biuro zamknie bez pełnego załadunku
		Task<IssueResult> CompletedIssueAsync(int issueId, string confirmedBy);
		//zrobienie kompletacji x1
		Task<IssueResult> VerifyIssueAfterLoadingAsync(int issueId, string verifyBy);
		Task<IssueResult> ChangePalletInIssueAsync(int issueId, string oldPalletId, string newPalletId, string performedBy);
		Task<IssuePalletsWithLocationDTO> PalletsToTakeOffListAsync(int issueId, string userId);
		Task<List<IssueDTO>> GetIssuesByFiltrAsync(IssueReceiptSearchFilter filter);
		
	}
}
