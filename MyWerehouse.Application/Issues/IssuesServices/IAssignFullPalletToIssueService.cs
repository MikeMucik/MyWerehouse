using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public interface IAssignFullPalletToIssueService
	{
		Task AddPallets(Issue issue, List<Pallet> pallets);
	}
}
