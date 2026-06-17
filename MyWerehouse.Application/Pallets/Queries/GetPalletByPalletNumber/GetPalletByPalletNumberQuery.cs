using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Queries.GetPallet;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletBySKU
{
	public record GetPalletByPalletNumberQuery(string palletNumber) :IRequest<AppResult<PalletDTO>>;
	
}
