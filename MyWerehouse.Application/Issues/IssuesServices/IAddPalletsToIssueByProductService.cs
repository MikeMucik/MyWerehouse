using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public interface IAddPalletsToIssueByProductService
	{
		Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product);
	}
}
