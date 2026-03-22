using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public record GetPalletToEditQuery(Guid PalletId):IRequest<AppResult<UpdatePalletDTO>>;
}
