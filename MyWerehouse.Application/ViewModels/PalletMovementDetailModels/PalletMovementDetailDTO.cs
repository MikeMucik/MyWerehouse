using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.PalletMovementDetailModels
{
	public class PalletMovementDetailDTO : IMapFrom<PalletMovementDetail>
	{
		public int Id { get; set; }
		public int PalletMovementId { get; set; }		
		public int ProductId { get; set; }		
		public int QuantityChange { get; set; } //+/-
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PalletMovementDetail, PalletMovementDetailDTO>()
				.ForMember(dest=>dest.QuantityChange, opt=>opt.MapFrom(src=>src.Quantity));
		}
	}
}
