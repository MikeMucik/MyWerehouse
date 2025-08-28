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
		Task<int> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, string userId, DateTime dateToSend);		
		Task AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product, string userId);		
		Task <IssueToUpdateDTO> GetIssueByIdAsync(int numberIssue);
		Task UpdateIssueAsync(int numberIssue, string perfomedBy, ListProductsOfIssue products, DateTime dateToSend);		
		Task DeleteIssueAsync(int issueId);//warunki				
		Task VerifyIssueToLoadAsync(int issueId, string userId);		
		Task<ListPalletsToLoadDTO> LoadingIssueAsync(int issueId, string sendedBy);
		Task MarkAsLoadedAsync(string palletId, string sendedBy);
		Task FinishIssueNotCompleted(int issueId,string performedBy);
		//możesz zakończyć załadunek gdy wszystkie nie załadowane, bo biuro zamknie bez pełnego załadunku
		Task CompletedIssueAsync(int issueId, string confirmedBy);
		//zrobienie kompletacji x1
		Task VerifyIssueAfterLoadingAsync(int issueId, string verifyBy);
		Task ChangePalletDuringLoadingAsync(int issueId,string oldPalletId, string newPalletId, string performedBy);		
		Task <IssuePalletsWithLocationDTO> PalletsToTakeOffList(int issueId, string userId);
		
		// zebranie całych palet i z pickingu
	}
}
