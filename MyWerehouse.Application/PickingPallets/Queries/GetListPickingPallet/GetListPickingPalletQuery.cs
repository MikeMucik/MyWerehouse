using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.PickingPallets.DTOs;

namespace MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet
{
	public record GetListPickingPalletQuery(DateTime DateMovedStart, DateTime DateMovedEnd): IRequest<List<PickingPalletWithLocationDTO>>;
	
}
