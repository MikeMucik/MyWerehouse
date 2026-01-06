using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Filters;

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public record FindPalletsByFiltrQuery(PalletSearchFilter Filter):IRequest<List<PalletDTO>>;	
}
