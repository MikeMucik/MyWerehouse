using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreatePalletDTO
	{
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }		
		public PalletStatus Status { get; init; } = 0; 
		public required string UserId { get; init; }
		public ICollection<ProductOnPalletCreateDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletCreateDTO>();//tworzę paletę razem z towarem
	}
}
