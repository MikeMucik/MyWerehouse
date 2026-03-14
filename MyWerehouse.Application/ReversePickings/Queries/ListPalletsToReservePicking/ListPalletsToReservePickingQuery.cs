using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsToReservePicking
{
	public record ListPalletsToReservePickingQuery(DateOnly Start, DateOnly End):IRequest<AppResult<List<PalletWithLocationDTO>>>;
}
