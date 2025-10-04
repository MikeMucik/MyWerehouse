using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IIssueService
	{
		Task<List<IssueResult>> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend);
		//Task AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product);		
		Task<UpdateIssueDTO> GetIssueByIdAsync(int numberIssue);		
		Task<List<IssueResult>> UpdateIssueAsync(UpdateIssueDTO issueDTO);
		Task DeleteIssueAsync(int issueId);//warunki				
		Task VerifyIssueToLoadAsync(int issueId, string userId);
		Task<ListPalletsToLoadDTO> LoadingIssueAsync(int issueId, string sendedBy);
		Task MarkAsLoadedAsync(string palletId, string sendedBy);
		Task FinishIssueNotCompleted(int issueId, string performedBy);
		//możesz zakończyć załadunek gdy wszystkie nie załadowane, bo biuro zamknie bez pełnego załadunku
		Task CompletedIssueAsync(int issueId, string confirmedBy);
		//zrobienie kompletacji x1
		Task VerifyIssueAfterLoadingAsync(int issueId, string verifyBy);
		Task ChangePalletDuringLoadingAsync(int issueId, string oldPalletId, string newPalletId, string performedBy);
		Task<IssuePalletsWithLocationDTO> PalletsToTakeOffList(int issueId, string userId);
		//TODO: całkowite wycofanie zamówienia:
		// - usunięcie z palet z issue 
		// - zmiana statusów pale
		// - wycofanie alokacji jeśli są nie wykonane
		// - anulacja palet kompletacyjnych -> łączenie po dacie i towarze z innymi - sugestia
	}
}
