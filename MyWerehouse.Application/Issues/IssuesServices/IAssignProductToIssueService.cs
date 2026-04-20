using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public interface IAssignProductToIssueService
	{
		Task<AssignProductToIssueResult> AssignProductToIssue(Issue issue, IssueItemDTO product, IssueAllocationPolicy policy, List<Pallet> reusablePalletsForProduct, string userId);
	}
}
