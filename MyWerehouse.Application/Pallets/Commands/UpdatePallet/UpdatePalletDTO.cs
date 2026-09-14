using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.UpdatePallet
{
	public class UpdatePalletDTO 
	{
		public PalletStatus Status { get; init; } = 0;
		public required string UserId { get; init; }
		public ICollection<ProductOnPalletUpdateDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletUpdateDTO>();
	}	
}
