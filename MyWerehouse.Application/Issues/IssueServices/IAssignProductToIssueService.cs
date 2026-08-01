using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public interface IAssignProductToIssueService
	{
		Task<AssignProductToIssueResult> AssignGoodsToIssue(Issue issue, IssueItemDTO issueItem, IssueAllocationPolicy policy,
		List<Pallet>? oldAssignedPallets, string userId);
	}
}
