using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Pallets.Queries.GetPallet;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber
{
	public record GetPalletByPalletNumberQuery(string PalletNumber)
		:IRequest<AppResult<PalletSimplyDTO>>;
	
}
