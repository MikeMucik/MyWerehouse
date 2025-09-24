using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.PickingPalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPickingPalletService
	{
		Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd);
		Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje pojedyncze alokacje
		Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje według klienta -> drzewko		
		Task <List<AllocationDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate);
		Task DoPickingAsync(AllocationDTO allocationDTO, string userId);
		//Ręczny picking, klikasz paletę musi ona mieć status ToPicking, z palety pobierany jest ProductId
		//wstawiasz numer IssueId, powinno dać ile dla tego zlecenia jeszcze trzeba pobrać tego produktu
		//Czyli wyszukać w alokacjach po issueId i ProductId ile quantity potrzeba 
		Task<ManualPickingResult> DoManualPickingAsync(string palletId, int? IssueId, string userId);


		Task<ManualPickingResult> PrepareManualPickingAsync(string palletId);
		Task<ManualPickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId);
	}
}
