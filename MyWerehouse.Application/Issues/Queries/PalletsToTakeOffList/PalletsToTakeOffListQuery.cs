using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public record PalletsToTakeOffListQuery(int IssueId, string UserId):IRequest<IssuePalletsWithLocationDTO>;
	
}
