using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Application.ViewModels.AddressModels;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class DetailsOfClientDTO 
	{
		public int Id { get; init; }
		public string Name { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string Description { get; init; } = string.Empty;
		public IReadOnlyList<AddressDTO> Addresses { get; init; } = new List<AddressDTO>();
		public IReadOnlyList<ReceiptSimplyDTO> Receipts { get; init; } = new List<ReceiptSimplyDTO>();
		public IReadOnlyList<IssueSimplyDTO> Issues { get; init; } = new List<IssueSimplyDTO>();
		
	}
}
