using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.PickingPallets.DTOs;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking
{
	public record GetListIssueToPickingQuery(DateTime DateIssueStart, DateTime DateIssueEnd):IRequest<List<PickingGuideLineDTO>>;
	
}
