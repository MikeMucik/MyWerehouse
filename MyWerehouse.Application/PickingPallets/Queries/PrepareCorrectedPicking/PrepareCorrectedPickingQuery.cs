using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.PickingPallets.Queries.PrepareCorrectedPicking
{
	public record PrepareCorrectedPickingQuery(string PalletId):IRequest<PrepareCorrectedPickingResult>;	
}
