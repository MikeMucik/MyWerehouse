using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public record PalletsToTakeOffListQuery(Guid IssueId, string UserId):IRequest<AppResult<IssuePalletsWithLocationDTO>>;	
}
