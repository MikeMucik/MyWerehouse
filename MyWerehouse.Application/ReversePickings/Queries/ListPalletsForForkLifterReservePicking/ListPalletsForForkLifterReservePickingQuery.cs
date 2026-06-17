using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{
	public record ListPalletsForForkLifterReservePickingQuery(DateOnly Start, DateOnly End)
		:IRequest<AppResult<List<PickingPalletWithLocationDTO>>>;
}
