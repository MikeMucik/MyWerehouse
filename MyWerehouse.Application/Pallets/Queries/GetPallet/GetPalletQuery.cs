using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pallets.Queries.GetPallet
{
	public record GetPalletQuery(Guid Id) : IRequest<AppResult<PalletDTO>>;	
}
