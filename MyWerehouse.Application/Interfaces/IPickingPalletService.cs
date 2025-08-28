using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.PickingPalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPickingPalletService
	{
		Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd);
		Task<List<ProductToIssueDTO>> GetListToPicking(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje pojedyncze alokacje
		Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje według klienta -> drzewko		
		Task <List<AllocationDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate);
		Task DoPickingAsync(AllocationDTO allocationDTO, string userId);
		// pobierz palety pickingPallet by wszystko było wykonywane w pamięci, do momentu tworzenia, aktualizacji palety kompletacyjnej i aktualizacji palety wziętej do  -> zrób dictionary
		// wprowadź za pomocą skanera numer palety, dostajesz informacje co masz zrobić, jeśli paleta komoletacyjna jeszcze nie istnieje, stwórz paletę
		// dodaj towar do palety, jeśli taki towar już jest to zwiększ quantity(sprawdź daty na wszelki wypadek)

	}
}
