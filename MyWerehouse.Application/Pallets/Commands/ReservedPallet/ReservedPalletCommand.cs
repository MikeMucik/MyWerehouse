using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.ReservedPallet
{
	public record ReservedPalletCommand(int ProductId, DateOnly? BestBefore) :IRequest<Pallet?>;	
}
