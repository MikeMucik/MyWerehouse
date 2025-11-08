using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.HistoryDTO;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IHistoryService
	{
		Task <PalletHistoryDTO> GetHistoryPalletByIdAsync(string  id);
		Task <ReceiptHistoryDTO> GetHistoryReceiptByIdAsync(string  id);
		Task <IssueHistoryDTO> GetHistoryIssueByIdAsync(string  id);
	}
}
