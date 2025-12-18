using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public record CreateNewPalletCommand(PalletDTO DTO, string UserId) : IRequest<Pallet>;
}
