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
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }		
		public PalletStatus Status { get; set; } = 0; //może usunąć jeszcze trzeba sie zastanowić, decyzja biznesowa czy zawsze InStock
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new HashSet<ProductOnPalletDTO>();//tworzę paletę razem z towarem
	}
}
