using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public interface IComparePlanToPreparedService
	{
		Task<ComparePlanToPreparedResult> ComparePlanToPrepared(Guid issueId, Guid ProductId);
	}
}
